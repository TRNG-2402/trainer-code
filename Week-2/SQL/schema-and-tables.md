# Schema and Tables

## Learning Objectives

- Understand what a schema is and its purpose
- Learn the structure of database tables
- Understand rows and columns in the context of databases
- Define and create a basic schema

## Why This Matters

Before you can store data, you need to define where and how it will be stored. Schemas provide the organizational structure, and tables provide the storage containers. Understanding how to design schemas and tables is fundamental to working with any relational database. A well-designed schema makes data easy to query, maintain, and scale.

## The Concept

### What is a Schema?

A schema is the blueprint or structure of a database. It defines:

- What tables exist
- What columns each table has
- What data types are used
- How tables relate to each other
- What rules (constraints) apply to the data

Think of a schema as an architect's blueprint for a building. It describes the structure without containing the actual content.

### Schema Diagram Example

```
+------------------+         +------------------+
|    CUSTOMERS     |         |     ORDERS       |
+------------------+         +------------------+
| id (PK)          |----+    | id (PK)          |
| first_name       |    |    | customer_id (FK) |---+
| last_name        |    +----| order_date       |   |
| email            |         | total_amount     |   |
| created_at       |         | status           |   |
+------------------+         +------------------+   |
                                                    |
                             +------------------+   |
                             |   ORDER_ITEMS    |   |
                             +------------------+   |
                             | id (PK)          |   |
                             | order_id (FK)    |---+
                             | product_id (FK)  |---+
                             | quantity         |   |
                             | unit_price       |   |
                             +------------------+   |
                                                    |
                             +------------------+   |
                             |    PRODUCTS      |   |
                             +------------------+   |
                             | id (PK)          |---+
                             | name             |
                             | description      |
                             | price            |
                             | stock_quantity   |
                             +------------------+
```

### What is a Table?

A table is the fundamental storage structure in a relational database. It consists of:

- **Columns (Fields)**: Define the attributes/properties
- **Rows (Records)**: Contain the actual data entries

**Visual Representation:**

```
                    COLUMNS (Attributes)
                    |       |       |       |
                    v       v       v       v
              +------+-------+-------+---------+
              | id   | name  | email | country |  <-- Column headers
              +------+-------+-------+---------+
ROWS     -->  | 1    | Alice | a@... | USA     |
(Records)     | 2    | Bob   | b@... | UK      |
              | 3    | Carol | c@... | Canada  |
              +------+-------+-------+---------+
```

### Rows (Records)

A row represents a single entry in a table:

- Each row contains data for one instance (one customer, one order)
- All rows in a table have the same structure (same columns)
- Rows should be uniquely identifiable (usually via a primary key)

```sql
-- Each line returned is one row
SELECT * FROM customers;

-- Result:
-- +----+-------+---------------+---------+
-- | id | name  | email         | country |
-- +----+-------+---------------+---------+
-- | 1  | Alice | alice@mail.com| USA     |  <-- Row 1
-- | 2  | Bob   | bob@mail.com  | UK      |  <-- Row 2
-- +----+-------+---------------+---------+
```

### Columns (Fields)

A column represents a single attribute:

- Each column has a name (descriptive identifier)
- Each column has a data type (what kind of data it holds)
- Each column can have constraints (rules about the data)

```sql
-- View column information for a table
DESCRIBE customers;

-- Result:
-- +----------+-------------+------+-----+---------+----------------+
-- | Field    | Type        | Null | Key | Default | Extra          |
-- +----------+-------------+------+-----+---------+----------------+
-- | id       | int         | NO   | PRI | NULL    | auto_increment |
-- | name     | varchar(50) | NO   |     | NULL    |                |
-- | email    | varchar(100)| NO   | UNI | NULL    |                |
-- | country  | varchar(50) | YES  |     | USA     |                |
-- +----------+-------------+------+-----+---------+----------------+
```

### Creating a Database

```sql
-- Create a new database
CREATE DATABASE company_db;

-- Switch to using that database
USE company_db;

-- Verify you're in the right database
SELECT DATABASE();
```

### Creating a Table

```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    hire_date DATE,
    salary DECIMAL(10, 2),
    department VARCHAR(50) DEFAULT 'General'
);
```

Let's break this down:

| Element | Description |
|---------|-------------|
| `id INT` | Column named "id" with integer type |
| `PRIMARY KEY` | Uniquely identifies each row |
| `AUTO_INCREMENT` | Automatically generates sequential numbers |
| `VARCHAR(50)` | Variable-length string up to 50 characters |
| `NOT NULL` | Column must have a value |
| `UNIQUE` | No duplicate values allowed |
| `DATE` | Stores date values |
| `DECIMAL(10,2)` | Decimal with 10 total digits, 2 after decimal |
| `DEFAULT 'General'` | Default value if none provided |

### Viewing Table Structure

```sql
-- Show table structure
DESCRIBE employees;
-- or
SHOW COLUMNS FROM employees;

-- Show how the table was created
SHOW CREATE TABLE employees;
```

### Modifying Tables

```sql
-- Add a new column
ALTER TABLE employees ADD phone VARCHAR(20);

-- Modify a column's data type
ALTER TABLE employees MODIFY phone VARCHAR(30);

-- Rename a column
ALTER TABLE employees CHANGE phone phone_number VARCHAR(30);

-- Drop a column
ALTER TABLE employees DROP COLUMN phone_number;

-- Rename a table
RENAME TABLE employees TO staff;
```

### Dropping Tables and Databases

```sql
-- Remove a table (CAUTION: deletes all data)
DROP TABLE employees;

-- Remove only if it exists (prevents error)
DROP TABLE IF EXISTS employees;

-- Remove a database (CAUTION: deletes everything)
DROP DATABASE company_db;
```

### Schema Best Practices

1. **Use meaningful names**: `customer_orders` not `co1`
2. **Be consistent**: Choose a naming convention (snake_case is common)
3. **Avoid reserved words**: Do not name columns `select`, `table`, etc.
4. **Document your schema**: Keep an up-to-date schema diagram
5. **Plan for growth**: Consider future data needs
6. **Normalize your data**: Reduce redundancy (covered later this week)

### Naming Conventions

| Convention | Example | Common Use |
|------------|---------|------------|
| snake_case | customer_orders | MySQL, PostgreSQL |
| PascalCase | CustomerOrders | SQL Server |
| camelCase | customerOrders | Some ORMs |

Consistency matters more than which convention you choose.

## Summary

- A schema defines the structure of a database (tables, columns, relationships)
- Tables store data in rows (records) and columns (fields)
- Each column has a name, data type, and optional constraints
- CREATE TABLE defines a new table with its columns
- ALTER TABLE modifies existing table structure
- Use meaningful, consistent naming conventions

## Additional Resources

- [MySQL CREATE TABLE Syntax](https://dev.mysql.com/doc/refman/8.0/en/create-table.html)
- [Database Schema Design Best Practices](https://www.lucidchart.com/pages/database-diagram/database-schema)
- [SQL Data Definition Language](https://www.geeksforgeeks.org/sql-ddl-dql-dml-dcl-tcl-commands/)
