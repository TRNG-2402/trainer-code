var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(); // Adding Swagger

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
