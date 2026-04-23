

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


        // Set up our response using the Response object that belongs to context
        // context.Response.StatusCode, context.Response.ContentType (we want to send back JSON)
        

        // Create a body, serialized as JSON

        //Use the response to return the message back to the user in a way that they can see


    }

}