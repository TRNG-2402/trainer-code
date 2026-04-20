# QC 1 (.NET + SQL) Criteria

## C#/.NET

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Utilize the dotnet command line tools to generate and execute projects. | `dotnet new console -n MyApp`<br>`dotnet run` |
| Must know | Create methods that allow for the reusability of code. | `public int Add(int a, int b) { return a + b; }` |
| Must know | Describe the purpose and structure of a unit test. | Ensures individual units of code work as expected; typically structured using the Arrange, Act, Assert (AAA) pattern. |
| Must know | Create a functional application to fulfill behavioural requirements and user stories. | Developing a complete program that meets specific acceptance criteria (e.g., processing user input to yield a specified result). |
| Must know | Can describe the .NET compilation process and its steps. | Source code (.cs) compiles into Intermediate Language (IL), which the CLR's Just-In-Time (JIT) compiler then converts to native machine code at runtime. |
| Must know | Demonstrate proper syntax for working with arrays. | `int[] numbers = new int[3] { 1, 2, 3 };` |
| Must know | Effectively interprets stack traces in order to debug code files. | Reading exception output from top to bottom to identify the specific file, method, and line number where a crash originated. |
| Must know | Utilizes control flow where appropriate to achieve desired behavior during runtime. | `if (condition) { /* do A */ } else { /* do B */ }` |
| Must know | Uses Try-Catch-Finally to avoid hard crashing when running "risky" operations. | `try { /* network call */ } catch (Exception ex) { Console.WriteLine(ex); } finally { /* cleanup */ }` |
| Must know | Understands and can explain the four pillars of OOP. | Encapsulation (data hiding), Inheritance (code reuse), Polymorphism (method overriding/overloading), and Abstraction (hiding implementation complexity). |
| Must know | Can describe the role of the .NET SDK and its use in development. | A bundle of tools, libraries, and compilers (including the `dotnet` CLI) required to build and test .NET applications. |
| Must know | Can initialize and run a console application using the .NET CLI. | `dotnet new console`<br>`dotnet run` |
| Must know | Can identify and use basic data types appropriately. | `int age = 30; string name = "John"; bool isValid = true;` |
| Must know | Can use basic, comparison, equality, and logical operators in programming logic. | `if (age >= 18 && hasID == true)` |
| Must know | Can differentiate between value and reference types, and describe stack vs. heap allocation. | Value types (e.g., int, struct) hold data directly on the stack. Reference types (e.g., string, class) hold a memory address on the stack that points to the data on the heap. |
| Must know | Can model real-world entities using classes, fields, methods, and constructors. | `public class Car { public string Make; public Car(string m) { Make = m; } }` |
| Must know | Describe the purpose of a design pattern. | A standardized, reusable solution to a commonly occurring problem within a given context in software design. |
| Must know | Describe the purpose and differences of collections. | Data structures used to store objects. Arrays have fixed sizes, Lists are dynamic, and Dictionaries store key-value pairs for fast lookups. |
| Must know | Implement type conversion in an application. | `int number = Convert.ToInt32("123");` |
| Must know | Describe and utilize SOLID principles in application design. | Five architectural principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion) to ensure maintainability. |
| Must know | Utilize the HttpClient object to make HTTP calls to external APIs. | `using var client = new HttpClient(); var data = await client.GetStringAsync("https://api.example.com");` |
| Must know | Demonstrates understanding of data structures and collections in C#. | Knowing when performance dictates the use of a `Queue<T>` (FIFO), `Stack<T>` (LIFO), or `HashSet<T>` (unique elements). |
| Must know | Demonstrate understand of Generic Types in C#. | `public class Box<T> { public T Content { get; set; } }` |
| Must know | Program asynchronously in C# using async and await. | `public async Task<string> FetchAsync() { await Task.Delay(1000); return "Done"; }` |
| Must know | Describe the functionality of different sorting algorithms. | Understanding the mechanics and performance of approaches like Bubble Sort (swapping adjacent), Merge Sort (divide and conquer), and Quick Sort (pivot partitioning). |
| Must know | Understand and discuss Service Oriented Architecture and Microservices. | Architectural styles that structure an application as a collection of loosely coupled, independently deployable services. |
| Must know | Describe asymptotic and Big-O notation, and how application logic can be written efficiently. | Mathematical notation defining algorithmic complexity (e.g., O(1) for constant time, O(N) for linear time, O(N^2) for quadratic). |
| Should know | Use encapsulation and abstration in applications, with appropriate access modifiers and modifiers on classes and methods. | `private int _age; public void SetAge(int age) { if (age > 0) _age = age; }` |
| Should know | Use inheritance and polymorphism to create classes that have inherited members, and overrides or overloads members as necessary. | `public class Dog : Animal { public override void Speak() { Console.WriteLine("Bark"); } }` |
| Should know | Leverages manually thrown exceptions and bubbling to debug business logic. | `if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null");` |
| Should know | Understands auto-property syntax for class fields. | `public string Name { get; set; }` |
| Should know | Can describe the difference between an Interface and Abstract class, and can appropriately leverage either as needed in their program. | Interfaces define a contract without implementation (multiple interfaces allowed per class). Abstract classes can contain implemented logic but cannot be directly instantiated. |
| Should know | Understands the Primary Constructor syntax for classes. | `public class User(string Name, int Age);` |
| Should know | Can explain how garbage collection works in .NET and avoid common memory leaks. | The CLR periodically reclaims memory from objects no longer in use. Leaks are avoided by unsubscribing from events and disposing of unmanaged resources. |
| Should know | Uses the NuGet Package Manager to install and manage dependencies. | `dotnet add package Newtonsoft.Json` |
| Should know | Can organize applications using solutions and multi-project setups. | Creating a `.sln` file to manage multiple related `.csproj` files (e.g., separating Web API, Business Logic, and Tests). |
| Should know | Can differentiate between static and instance members and explain when to use each. | Static members belong to the class itself and are shared globally; instance members belong to a specific instantiated object. |
| Should know | Describe the repository design pattern. | An abstraction layer between the data access logic and the business logic, providing a centralized way to query the database. |
| Should know | Describe the singleton design pattern. | Restricts the instantiation of a class to one single instance globally and provides a static access point to it. |
| Should know | Describe the unit-of-work design pattern. | Manages a set of operations that alter the database and ensures they are committed as a single transaction. |
| Should know | Implement nullable types in an application. | `int? age = null; if (age.HasValue) { /* process */ }` |
| Should know | Utilize the collections namespace, and types that extend the IEnumerable interface in an application. | `IEnumerable<int> numbers = new List<int> { 1, 2, 3 };` |
| Should know | Implement the Repository pattern in an application. | `public class UserRepository : IUserRepository { public User Get(int id) { /* Db call */ } }` |
| Should know | Implement sorting algorithms in an application. | Constructing a custom `for` loop iteration logic to sort an array, or utilizing built-in methods like `Array.Sort()`. |
| Should know | Implement lambda expressions in an application. | `Func<int, int> square = x => x * x;` |
| Nice to Have | Can create and use their own reusable utility or helper class library. | Abstracting shared string manipulation logic into a `StringUtils` class located in a separate `.dll` for use across multiple applications. |
| Nice to Have | Able to simulate multiple inheritance. | Implementing multiple interfaces on a single class (e.g., `public class Drone : IFlyable, ICamera`). |
| Nice to Have | Can create custom exceptions in their applications to fit specific use cases. | `public class UserNotFoundException : Exception { public UserNotFoundException(string msg) : base(msg) {} }` |
| Nice to Have | Can construct and use class libraries and reference them in multi-project solutions. | `dotnet add reference ../MyLibraryProject/MyLibrary.csproj` |
| Nice to Have | Can use partial classes to split functionality across multiple files. | `public partial class Employee { }` // File 1<br>`public partial class Employee { }` // File 2 |
| Nice to Have | Can apply sealed classes to enforce class design decisions. | `public sealed class SecurityConfig { }` // Prevents other classes from inheriting from this class. |
| Nice to Have | Implement implicit typing using "var." | `var user = new User();` |
| Nice to Have | Implement a lambda expression to perform filters and sorts. | `var activeUsers = users.Where(u => u.IsActive).OrderBy(u => u.Name);` |
| Nice to Have | Demonstrate the implementation of a design pattern. | Implementing a thread-safe Singleton using `Lazy<T>` to guarantee initialization occurs only once. |
| Nice to Have | Demonstrate an understanding of REGEX and pattern matching syntax. | `bool isValid = Regex.IsMatch(input, @"^\d{3}-\d{2}-\d{4}$");` |
| Nice to Have | Implement recursion in an application. | `public int Factorial(int n) => n == 0 ? 1 : n * Factorial(n - 1);` |

## SQL

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Articulate the reasons for using a Relational Database Model to represent data. | Provides structured data organization, enforces data integrity, supports complex queries via SQL, and reduces data redundancy. |
| Must know | Explain the usage of the sublanguages in SQL | DDL defines schema; DML modifies data; DCL manages access and permissions; TCL manages transactions. |
| Must know | Understand the role of a primary key in a data-set. | A unique identifier for each record in a table, ensuring no duplicate rows exist and enabling efficient retrieval. |
| Must know | Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables | `CREATE TABLE Users (ID int, Name varchar(50));` |
| Must know | Describe and utilize constraints in table creation. (Unique, Not Null, Primary Key, Foreign Key, Auto Incrementing, Default, Check) | `CREATE TABLE Orders (ID int PRIMARY KEY, UserID int NOT NULL);` |
| Must know | Construct SQL statements using DML(Insert, Update, Delete) keywords to manipulate pre-existing data within tables | `INSERT INTO Users (ID, Name) VALUES (1, 'Alice');` |
| Must know | Describe the difference between DROP, DELETE, and TRUNCATE functionality | DROP removes the table entity; TRUNCATE empties the table structure quickly; DELETE removes specific rows based on a WHERE condition. |
| Must know | Understand the role of a foreign key in a data-set. | A field in one table that uniquely identifies a row of another table, establishing a referential link between them. |
| Must know | Create a valid schema for a given data-set | `CREATE TABLE Dept (ID int PK); CREATE TABLE Emp (ID int PK, DeptID int FK);` |
| Must know | Perform basic DDL operations, such as creating, dropping, or truncating tables. | `DROP TABLE Users;` |
| Must know | Understand difference between aggregate and scalar functions | Aggregate functions return a single value calculated from multiple rows (e.g., SUM). Scalar functions return a single value based on a single input value (e.g., UPPER). |
| Must know | Utilize the GROUP BY clause. | `SELECT DeptID, COUNT(*) FROM Emp GROUP BY DeptID;` |
| Must know | Demonstrate the ability to filter records using the WHERE clause and operators | `SELECT * FROM Users WHERE Age >= 18;` |
| Must know | Understand basic types of joins and demonstrate usage in select statements (inner, left/right outer, full outer, equi) | `SELECT * FROM TableA INNER JOIN TableB ON TableA.id = TableB.id;` |
| Must know | Describe the purpose of transactions in a database and when they are used. | Groups a sequence of operations into a single logical unit of work to guarantee either complete execution or complete rollback. |
| Must know | Understand database consistency and utilize transactions to ensure data consistency in a set of SQL commands. | `BEGIN TRANSACTION; UPDATE Accounts SET Balance = Balance - 100 WHERE ID = 1; COMMIT;` |
| Must know | Be able to create a User Defined Function | `CREATE FUNCTION GetTotal() RETURNS INT AS BEGIN RETURN (SELECT SUM(Amount) FROM Sales) END;` |
| Must know | Be able to create and call a Stored Procedure | `CREATE PROCEDURE GetUsers AS SELECT * FROM Users; EXEC GetUsers;` |
| Must know | Create and use views to store the results of a SQL query | `CREATE VIEW ActiveUsers AS SELECT * FROM Users WHERE Status = 'Active';` |
| Must know | Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing step-by-step justification. | 1NF: Eliminate repeating groups. 2NF: Remove partial dependencies. 3NF: Remove transitive dependencies. |
| Must know | Describe referential integrity | A property ensuring that a foreign key value must correspond to a valid primary key value in the referenced table, preventing orphaned records. |
| Must know | Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance in transaction management. | Atomicity (all or nothing), Consistency (valid state transitions), Isolation (concurrent execution control), Durability (permanent storage of committed data). |
| Should know | Demonstrate how to identify valid candidate keys for a primary key of an entity | Identify all attributes or combinations of attributes that uniquely identify a record (e.g., SSN, Email). One is selected as the Primary Key. |
| Should know | Read and understand ERD (Entity Relationship Diagram) | A visual blueprint depicting entities (tables), attributes (columns), and their relational connections. |
| Should know | Explain the concept of multiplicity in database relationships. | Defines the cardinality between entities, such as 1:1 (one-to-one), 1:N (one-to-many), or M:N (many-to-many). |
| Should know | Accurately describe database schemas, including tables, fields, and the relationships between them. | A logical architecture detailing tables, columns, data types, constraints, and relational mapping via primary and foreign keys. |
| Should know | Identify and implement common data typessuch as varchar, decimal, integer, and char. | `Price DECIMAL(10,2), Age INT` |
| Should know | Translate real-world problem descriptions into ER diagrams and implement them as a working relational schema. | Converting business logic into structured tables, applying appropriate normalization, and resolving M:N relationships. |
| Should know | Utilize the HAVING clause to filter aggregated query results | `SELECT DeptID, COUNT(*) FROM Emp GROUP BY DeptID HAVING COUNT(*) > 5;` |
| Should know | Understand when to use subqueries versus joins in SQL logic | Use joins to combine data sets; use subqueries for filtering against calculated aggregates or temporary sets not explicitly joined. |
| Should know | Identify and use commonly used aggregate functions (e.g., COUNT(), SUM(), AVG(), MIN(), MAX()) to summarize data in queries. | `SELECT AVG(Salary) FROM Employees;` |
| Should know | Utilize column aliases to enhance readability and clarity of SQL queries. | `SELECT FirstName AS [First Name] FROM Users;` |
| Should know | Identify and compare different isolation levels (Read Uncommitted, Read Committed, Repeatable Read, Serializable) and their trade-offs. | Higher isolation levels reduce concurrency phenomena (dirty reads, phantom reads) but decrease system performance and throughput. |
| Should know | Explain the benefits and potential drawbacks of database normalization, including impacts on performance and data integrity. | Benefits: Reduces redundancy, prevents insertion/deletion anomalies. Drawbacks: Increases query complexity and execution time due to required joins. |
| Should know | Utilize cascades to define what happens to related tables during DML operations | `FOREIGN KEY (DeptID) REFERENCES Dept(ID) ON DELETE CASCADE` |
| Should know | Configure triggers to execute the corresponding stored procedures when certain events occur | `CREATE TRIGGER AfterInsertUser ON Users AFTER INSERT AS EXEC LogNewUser;` |
| Should know | Describe database indexing and its benefits | Indexes are data structures (such as B-trees) that improve data retrieval speed on a table at the cost of additional storage and slower write operations. |
| Should know | Describe triggers and their use in automating tasks. | Stored procedures automatically executed by the database engine in response to specific DML events (INSERT, UPDATE, DELETE). |
| Nice to Have | Explain how to modify a table structure after creation using ALTER TABLE with examples (e.g., adding a column, modifying a data type). | `ALTER TABLE Users ADD Email varchar(100);` |
| Nice to Have | Recognize and explain less common key types beyond primary and foreign keys, such as candidate keys or composite keys. | Composite key: A primary key consisting of two or more columns to guarantee uniqueness when a single column is insufficient. |
| Nice to Have | Identify and implement advanced data types beyond the basics, such as BIGINT for handling very large integers | `TransactionID BIGINT` |
| Nice to Have | Capable of implementing bridge tables to handle many-to-many relationships between entities. | `CREATE TABLE StudentCourse (StudentID int, CourseID int);` |
| Nice to Have | Utilize subquery structure to execute a select statement. | `SELECT * FROM Emp WHERE DeptID IN (SELECT ID FROM Dept WHERE Name = 'Sales');` |
| Nice to Have | Utilize set operations between multiple select statement | `SELECT Name FROM TableA UNION SELECT Name FROM TableB;` |
| Nice to Have | Understand and know how to utilize Window Functions | `SELECT Name, Salary, ROW_NUMBER() OVER(PARTITION BY DeptID ORDER BY Salary DESC) FROM Employees;` |
| Nice to Have | Utilize Common Table Expressions (CTEs) | `WITH CTE AS (SELECT * FROM Users WHERE Age > 30) SELECT * FROM CTE;` |
