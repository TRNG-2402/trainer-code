# SQL Constraints

## Learning Objectives

- Understand the purpose and importance of constraints
- Master PRIMARY KEY, FOREIGN KEY, and UNIQUE constraints
- Apply NOT NULL, CHECK, and DEFAULT constraints
- Use AUTO_INCREMENT and CASCADE options

## Why This Matters

Constraints are rules that enforce data integrity. Without them, your database could contain invalid, duplicate, or inconsistent data. Constraints act as guardrails, preventing bad data from entering the system. They are essential for maintaining trustworthy data that applications and reports can rely on.

## The Concept

### What are Constraints?

Constraints are rules applied to columns or tables that restrict the type of data that can be stored. They ensure:

- **Data accuracy**: Only valid data is accepted
- **Data consistency**: Related data stays synchronized
- **Data integrity**: Relationships are maintained

### PRIMARY KEY

A primary key uniquely identifies each row in a table.

**Rules:**
- Must be unique for each row
- Cannot contain NULL values
- Only one primary key per table

```sql
-- Single column primary key
CREATE TABLE employees (
    id INT PRIMARY KEY,
    name VARCHAR(100)
);

-- With AUTO_INCREMENT
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100)
);

-- Composite primary key (multiple columns)
CREATE TABLE order_items (
    order_id INT,
    product_id INT,
    quantity INT,
    PRIMARY KEY (order_id, product_id)
);

-- Add primary key to existing table
ALTER TABLE employees ADD PRIMARY KEY (id);
```

### FOREIGN KEY

A foreign key creates a link between two tables.

**Rules:**
- References a primary key in another table
- Ensures referential integrity
- Prevents orphaned records

```sql
CREATE TABLE departments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    department_id INT,
    FOREIGN KEY (department_id) REFERENCES departments(id)
);

-- Now you cannot insert an employee with a non-existent department_id
INSERT INTO employees (name, department_id) VALUES ('Alice', 999);
-- Error: Cannot add or update a child row: foreign key constraint fails
```

**Named Foreign Key:**
```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    department_id INT,
    CONSTRAINT fk_department FOREIGN KEY (department_id) REFERENCES departments(id)
);
```

### UNIQUE

UNIQUE ensures no duplicate values in a column.

```sql
CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) UNIQUE,
    email VARCHAR(100) UNIQUE
);

-- Insert duplicate fails
INSERT INTO users (username, email) VALUES ('alice', 'alice@mail.com');
INSERT INTO users (username, email) VALUES ('alice', 'other@mail.com');
-- Error: Duplicate entry 'alice' for key 'username'
```

**Difference from PRIMARY KEY:**
- A table can have multiple UNIQUE constraints
- UNIQUE columns can contain NULL (but only one NULL in MySQL)

### NOT NULL

NOT NULL prevents NULL values in a column.

```sql
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,      -- Required
    description TEXT,                 -- Optional (allows NULL)
    price DECIMAL(10, 2) NOT NULL    -- Required
);

-- This fails
INSERT INTO products (description, price) VALUES ('A product', 29.99);
-- Error: Field 'name' doesn't have a default value
```

### DEFAULT

DEFAULT provides a value when none is specified.

```sql
CREATE TABLE orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    quantity INT DEFAULT 1
);

-- Insert without specifying defaults
INSERT INTO orders (id) VALUES (1);
-- Result: status='pending', created_at=now(), quantity=1
```

### CHECK (MySQL 8.0+)

CHECK validates data against a condition.

```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    age INT CHECK (age >= 18 AND age <= 120),
    salary DECIMAL(10, 2) CHECK (salary > 0)
);

-- This fails
INSERT INTO employees (name, age, salary) VALUES ('Alice', 16, 50000);
-- Error: Check constraint 'employees_chk_1' is violated
```

**Named Check Constraint:**
```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    age INT,
    CONSTRAINT chk_age CHECK (age >= 18)
);
```

### AUTO_INCREMENT

AUTO_INCREMENT generates sequential numbers automatically.

```sql
CREATE TABLE customers (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100)
);

INSERT INTO customers (name) VALUES ('Alice');  -- id = 1
INSERT INTO customers (name) VALUES ('Bob');    -- id = 2
INSERT INTO customers (name) VALUES ('Charlie'); -- id = 3

-- Check the next AUTO_INCREMENT value
SHOW TABLE STATUS LIKE 'customers';

-- Reset AUTO_INCREMENT
ALTER TABLE customers AUTO_INCREMENT = 100;
```

### CASCADE Options

CASCADE defines what happens when a referenced row is deleted or updated.

```sql
CREATE TABLE orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    customer_id INT,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
        ON DELETE CASCADE       -- Delete orders when customer is deleted
        ON UPDATE CASCADE       -- Update customer_id when customer id changes
);
```

**CASCADE Options:**

| Option | ON DELETE Effect | ON UPDATE Effect |
|--------|------------------|------------------|
| CASCADE | Delete child rows | Update child references |
| SET NULL | Set to NULL | Set to NULL |
| SET DEFAULT | Set to default | Set to default |
| RESTRICT | Prevent delete | Prevent update |
| NO ACTION | Same as RESTRICT | Same as RESTRICT |

```sql
-- Example: SET NULL on delete
CREATE TABLE orders (
    id INT PRIMARY KEY,
    customer_id INT,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
        ON DELETE SET NULL
);

-- When customer is deleted, their orders remain but customer_id becomes NULL
```

### Viewing Constraints

```sql
-- Show table structure including constraints
DESCRIBE table_name;

-- Show CREATE statement with all constraints
SHOW CREATE TABLE table_name;

-- Query constraint information
SELECT * FROM information_schema.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'employees';

SELECT * FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'employees';
```

### Dropping Constraints

```sql
-- Drop primary key
ALTER TABLE employees DROP PRIMARY KEY;

-- Drop foreign key (need constraint name)
ALTER TABLE orders DROP FOREIGN KEY fk_customer;

-- Drop unique constraint
ALTER TABLE users DROP INDEX username;

-- Drop check constraint (MySQL 8.0+)
ALTER TABLE employees DROP CHECK chk_age;
```

### Constraint Best Practices

1. **Always have a primary key** on every table
2. **Use foreign keys** to enforce relationships
3. **Apply NOT NULL** to required fields
4. **Use CHECK constraints** for data validation
5. **Consider CASCADE carefully** - it can cause mass deletions
6. **Name your constraints** for easier management

```sql
-- Well-constrained table
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sku VARCHAR(20) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL CHECK (price >= 0),
    stock_quantity INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    category_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_category FOREIGN KEY (category_id) REFERENCES categories(id)
);
```

## Summary

- PRIMARY KEY uniquely identifies rows; one per table
- FOREIGN KEY enforces relationships between tables
- UNIQUE prevents duplicate values in a column
- NOT NULL requires a value; no NULLs allowed
- CHECK validates data against conditions
- DEFAULT provides values when none specified
- AUTO_INCREMENT generates sequential IDs
- CASCADE controls referential actions on delete/update

## Additional Resources

- [MySQL Constraints Reference](https://dev.mysql.com/doc/refman/8.0/en/constraints.html)
- [Foreign Key Constraints Tutorial](https://www.mysqltutorial.org/mysql-foreign-key/)
- [Understanding SQL Constraints](https://www.w3schools.com/sql/sql_constraints.asp)
