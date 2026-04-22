# QC 2 (REST + ASP.NET Core + EF Core) Criteria

## EF Core

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Create an EF Core model using EF Core code conventions. | `public class User { public int Id { get; set; } public string Name { get; set; } }` (The `Id` property automatically becomes the primary key). |
| Must know | Use Entity Framework to generate a SQL schema with a code-first approach. | `dotnet ef migrations add InitialCreate`<br>`dotnet ef database update` |
| Must know | Use Entity Framework to generate classes from a data source with a data-first approach. | `dotnet ef dbcontext scaffold "Server=...;Database=...;" Microsoft.EntityFrameworkCore.SqlServer -o Models` |
| Must know | Explain the role of the DbContext class and how it manages database interactions. | Acts as the primary class that coordinates Entity Framework functionality for a given data model, establishing a session with the database and serving as a Unit of Work. |
| Must know | Explain how Entity Framework tracks changes in entities and persists them to the database. | EF relies on a `ChangeTracker` to monitor entity states (Added, Unchanged, Modified, Deleted). When `SaveChanges()` is invoked, it generates and executes the required DML statements. |
| Must know | Describe the differences between the code-first and data-first approaches and when each should be applied. | Code-first uses C# classes to generate the database schema (ideal for new applications). Data-first uses an existing database schema to generate C# classes (ideal for integrating with legacy databases). |
| Should know | Create a dbcontext object in an application, and use it to manage persistance to a database. | `using (var context = new AppDbContext()) { context.Users.Add(newUser); context.SaveChanges(); }` |
| Should know | Configure a model using Data Annotations in the model class. | `[Table("Staff")] public class Employee { [Key] public int EmpId { get; set; } [Required, MaxLength(50)] public string Name { get; set; } }` |
| Should know | Effectively manages migrations to avoid breaking changes and data loss. | Reviewing generated `Up()` and `Down()` methods to ensure columns are not unexpectedly dropped, and supplying default values when adding non-nullable columns to populated tables. |
| Should know | Describe the role of the Fluent API and when it is required instead of Data Annotations. | Overrides conventions via the `OnModelCreating` method. Required for complex configurations that annotations cannot handle, such as composite primary keys or precise relationship mapping. |
| Nice to Have | Call stored procedures and query scalar types by dropping down to SQL, using FromSQL() and SqlQuery(). | `var activeUsers = context.Users.FromSqlRaw("EXEC GetActiveUsers").ToList();` |
| Nice to Have | Modify a migration created by Entity Framework before execution. | Manually editing the `<timestamp>_Migration.cs` file to insert custom logic, such as `migrationBuilder.Sql("UPDATE Table SET Column = 'Default'");`, prior to running the update command. |

## REST

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe the purpose of REST and RESTful design. | An architectural style for distributed hypermedia systems that enforces a stateless, cacheable, and uniform interface for communication between clients and servers. |
| Must know | Describe the purpose of HTTP messaging. | The foundational protocol of the World Wide Web, defining the structure and transmission mechanisms for requests and responses between a client and a server. |
| Must know | Describe the HTTP request/response lifecycle. | Client establishes a TCP connection, sends an HTTP request (method, URI, headers, body), server processes the request, server returns an HTTP response (status code, headers, body), and the connection is closed or kept alive. |
| Must know | Describe HTTP request methods (verbs). | GET (retrieve resource), POST (create resource), PUT (replace resource entirely), PATCH (update resource partially), DELETE (remove resource). |
| Must know | Describe HTTP response code classes (1xx,2xx,3xx,etc.) | 1xx: Informational; 2xx: Success (e.g., 200 OK); 3xx: Redirection (e.g., 301 Moved Permanently); 4xx: Client Error (e.g., 404 Not Found); 5xx: Server Error (e.g., 500 Internal Server Error). |
| Must know | Describe the REST principles. | Client-Server architecture, Statelessness, Cacheability, Layered System, Uniform Interface, and Code on Demand (optional). |
| Must know | Be capable of sending a GET request to an open source REST API using curl or Postman | `curl -X GET https://api.example.com/v1/users` |
| Should know | Describe URL conventions when using REST. | Endpoints should use plural nouns representing resources and hierarchical structures without verbs. Example: `/users/123/orders/456` |
| Should know | Describe SOA (Service Oriented Architecture) and be capable of diagramming the components of a sample system | An enterprise architectural pattern where distinct, loosely coupled services communicate across a network to provide functional components to applications. |
| Should know | Describe the difference between authorization and authentication. | Authentication validates identity (verifying who the user is). Authorization validates permissions (verifying what the authenticated user is permitted to do). |
| Should know | Be capable of sending a POST request to a REST API using curl or Postman and populating the request body | `curl -X POST https://api.example.com/v1/users -H "Content-Type: application/json" -d '{"name":"Jane", "role":"admin"}'` |
| Should know | Build a RESTful web service using a popular framework (e.g. Spring, Flask, Express) | `app.get('/users/:id', (req, res) => { res.status(200).json({ id: req.params.id }); });` |
| Nice to Have | Implement authentication and authorization using a popular RESTful framework (e.g. OAuth, JWT) | Securing endpoints by requiring and verifying a JSON Web Token passed via headers: `Authorization: Bearer <token>` |
| Nice to Have | Compare and contrast RESTful and SOAP-based web services in terms of functionality, performance, and scalability | REST utilizes standard HTTP methods, lightweight JSON payloads, and scales easily. SOAP is a strict protocol using heavy XML payloads, offering built-in stateful operations and WS-Security, but with higher bandwidth overhead. |

## ASP.NET Core

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe the HTTP pipeline | The sequence of middleware components in ASP.NET Core that process incoming HTTP requests and generate outgoing HTTP responses. |
| Must know | Implement controllers and action methods. | `[ApiController] [Route("api/[controller]")] public class UsersController : ControllerBase { [HttpGet] public IActionResult Get() { return Ok(); } }` |
| Must know | Describe the purpose and types of HTTP response codes. | Standardized numerical codes indicating request outcomes. 2xx (Success), 3xx (Redirection), 4xx (Client Error), 5xx (Server Error). |
| Must know | Implement an API service. | Creating a RESTful interface using ASP.NET Core that exposes endpoints for client consumption. |
| Must know | Demonstrate functional kowledge of Data Transfer Objects, and their use. | Objects used to encapsulate data and send it over the network, decoupling the API contract from the internal database domain models. |
| Must know | Describe and implement a Minimal API endpoint. | `app.MapGet("/users", () => new[] { "Alice", "Bob" });` |
| Must know | Describe the function of HTTP method annotations. | Attributes like `[HttpGet]` or `[HttpPost]` that map incoming HTTP verbs to specific controller action methods. |
| Should know | Implement model binding effectively. | Automatically mapping HTTP request data (route parameters, query strings, body) to C# action method parameters. `public IActionResult Create([FromBody] UserDto user)` |
| Should know | Describe and implement data validation using annotations. | Using attributes to enforce rules. `public class UserDto { [Required] [StringLength(50)] public string Name { get; set; } }` |
| Should know | Demonstrate the use of automatic mapping for objects and DTOs. | Utilizing libraries like AutoMapper to seamlessly translate between database entities and DTOs. `var userDto = _mapper.Map<UserDto>(userEntity);` |
| Should know | Implement native ASP.NET middleware, such as Logging or Identity. | Registering built-in middleware in the request pipeline: `app.UseAuthentication(); app.UseAuthorization();` |
| Should know | Implement HTTP response codes effectively. | Returning appropriate codes based on context: `return NotFound();` (404) or `return CreatedAtAction(nameof(Get), new { id = user.Id }, user);` (201). |
| Nice to Have | Implements third party or custom filters and middleware. | Creating a custom `ExceptionMiddleware` to catch unhandled exceptions globally and return standardized JSON error responses. |
| Nice to Have | Demonstrate understanding of caching in an API. | Utilizing `[ResponseCache(Duration = 60)]` or implementing `IDistributedCache` (e.g., Redis) to store frequent query results and reduce database load. |
| Nice to Have | Implement an API which consumes a 3rd party API. | Registering `IHttpClientFactory` and creating a typed client to fetch data from an external service within your own API endpoint. |
