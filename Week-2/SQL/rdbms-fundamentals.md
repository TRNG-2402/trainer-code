# RDBMS Fundamentals

## Learning Objectives

- Understand what a database is and why databases are used
- Learn the principles of relational database management systems (RDBMS)
- Differentiate between database types
- Get familiar with MySQL as our primary RDBMS

## Why This Matters

Before writing SQL queries, you need to understand what you are querying. Relational databases have been the backbone of data storage for businesses since the 1980s. Understanding RDBMS concepts helps you design better databases, write more efficient queries, and troubleshoot data issues. As part of becoming data fluent, grasping these fundamentals is essential.

## The Concept

### What is a Database?

A database is an organized collection of structured data stored electronically. Databases allow you to:

- Store large amounts of data efficiently
- Retrieve specific data quickly
- Maintain data integrity and consistency
- Support multiple users simultaneously
- Protect data with security and backup systems

### Database vs. Spreadsheet

| Feature | Spreadsheet | Database |
|---------|-------------|----------|
| Data Volume | Thousands of rows | Millions/billions of rows |
| Users | One at a time (typically) | Many simultaneously |
| Data Integrity | Manual validation | Enforced constraints |
| Relationships | Limited | Full relational support |
| Security | File-level | Row/column level |
| Performance | Degrades with size | Optimized for scale |

### What is a DBMS?

A Database Management System (DBMS) is software that:

- Stores and retrieves data
- Manages concurrent access
- Ensures data security
- Maintains data integrity
- Provides backup and recovery

### What is an RDBMS?

A Relational Database Management System (RDBMS) is a DBMS based on the relational model:

- Data is organized into **tables** (relations)
- Tables have **rows** (records) and **columns** (attributes)
- Tables are connected through **relationships** using keys
- Data is accessed using **SQL**

**Core Principles of Relational Databases:**

1. **Tables represent entities** (customers, orders, products)
2. **Columns represent attributes** (name, date, price)
3. **Rows represent instances** (specific customer, specific order)
4. **Keys establish relationships** between tables
5. **Constraints enforce rules** on data

### The Relational Model

```
CUSTOMERS Table                 ORDERS Table
+----+----------+--------+      +----+---------+----------+--------+
| id | name     | email  |      | id | cust_id | date     | total  |
+----+----------+--------+      +----+---------+----------+--------+
| 1  | Alice    | a@mail |  <-- | 1  | 1       | 2024-01  | 150.00 |
| 2  | Bob      | b@mail |  <-- | 2  | 1       | 2024-02  | 200.00 |
| 3  | Charlie  | c@mail |      | 3  | 2       | 2024-01  | 75.00  |
+----+----------+--------+      +----+---------+----------+--------+
                                      ^
                                      |
                                Relationship via cust_id
```

### Types of Databases

**Relational Databases (RDBMS)**
- MySQL, PostgreSQL, Oracle, SQL Server, SQLite
- Best for: Structured data with clear relationships
- Query language: SQL

**NoSQL Databases**
- MongoDB (Document), Redis (Key-Value), Cassandra (Column), Neo4j (Graph)
- Best for: Unstructured data, high scalability, specific use cases
- Query language: Varies by database

**NewSQL Databases**
- CockroachDB, TiDB, Spanner
- Best for: Combining SQL with horizontal scalability
- Query language: SQL

### MySQL Overview

MySQL is one of the most popular open-source RDBMSs:

**Key Features:**
- Free and open source (community edition)
- Cross-platform (Windows, Linux, macOS)
- High performance and reliability
- Strong community and documentation
- Used by major companies (Facebook, Twitter, YouTube)

**MySQL Architecture:**

```
Client Applications
        |
        v
+------------------+
|  MySQL Server    |
|  +------------+  |
|  | SQL Parser |  |
|  +------------+  |
|  | Query      |  |
|  | Optimizer  |  |
|  +------------+  |
|  | Storage    |  |
|  | Engine     |  |
|  +------------+  |
+------------------+
        |
        v
   Data Files
```

### Connecting to MySQL

```sql
-- Command line connection
mysql -u username -p

-- After entering password, you'll see:
mysql>

-- Show available databases
SHOW DATABASES;

-- Select a database to use
USE database_name;

-- Show tables in current database
SHOW TABLES;
```

### Database vs. Schema

In MySQL, "database" and "schema" are synonymous:

```sql
-- These are equivalent in MySQL
CREATE DATABASE myapp;
CREATE SCHEMA myapp;
```

In other systems like PostgreSQL and SQL Server, a schema is a namespace within a database.

### Client-Server Architecture

```
+--------+     +--------+     +--------+
| Client |     | Client |     | Client |
+--------+     +--------+     +--------+
    |              |              |
    +--------------+--------------+
                   |
                   v
           +---------------+
           | MySQL Server  |
           +---------------+
                   |
                   v
           +---------------+
           | Data Storage  |
           +---------------+
```

Multiple clients can connect simultaneously, and MySQL manages:
- Connection handling
- Query processing
- Concurrency control
- Transaction management

### ACID Properties (Preview)

Relational databases guarantee ACID properties for transactions:

- **Atomicity**: All operations complete or none do
- **Consistency**: Data remains valid after transactions
- **Isolation**: Concurrent transactions do not interfere
- **Durability**: Committed changes persist permanently

We will cover these in depth on Wednesday.

## Summary

- A database is an organized collection of structured data
- DBMS is software that manages database operations
- RDBMS uses the relational model with tables, rows, and columns
- Tables are connected through relationships using keys
- MySQL is a popular open-source RDBMS we will use throughout this week
- ACID properties ensure reliable database transactions

## Additional Resources

- [MySQL Official Documentation](https://dev.mysql.com/doc/)
- [What is a Relational Database? (AWS)](https://aws.amazon.com/relational-database/)
- [Database Fundamentals (Microsoft)](https://learn.microsoft.com/en-us/sql/relational-databases/)
