# SQL Introduction

## Learning Objectives

- Understand what SQL is and its role in data management
- Learn the history and evolution of SQL
- Recognize SQL standards and dialects
- Appreciate SQL's importance in the modern data ecosystem

## Why This Matters

SQL (Structured Query Language) is the universal language for working with relational databases. Whether you are a data engineer, analyst, or backend developer, SQL is essential. It has been the standard for over 40 years and remains the most important skill for anyone working with data. This week, as part of "Becoming Data Fluent," SQL will become your primary tool for accessing, manipulating, and managing data.

## The Concept

### What is SQL?

SQL (pronounced "S-Q-L" or "sequel") is a domain-specific language designed for managing and manipulating relational databases. It allows you to:

- **Query data**: Retrieve specific information from databases
- **Modify data**: Insert, update, and delete records
- **Define structure**: Create tables, define relationships
- **Control access**: Manage who can access what data
- **Manage transactions**: Ensure data integrity during operations

### A Brief History

| Year | Milestone |
|------|-----------|
| 1970 | Edgar F. Codd publishes relational model theory at IBM |
| 1974 | SEQUEL (predecessor to SQL) developed at IBM |
| 1979 | Oracle releases first commercial SQL database |
| 1986 | SQL becomes ANSI standard (SQL-86) |
| 1992 | SQL-92 brings major enhancements |
| 1999 | SQL:1999 adds recursive queries, triggers |
| 2003 | SQL:2003 adds XML features, window functions |
| 2011 | SQL:2011 adds temporal data support |
| 2016 | SQL:2016 adds JSON support |
| Today | SQL remains the standard for relational data |

### SQL Standards vs. Dialects

**SQL Standard (ANSI/ISO)**

The SQL standard defines core syntax that should work across all compliant databases:

```sql
SELECT first_name, last_name
FROM employees
WHERE department = 'Engineering';
```

**Database Dialects**

Each database vendor extends SQL with proprietary features:

| Database | Dialect Name | Notable Features |
|----------|--------------|------------------|
| MySQL | MySQL SQL | LIMIT, AUTO_INCREMENT |
| PostgreSQL | PostgreSQL | SERIAL, RETURNING, rich types |
| SQL Server | T-SQL | TOP, IDENTITY, procedures |
| Oracle | PL/SQL | ROWNUM, packages, hierarchical |
| SQLite | SQLite SQL | Lightweight, file-based |

For this course, we will use MySQL, but the core concepts apply across all databases.

### SQL in the Data Ecosystem

SQL is central to modern data architecture:

```
Applications     Data Engineers     Analysts
    |                 |                |
    v                 v                v
+------------------------------------------+
|              SQL Interface               |
+------------------------------------------+
    |                 |                |
    v                 v                v
+----------+    +-----------+    +---------+
| OLTP     |    | Data      |    | Data    |
| Databases|    | Warehouses|    | Lakes   |
+----------+    +-----------+    +---------+
```

- **Applications** use SQL to store and retrieve user data
- **Data Engineers** use SQL for ETL pipelines and transformations
- **Analysts** use SQL for reporting and business intelligence
- **Data Scientists** use SQL to prepare data for modeling

### Why Learn SQL?

1. **Universal**: Works across virtually all relational databases
2. **Declarative**: Tell the database what you want, not how to get it
3. **Powerful**: Complex operations in simple statements
4. **Mature**: 40+ years of optimization and refinement
5. **In Demand**: One of the most requested skills in data jobs

### Your First SQL Query

```sql
-- This is a comment in SQL
-- Select specific columns from a table
SELECT first_name, last_name, email
FROM customers
WHERE country = 'USA'
ORDER BY last_name;
```

This query:
1. Selects three columns
2. From the customers table
3. Filters for USA customers only
4. Orders results alphabetically by last name

### SQL vs. Other Approaches

**SQL (Declarative):**
```sql
SELECT name FROM employees WHERE salary > 50000;
```

**Procedural equivalent (Python-like pseudocode):**
```python
results = []
for employee in employees:
    if employee.salary > 50000:
        results.append(employee.name)
```

SQL focuses on what you want, not the steps to get it. The database optimizer determines the most efficient execution path.

### Key Terminology

| Term | Definition |
|------|------------|
| Database | Collection of organized data |
| Table | Structured set of data organized in rows and columns |
| Row (Record) | Single entry in a table |
| Column (Field) | Single attribute/property in a table |
| Query | Request for data from a database |
| Schema | Structure and organization of a database |

## Summary

- SQL is the standard language for relational database management
- Developed in the 1970s, standardized in 1986, continuously evolving
- Different databases have dialects but share core SQL syntax
- SQL is declarative: you specify what data you want, not how to retrieve it
- Essential skill for anyone working with data in any capacity

## Additional Resources

- [W3Schools SQL Tutorial](https://www.w3schools.com/sql/)
- [SQL Wikipedia History](https://en.wikipedia.org/wiki/SQL)
- [MySQL Official Documentation](https://dev.mysql.com/doc/)
