using System.Text.Json;
using System.Net;

namespace ProductCatalog.Middleware;

// Custom middleware sounds fancy, but its just a class. 
// Middlware classes have a specific "shape" 
// 1. A constructor that accepts a "RequestDelegate"
// 2. A public InvokeAsync(HttpContext context) method 
// ASP.NET Core discovers our middlware when we call app.UseMiddleware<T>() 
// in the app section below the builder area in Program.cs

public class GlobalExceptionMiddleware
{
    
    private readonly RequestDelegate _next; 

    // Our constructor 
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // InvokeAsync method
    public async Task InvokeAsync(HttpContext context)
    {
        //HttpContext - Holds EVERYTHING about a given HTTP Request AND its response
        try
        {
            // Pass the request along to the next piece of middlware
            // (eventually) it will reach our controllers. If anything inside our Controller,
            // Service, or Repo/Data layers throws an exception - it'll bubble back up here. 
            await _next(context);
        }
        catch (Exception e)
        {
            // Here, we will call our logic for handling/routing given specific Exception types
            // to their correct status codes 
            await HandleExceptionAsync(context, e);
        }
    }

    // HandleExceptionAsync
    // This method will contain the "logic" for my middleware
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        
        // Create some way to map specific exception types to HTTP status codes
        // As we grow our app, test it more with a front end, etc - we may encounter 
        // exceptions we didn't forsee - and that's okay. But as we find them, we can
        // come back and handle them here in one centralized place. 
        // For the sake of demo, we will use a Switch Expression - just another way to write a switch
        // If we wanted to, we could use a traditional switch statement, we could use a series of if statements
        
        int statusCode;

        // Changing to traditional switch - less "magic"
        switch (ex)
        {
            case KeyNotFoundException _: // Since we're switching off of the "type" of the ex object
                statusCode = 404; // rather than like a specific value of a property, we use the _ 
                 // to tell the compiler "evaluate the type of ex, and compare that"
                break;                  // this is called a discard - we dont want to actually act on the variable
            case ArgumentOutOfRangeException _:
                statusCode = 400;
                break;
            case ArgumentException _:
                statusCode = 400;
                break;
            case NullReferenceException _:
                statusCode = 404;
                break;
            case UnauthorizedAccessException _:
                statusCode = 401;
                break;
            default:
                statusCode = 500; //500 error
                break;
        }

        // Set up our response using the Response object that belongs to context
        // context.Response.StatusCode, context.Response.ContentType (we want to send back JSON)
        context.Response.ContentType = "application/json"; // explicitly set the content type as json
        context.Response.StatusCode = statusCode;

        // Create a body, serialized as JSON
        var body = JsonSerializer.Serialize(new
        {
            status = statusCode,
            message = ex.Message
        }); // We can create an object inline to then pass to JsonSerializer.Serialize() to be 
        // serialized as JSON

        //Use the response to return the message back to the user in a way that they can see
        await context.Response.WriteAsync(body);

    }

}