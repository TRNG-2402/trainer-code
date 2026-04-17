# Database Normalization

## Learning Objectives

- Understand the purpose and benefits of normalization
- Apply First, Second, and Third Normal Forms (1NF, 2NF, 3NF)
- Recognize data anomalies that normalization prevents
- Understand multiplicity and data consistency concepts

## Why This Matters

Normalization is a fundamental database design technique that organizes data to reduce redundancy and improve integrity. Without normalization, your database becomes difficult to maintain, wastes storage, and is prone to inconsistencies. Understanding normalization helps you design efficient, reliable databases.

## The Concept

### What is Normalization?

Normalization is the process of organizing database tables to:

- **Reduce redundancy**: Store each piece of data once
- **Prevent anomalies**: Avoid data inconsistencies
- **Improve integrity**: Make data relationships clear
- **Enable flexibility**: Allow easy modifications

### Data Anomalies

Without normalization, three types of problems occur:

**1. Insert Anomaly**: Cannot add data without other data
```
| order_id | customer_name | customer_email     | product |
|----------|---------------|--------------------|---------| 
| 1        | Alice         | alice@mail.com     | Widget  |

-- Cannot add a new customer without an order
```

**2. Update Anomaly**: Must update multiple rows for one change
```
| order_id | customer_name | customer_email |
|----------|---------------|----------------|
| 1        | Alice         | alice@mail.com |
| 2        | Alice         | alice@mail.com |  <- If email changes,
| 3        | Alice         | alice@mail.com |  <- must update all rows
```

**3. Delete Anomaly**: Deleting data removes unrelated information
```
| order_id | customer_name | customer_email     |
|----------|---------------|--------------------| 
| 1        | Alice         | alice@mail.com     |

-- Deleting order 1 loses customer information entirely
```

### Unnormalized Data Example

Consider this poorly designed table:

```
ORDERS (Unnormalized)
+----------+-----------+-----------------+---------+----------+-----------+
| order_id | cust_name | cust_email      | product | quantity | category  |
+----------+-----------+-----------------+---------+----------+-----------+
| 1        | Alice     | alice@mail.com  | Widget  | 2        | Hardware  |
| 2        | Alice     | alice@mail.com  | Gadget  | 1        | Hardware  |
| 3        | Bob       | bob@mail.com    | Widget  | 3        | Hardware  |
+----------+-----------+-----------------+---------+----------+-----------+
```

Problems:
- Customer info repeated on every order
- Product category repeated
- Updating Alice's email requires multiple updates

### First Normal Form (1NF)

**Rules:**
1. Each column contains atomic (indivisible) values
2. Each column contains values of a single type
3. Each row is unique (has a primary key)
4. No repeating groups

**Violation of 1NF:**
```
| order_id | customer | products           |
|----------|----------|--------------------|
| 1        | Alice    | Widget, Gadget     |  <- Multiple values in one cell
```

**1NF Compliant:**
```
| order_id | customer | product |
|----------|----------|---------|
| 1        | Alice    | Widget  |
| 1        | Alice    | Gadget  |
```

**SQL for 1NF:**
```sql
CREATE TABLE order_items (
    order_id INT,
    customer VARCHAR(100),
    product VARCHAR(100),
    PRIMARY KEY (order_id, product)
);
```

### Second Normal Form (2NF)

**Rules:**
1. Must be in 1NF
2. No partial dependencies (all non-key columns depend on the entire primary key)

**Violation of 2NF:**
```
Primary Key: (order_id, product_id)

| order_id | product_id | quantity | product_name | customer_name |
|----------|------------|----------|--------------|---------------|
```

- `product_name` depends only on `product_id`, not the full key
- `customer_name` depends only on `order_id`, not the full key

**2NF Compliant - Split into tables:**
```sql
-- Products table
CREATE TABLE products (
    product_id INT PRIMARY KEY,
    product_name VARCHAR(100)
);

-- Orders table
CREATE TABLE orders (
    order_id INT PRIMARY KEY,
    customer_name VARCHAR(100)
);

-- Order Items table (junction table)
CREATE TABLE order_items (
    order_id INT,
    product_id INT,
    quantity INT,
    PRIMARY KEY (order_id, product_id),
    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);
```

### Third Normal Form (3NF)

**Rules:**
1. Must be in 2NF
2. No transitive dependencies (non-key columns do not depend on other non-key columns)

**Violation of 3NF:**
```
| employee_id | department_id | department_name | department_location |
|-------------|---------------|-----------------|---------------------|
```

- `department_name` and `department_location` depend on `department_id`, not `employee_id`

**3NF Compliant:**
```sql
-- Departments table
CREATE TABLE departments (
    department_id INT PRIMARY KEY,
    department_name VARCHAR(100),
    department_location VARCHAR(100)
);

-- Employees table
CREATE TABLE employees (
    employee_id INT PRIMARY KEY,
    name VARCHAR(100),
    department_id INT,
    FOREIGN KEY (department_id) REFERENCES departments(id)
);
```

### Normalization Summary

| Form | Rule |
|------|------|
| **1NF** | Atomic values, no repeating groups |
| **2NF** | No partial dependencies on composite keys |
| **3NF** | No transitive dependencies between non-key attributes |

### Multiplicity (Cardinality)

Multiplicity describes the relationship between tables:

**One-to-One (1:1)**
```
EMPLOYEE --1----1-- EMPLOYEE_DETAILS
Each employee has exactly one detail record
```

**One-to-Many (1:N)**
```
DEPARTMENT --1----N-- EMPLOYEES
One department has many employees
```

**Many-to-Many (M:N)**
```
STUDENTS --M----N-- COURSES
Students take many courses; courses have many students
(Requires a junction table)
```

```sql
-- Junction table for many-to-many
CREATE TABLE student_courses (
    student_id INT,
    course_id INT,
    enrollment_date DATE,
    PRIMARY KEY (student_id, course_id),
    FOREIGN KEY (student_id) REFERENCES students(id),
    FOREIGN KEY (course_id) REFERENCES courses(id)
);
```

### Data Consistency

Normalization supports data consistency by:

**1. Single Source of Truth:**
```sql
-- Customer name stored once
UPDATE customers SET name = 'Alice Smith' WHERE id = 1;
-- All orders automatically refer to correct name via FK
```

**2. Referential Integrity:**
```sql
-- Cannot create order for non-existent customer
INSERT INTO orders (customer_id, total) VALUES (999, 100);
-- Error: Foreign key constraint fails
```

**3. Cascading Updates:**
```sql
-- Changes propagate automatically
ALTER TABLE orders
ADD FOREIGN KEY (customer_id) REFERENCES customers(id)
ON UPDATE CASCADE;
```

### Normalized Database Example

```
                    +----------------+
                    |   categories   |
                    +----------------+
                    | id (PK)        |
                    | name           |
                    +-------+--------+
                            |
                    +-------v--------+
                    |   products     |
                    +----------------+
+------------+      | id (PK)        |
| customers  |      | name           |
+------------+      | price          |
| id (PK)    |      | category_id(FK)|
| name       |      +-------+--------+
| email      |              |
+-----+------+      +-------v--------+
      |             |  order_items   |
      |             +----------------+
+-----v------+      | id (PK)        |
|  orders    |<-----| order_id (FK)  |
+------------+      | product_id (FK)|
| id (PK)    |      | quantity       |
| customer_id|      | unit_price     |
| order_date |      +----------------+
| status     |
+------------+
```

### When to Denormalize

Sometimes denormalization improves performance:

- Frequently joined data
- Read-heavy applications
- Reporting and analytics

We will cover denormalization concepts later in Week 4 with data warehousing.

## Summary

- Normalization reduces redundancy and prevents anomalies
- 1NF: Atomic values, no repeating groups
- 2NF: No partial dependencies on composite keys
- 3NF: No transitive dependencies
- Multiplicity describes relationships: 1:1, 1:N, M:N
- Normalized databases are easier to maintain and update
- FK constraints enforce referential integrity

## Additional Resources

- [Database Normalization Explained](https://www.guru99.com/database-normalization.html)
- [MySQL Normalization Tutorial](https://www.mysqltutorial.org/mysql-database-normalization/)
- [Normal Forms in DBMS](https://www.geeksforgeeks.org/normal-forms-in-dbms/)
