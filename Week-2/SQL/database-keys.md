# Database Keys

## Learning Objectives

- Understand the types of keys used in relational databases
- Differentiate between primary, foreign, composite, and unique keys
- Apply referential integrity concepts
- Choose appropriate keys for different scenarios

## Why This Matters

Keys are the foundation of relational database design. They uniquely identify rows, establish relationships between tables, and enforce data integrity. Choosing the right keys affects query performance, data accuracy, and application design. Understanding keys is essential for effective database work.

## The Concept

### What is a Key?

A key is one or more columns used to:
- Uniquely identify rows within a table
- Establish relationships between tables
- Enable efficient data retrieval

### Primary Key

The primary key uniquely identifies each row in a table.

**Characteristics:**
- Must be unique for each row
- Cannot contain NULL values
- Only one primary key per table
- Should be stable (rarely changes)

```sql
-- Simple primary key
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100),
    email VARCHAR(100)
);

-- Alternative syntax
CREATE TABLE employees (
    id INT,
    name VARCHAR(100),
    PRIMARY KEY (id)
);
```

**Natural vs. Surrogate Keys:**

| Type | Description | Example |
|------|-------------|---------|
| Natural Key | Business data that is naturally unique | Email, SSN |
| Surrogate Key | System-generated identifier | AUTO_INCREMENT id |

```sql
-- Natural key (email as primary key)
CREATE TABLE users (
    email VARCHAR(255) PRIMARY KEY,
    name VARCHAR(100)
);

-- Surrogate key (recommended)
CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE,
    name VARCHAR(100)
);
```

Surrogate keys are generally preferred because they are:
- Independent of business rules
- Stable and never need to change
- Simple and efficient for joins

### Composite Key

A composite key uses multiple columns to uniquely identify a row.

```sql
-- Order items table: order_id + product_id is unique
CREATE TABLE order_items (
    order_id INT,
    product_id INT,
    quantity INT,
    unit_price DECIMAL(10, 2),
    PRIMARY KEY (order_id, product_id)
);
```

Use composite keys when:
- No single column is unique
- The combination of columns is naturally unique
- Creating a junction table for many-to-many relationships

```sql
-- Many-to-many relationship
CREATE TABLE student_courses (
    student_id INT,
    course_id INT,
    enrollment_date DATE,
    grade CHAR(2),
    PRIMARY KEY (student_id, course_id),
    FOREIGN KEY (student_id) REFERENCES students(id),
    FOREIGN KEY (course_id) REFERENCES courses(id)
);
```

### Foreign Key

A foreign key creates a link between two tables by referencing the primary key of another table.

**Characteristics:**
- References a primary key (or unique key) in another table
- Can contain NULL (unless NOT NULL specified)
- Enforces referential integrity
- Can have multiple foreign keys per table

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

-- With named constraint
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    department_id INT,
    CONSTRAINT fk_department 
        FOREIGN KEY (department_id) REFERENCES departments(id)
);
```

### Unique Key

A unique key prevents duplicate values in a column but allows NULL.

```sql
CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE,
    phone VARCHAR(20) UNIQUE
);

-- Multiple columns unique together
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sku VARCHAR(50),
    warehouse_id INT,
    UNIQUE (sku, warehouse_id)
);
```

**Primary Key vs. Unique Key:**

| Feature | Primary Key | Unique Key |
|---------|-------------|------------|
| NULLs allowed | No | Yes (one NULL) |
| Per table | One only | Multiple allowed |
| Identifies rows | Yes | No (just enforces uniqueness) |
| Creates index | Clustered (usually) | Non-clustered |

### Secondary (Alternate) Key

A secondary key is any key that could serve as the primary key but was not chosen.

```sql
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,      -- Primary key
    sku VARCHAR(50) UNIQUE,                  -- Alternate key
    barcode VARCHAR(20) UNIQUE               -- Alternate key
);
```

Any UNIQUE column (or column combination) is a candidate key. The one chosen as PRIMARY KEY is the primary key; others are alternate keys.

### Referential Integrity

Referential integrity ensures that relationships between tables remain consistent.

**Rules:**
1. Foreign key values must match existing primary key values (or be NULL)
2. Cannot delete a parent record if child records reference it (unless CASCADE)
3. Cannot update a parent key if child records reference it (unless CASCADE)

```sql
-- This fails: department doesn't exist
INSERT INTO employees (name, department_id) VALUES ('Alice', 999);
-- Error: Cannot add or update a child row: foreign key constraint fails

-- This fails: department has employees
DELETE FROM departments WHERE id = 1;
-- Error: Cannot delete or update a parent row: foreign key constraint fails
```

**Cascade Options:**

```sql
CREATE TABLE employees (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100),
    department_id INT,
    CONSTRAINT fk_dept FOREIGN KEY (department_id) REFERENCES departments(id)
        ON DELETE SET NULL      -- Set to NULL when department deleted
        ON UPDATE CASCADE       -- Update when department id changes
);
```

| Action | ON DELETE Effect | ON UPDATE Effect |
|--------|------------------|------------------|
| CASCADE | Delete child rows | Update child FK values |
| SET NULL | Set FK to NULL | Set FK to NULL |
| RESTRICT | Block deletion | Block update |
| NO ACTION | Block deletion | Block update |

### Key Summary Table

| Key Type | Purpose | Nullability | Count per Table |
|----------|---------|-------------|-----------------|
| Primary Key | Unique row identifier | No NULLs | Exactly 1 |
| Foreign Key | Link to another table | NULLs allowed | Multiple |
| Unique Key | Prevent duplicates | NULLs allowed | Multiple |
| Composite Key | Multi-column identifier | No NULLs (if PK) | 1 (if PK) |

### Viewing Keys

```sql
-- Show primary key
SHOW KEYS FROM employees WHERE Key_name = 'PRIMARY';

-- Show all keys/indexes
SHOW INDEX FROM employees;

-- Show foreign keys
SELECT 
    CONSTRAINT_NAME,
    COLUMN_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'employees'
AND REFERENCED_TABLE_NAME IS NOT NULL;
```

## Summary

- **Primary Key**: Uniquely identifies rows; one per table, no NULLs
- **Foreign Key**: References another table's primary key; enforces referential integrity
- **Composite Key**: Multiple columns forming a unique identifier
- **Unique Key**: Prevents duplicates; allows NULLs; multiple per table
- Surrogate keys (auto-increment IDs) are preferred over natural keys
- Referential integrity ensures relationship consistency
- CASCADE options control behavior when parent rows change

## Additional Resources

- [MySQL Keys and Indexes](https://dev.mysql.com/doc/refman/8.0/en/create-table.html)
- [Understanding Database Keys](https://www.guru99.com/dbms-keys.html)
- [Foreign Key Constraints Tutorial](https://www.mysqltutorial.org/mysql-foreign-key/)
