using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;
using ProductCatalog.Middleware;
using ProductCatalog.Services;
// New using statements from auth self-guided demo
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste the JWT from /api/Auth/login. No 'Bearer ' prefix needed."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement                     
    {                                                                                        
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }            
    }); 
});

// Lets add our caching - we'll start with ResponseCaching
builder.Services.AddResponseCaching();

// Now lets add MemoryCache
builder.Services.AddMemoryCache(); // caching on server memory! Can be (often is) used alongside Response caching

//Adding our Authorization schema here
// Authentication: register the JWT bearer scheme and tell it how to validate tokens.
string jwtKey      = builder.Configuration["Jwt:Key"]!;
string jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
string jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtIssuer,

        ValidateAudience         = true,
        ValidAudience            = jwtAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),

        ValidateLifetime         = true,         // enforce the exp claim
        ClockSkew                = TimeSpan.Zero // strict - no 5-min grace
    };
});

builder.Services.AddAuthorization();

//Once we've created our DbContext class, we've added our connection string (with password!) to 
// appsettings.development.json, and we brought our models in (or created them!)
// we register our dbcontext with the builder.
builder.Services.AddDbContext<AppDbContext>(options => 
    //Here is where we tell EF Core 2 things: 
    //  1. What type of db provider are we using, for me its MS SQL Server
    //  2. Where is the server? (connection string)
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

// Category stuff
builder.Services.AddScoped<ICategoryService, CategoryService>(); // Adding the service layer class
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>(); // Adding the data layer class

// Product stuff
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();

// Auth layer
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Once we have things like our DbContext, our Services, etc 
//We will register them here, using builder.Services (or some specialty methods for things
// like a dbcontext)

var app = builder.Build();

// Telling app to use our middleware
// This middleware runs first - why?
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


//Adding swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Serves the JSON at /swagger/v1/swagger.json
    app.UseSwaggerUI(); // Serves the UI at /swagger
}

app.UseResponseCaching(); // This must come before UseAuthorization + MapControllers - why?
// Everything that runs after like 62, can contribute to the response. 


app.UseAuthentication();   // NEW - must run BEFORE UseAuthorization

app.UseAuthorization();

app.MapControllers();

app.Run();

