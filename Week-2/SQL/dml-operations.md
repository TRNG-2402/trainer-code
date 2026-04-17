# DML Operations

## Learning Objectives

- Master INSERT statements for adding data
- Use UPDATE statements to modify existing data
- Apply DELETE statements safely
- Understand best practices for data manipulation

## Why This Matters

Data Manipulation Language (DML) is how you work with the actual data in your database. While DDL defines the structure, DML populates and maintains that structure with real information. These are the commands you will use most frequently when building applications and managing data.

## The Concept

### INSERT Statement

INSERT adds new rows to a table.

**Basic Syntax:**
```sql
-- Insert a single row
INSERT INTO employees (first_name, last_name, email, salary)
VALUES ('Alice', 'Johnson', 'alice@company.com', 75000);
```

**Insert Multiple Rows:**
```sql
INSERT INTO employees (first_name, last_name, email, salary) VALUES
    ('Bob', 'Smith', 'bob@company.com', 65000),
    ('Carol', 'Williams', 'carol@company.com', 80000),
    ('David', 'Brown', 'david@company.com', 70000);
```

**Insert All Columns:**
```sql
-- When inserting all columns, you can omit the column list
-- Order must match table definition
INSERT INTO departments VALUES (1, 'Engineering', 'Building A');
```

**Insert with DEFAULT:**
```sql
-- Use DEFAULT keyword for columns with default values
INSERT INTO orders (customer_id, total, status)
VALUES (1, 150.00, DEFAULT);  -- Uses default 'pending' status
```

**Insert with SELECT (Copy Data):**
```sql
-- Copy data from another table
INSERT INTO employees_archive (id, name, email)
SELECT id, CONCAT(first_name, ' ', last_name), email
FROM employees
WHERE hire_date < '2020-01-01';
```

**Insert or Update (UPSERT):**
```sql
-- Insert new row or update if key exists
INSERT INTO inventory (product_id, quantity)
VALUES (101, 50)
ON DUPLICATE KEY UPDATE quantity = quantity + 50;
```

### UPDATE Statement

UPDATE modifies existing rows.

**Basic Syntax:**
```sql
-- Update specific rows
UPDATE employees
SET salary = 80000
WHERE id = 1;
```

**Update Multiple Columns:**
```sql
UPDATE employees
SET salary = 85000,
    department = 'Engineering',
    updated_at = NOW()
WHERE id = 1;
```

**Update with Calculation:**
```sql
-- Give everyone a 10% raise
UPDATE employees
SET salary = salary * 1.10;

-- Increase quantity by ordered amount
UPDATE inventory
SET stock = stock - 5
WHERE product_id = 101;
```

**Update with Subquery:**
```sql
-- Update based on data from another table
UPDATE employees e
SET department_id = (
    SELECT id FROM departments WHERE name = 'Engineering'
)
WHERE e.department = 'Engineering';
```

**Update with JOIN:**
```sql
-- Update using data from related table
UPDATE orders o
JOIN customers c ON o.customer_id = c.id
SET o.customer_email = c.email
WHERE o.customer_email IS NULL;
```

**IMPORTANT: Always Use WHERE**
```sql
-- DANGEROUS: Updates ALL rows
UPDATE employees SET salary = 50000;

-- SAFE: Updates only specified rows
UPDATE employees SET salary = 50000 WHERE id = 5;
```

### DELETE Statement

DELETE removes rows from a table.

**Basic Syntax:**
```sql
-- Delete specific rows
DELETE FROM employees WHERE id = 1;
```

**Delete with Multiple Conditions:**
```sql
-- Delete inactive employees hired before 2020
DELETE FROM employees
WHERE is_active = FALSE
AND hire_date < '2020-01-01';
```

**Delete with Subquery:**
```sql
-- Delete orders from inactive customers
DELETE FROM orders
WHERE customer_id IN (
    SELECT id FROM customers WHERE status = 'inactive'
);
```

**Delete All Rows:**
```sql
-- Delete all (but keep table structure)
DELETE FROM logs;

-- More efficient for large tables
TRUNCATE TABLE logs;
```

**IMPORTANT: Always Use WHERE**
```sql
-- DANGEROUS: Deletes ALL rows
DELETE FROM employees;

-- SAFE: Deletes only specified rows
DELETE FROM employees WHERE id = 5;
```

### Safe Mode for Updates and Deletes

MySQL has a safe mode that prevents updates/deletes without a WHERE clause:

```sql
-- Check safe mode status
SHOW VARIABLES LIKE 'sql_safe_updates';

-- Disable safe mode (use with caution)
SET SQL_SAFE_UPDATES = 0;

-- Re-enable safe mode
SET SQL_SAFE_UPDATES = 1;
```

### RETURNING Data (Preview for Later)

Some databases support RETURNING to see affected rows:

```sql
-- MySQL uses LAST_INSERT_ID() for inserts
INSERT INTO employees (name, salary) VALUES ('Eve', 70000);
SELECT LAST_INSERT_ID();

-- PostgreSQL style (not MySQL)
-- INSERT INTO employees (name) VALUES ('Eve') RETURNING id;
```

### Best Practices

**1. Always Preview Before Modifying:**
```sql
-- First, see what will be affected
SELECT * FROM employees WHERE department = 'Sales';

-- Then, run your update/delete
UPDATE employees SET salary = salary * 1.05 WHERE department = 'Sales';
```

**2. Use Transactions for Safety:**
```sql
START TRANSACTION;

DELETE FROM order_items WHERE order_id = 100;
DELETE FROM orders WHERE id = 100;

-- Review changes, then commit or rollback
COMMIT;
-- or ROLLBACK;
```

**3. Limit Rows for Large Operations:**
```sql
-- Process in batches to avoid locking
DELETE FROM logs WHERE created_at < '2023-01-01' LIMIT 1000;
-- Repeat until all are deleted
```

**4. Back Up Before Major Operations:**
```sql
-- Create backup table
CREATE TABLE employees_backup AS SELECT * FROM employees;

-- Then perform operation
DELETE FROM employees WHERE department = 'Obsolete';
```

**5. Use Specific Column Lists:**
```sql
-- Good: Explicit columns
INSERT INTO employees (first_name, last_name, email)
VALUES ('Frank', 'Miller', 'frank@company.com');

-- Risky: All columns in table order
INSERT INTO employees VALUES (NULL, 'Frank', 'Miller', 'frank@company.com', ...);
```

### Common Patterns

**Soft Delete:**
```sql
-- Instead of deleting, mark as inactive
UPDATE employees SET is_deleted = TRUE WHERE id = 5;

-- Query only active records
SELECT * FROM employees WHERE is_deleted = FALSE;
```

**Audit Trail:**
```sql
-- Update with audit info
UPDATE products
SET price = 29.99,
    updated_at = NOW(),
    updated_by = 'admin_user'
WHERE id = 101;
```

**Conditional Insert:**
```sql
-- Insert only if not exists
INSERT INTO categories (name)
SELECT 'Electronics'
WHERE NOT EXISTS (SELECT 1 FROM categories WHERE name = 'Electronics');
```

## Summary

- INSERT adds new rows; can insert single or multiple rows
- UPDATE modifies existing rows; always use WHERE clause
- DELETE removes rows; always use WHERE clause
- Use transactions for multi-statement safety
- Preview changes with SELECT before modifying
- Back up data before major operations
- Consider soft deletes for recoverable deletions

## Additional Resources

- [MySQL INSERT Reference](https://dev.mysql.com/doc/refman/8.0/en/insert.html)
- [MySQL UPDATE Reference](https://dev.mysql.com/doc/refman/8.0/en/update.html)
- [MySQL DELETE Reference](https://dev.mysql.com/doc/refman/8.0/en/delete.html)
