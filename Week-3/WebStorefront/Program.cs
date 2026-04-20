using Microsoft.EntityFrameworkCore;
using ProductCatalog.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(); // Adding Swagger

//Once we've created our DbContext class, we've added our connection string (with password!) to 
// appsettings.development.json, and we brought our models in (or created them!)
// we register our dbcontext with the builder.
builder.Services.AddDbContext<AppDbContext>(options => 
    //Here is where we tell EF Core 2 things: 
    //  1. What type of db provider are we using, for me its MS SQL Server
    //  2. Where is the server? (connection string)
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

//Once we have things like our DbContext, our Services, etc 
//We will register them here, using builder.Services (or some specialty methods for things
// like a dbcontext)

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();

