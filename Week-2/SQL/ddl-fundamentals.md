# DDL Fundamentals

## Learning Objectives

- Master CREATE statements for databases, tables, and indexes
- Use ALTER to modify existing database structures
- Understand DROP and TRUNCATE and their differences
- Apply DDL best practices

## Why This Matters

Data Definition Language (DDL) is how you create and modify the structure of your database. Whether setting up a new application or modifying an existing schema, DDL commands are essential. Understanding DDL helps you design databases correctly from the start and make safe modifications later.

## The Concept

### CREATE Statement

CREATE is used to make new database objects.

**Create a Database:**
```sql
-- Create a new database
CREATE DATABASE company_db;

-- Create with character set specification
CREATE DATABASE company_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

-- Create only if it doesn't exist
CREATE DATABASE IF NOT EXISTS company_db;
```

**Create a Table:**
```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE,
    department VARCHAR(50),
    salary DECIMAL(10, 2),
    hire_date DATE,
    is_active BOOLEAN DEFAULT TRUE
);
```

**Create Table with Foreign Key:**
```sql
CREATE TABLE orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    customer_id INT NOT NULL,
    order_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    total_amount DECIMAL(12, 2),
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);
```

**Create Table from Existing Table:**
```sql
-- Copy structure and data
CREATE TABLE employees_backup AS
SELECT * FROM employees;

-- Copy structure only (no data)
CREATE TABLE employees_copy AS
SELECT * FROM employees WHERE 1=0;
```

**Create Index:**
```sql
-- Single column index
CREATE INDEX idx_last_name ON employees(last_name);

-- Composite index (multiple columns)
CREATE INDEX idx_name ON employees(last_name, first_name);

-- Unique index
CREATE UNIQUE INDEX idx_email ON employees(email);
```

### ALTER Statement

ALTER modifies existing database objects.

**Add a Column:**
```sql
ALTER TABLE employees ADD phone VARCHAR(20);

-- Add with position
ALTER TABLE employees ADD middle_name VARCHAR(50) AFTER first_name;

-- Add as first column
ALTER TABLE employees ADD employee_code VARCHAR(10) FIRST;
```

**Modify a Column:**
```sql
-- Change data type
ALTER TABLE employees MODIFY phone VARCHAR(30);

-- Change type and null constraint
ALTER TABLE employees MODIFY salary DECIMAL(12, 2) NOT NULL;
```

**Rename a Column:**
```sql
ALTER TABLE employees CHANGE phone phone_number VARCHAR(30);
```

**Drop a Column:**
```sql
ALTER TABLE employees DROP COLUMN middle_name;
```

**Rename a Table:**
```sql
ALTER TABLE employees RENAME TO staff;
-- or
RENAME TABLE employees TO staff;
```

**Add Constraints:**
```sql
-- Add primary key
ALTER TABLE employees ADD PRIMARY KEY (id);

-- Add foreign key
ALTER TABLE orders ADD FOREIGN KEY (customer_id) REFERENCES customers(id);

-- Add unique constraint
ALTER TABLE employees ADD UNIQUE (email);

-- Add check constraint (MySQL 8.0+)
ALTER TABLE employees ADD CHECK (salary > 0);
```

**Drop Constraints:**
```sql
-- Drop primary key
ALTER TABLE employees DROP PRIMARY KEY;

-- Drop foreign key (need constraint name)
ALTER TABLE orders DROP FOREIGN KEY fk_customer;

-- Drop index
ALTER TABLE employees DROP INDEX idx_last_name;
```

### DROP Statement

DROP permanently removes database objects.

```sql
-- Drop a table (deletes structure and all data)
DROP TABLE employees;

-- Drop only if exists (prevents error)
DROP TABLE IF EXISTS employees;

-- Drop a database
DROP DATABASE company_db;

-- Drop an index
DROP INDEX idx_last_name ON employees;
```

**Warning**: DROP is irreversible. Always backup before dropping.

### TRUNCATE Statement

TRUNCATE quickly removes all data from a table.

```sql
TRUNCATE TABLE logs;
```

### DROP vs. TRUNCATE vs. DELETE

| Aspect | DROP | TRUNCATE | DELETE |
|--------|------|----------|--------|
| Removes | Structure + Data | Data only | Data only |
| Can filter rows | No | No | Yes (WHERE) |
| Speed | Fast | Very fast | Slower |
| Triggers fire | No | No | Yes |
| Can rollback | No (DDL) | No (DDL) | Yes (DML) |
| Resets AUTO_INCREMENT | Yes | Yes | No |

```sql
-- DELETE with condition
DELETE FROM logs WHERE created_at < '2024-01-01';

-- DELETE all (slower than TRUNCATE)
DELETE FROM logs;

-- TRUNCATE all (faster, resets AUTO_INCREMENT)
TRUNCATE TABLE logs;

-- DROP (removes table entirely)
DROP TABLE logs;
```

### Best Practices

**1. Use IF EXISTS / IF NOT EXISTS:**
```sql
CREATE DATABASE IF NOT EXISTS mydb;
CREATE TABLE IF NOT EXISTS users ( ... );
DROP TABLE IF EXISTS temp_data;
```

**2. Plan Before Altering:**
```sql
-- Check current structure first
DESCRIBE employees;
SHOW CREATE TABLE employees;

-- Then make changes
ALTER TABLE employees ADD column_name datatype;
```

**3. Backup Before Destructive Operations:**
```sql
-- Create backup before dropping
CREATE TABLE employees_backup AS SELECT * FROM employees;

-- Then drop original
DROP TABLE employees;
```

**4. Use Transactions Where Possible:**

Note: In MySQL, DDL statements cause an implicit commit. Consider using migration tools for complex schema changes.

**5. Document Schema Changes:**
```sql
-- Add comments to tables and columns
ALTER TABLE employees COMMENT = 'Store employee information';
ALTER TABLE employees MODIFY first_name VARCHAR(50) COMMENT 'Employee given name';
```

### Common DDL Patterns

**Creating Audit Columns:**
```sql
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2),
    -- Audit columns
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    updated_by VARCHAR(50)
);
```

**Temporary Tables:**
```sql
-- Create a temporary table (exists only for session)
CREATE TEMPORARY TABLE temp_results (
    id INT,
    value DECIMAL(10, 2)
);
```

## Summary

- CREATE builds new databases, tables, and indexes
- ALTER modifies existing structures (add, modify, drop columns)
- DROP permanently removes database objects
- TRUNCATE quickly removes all data but keeps structure
- Use IF EXISTS/IF NOT EXISTS to prevent errors
- Always backup before destructive DDL operations
- DDL changes are auto-committed in MySQL

## Additional Resources

- [MySQL CREATE TABLE Reference](https://dev.mysql.com/doc/refman/8.0/en/create-table.html)
- [MySQL ALTER TABLE Reference](https://dev.mysql.com/doc/refman/8.0/en/alter-table.html)
- [DDL Commands Tutorial](https://www.mysqltutorial.org/mysql-create-table/)
