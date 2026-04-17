# Transaction Isolation Levels

## Learning Objectives

- Understand concurrency problems in database transactions
- Master the four SQL isolation levels
- Learn about aliases and sequences
- Apply COMMIT and ROLLBACK appropriately

## Why This Matters

When multiple users access a database simultaneously, transactions can interfere with each other. Isolation levels control how much transactions see each other's changes. Choosing the right isolation level balances data consistency against performance.

## The Concept

### Concurrency Problems

Without proper isolation, concurrent transactions can cause these problems:

**Dirty Read:**
Transaction reads uncommitted data from another transaction.
```
T1: UPDATE accounts SET balance = 500 WHERE id = 1;
T2: SELECT balance FROM accounts WHERE id = 1;  -- Sees 500
T1: ROLLBACK;  -- Changes undone
-- T2 read data that never actually existed
```

**Non-Repeatable Read:**
Reading the same row twice yields different values.
```
T1: SELECT balance FROM accounts WHERE id = 1;  -- 100
T2: UPDATE accounts SET balance = 200 WHERE id = 1; COMMIT;
T1: SELECT balance FROM accounts WHERE id = 1;  -- 200 (different!)
```

**Phantom Read:**
A query returns different sets of rows when run twice.
```
T1: SELECT * FROM orders WHERE status = 'pending';  -- 5 rows
T2: INSERT INTO orders (status) VALUES ('pending'); COMMIT;
T1: SELECT * FROM orders WHERE status = 'pending';  -- 6 rows (new row appeared)
```

### The Four Isolation Levels

| Level | Dirty Read | Non-Repeatable | Phantom |
|-------|------------|----------------|---------|
| READ UNCOMMITTED | Possible | Possible | Possible |
| READ COMMITTED | Prevented | Possible | Possible |
| REPEATABLE READ | Prevented | Prevented | Possible* |
| SERIALIZABLE | Prevented | Prevented | Prevented |

*MySQL's REPEATABLE READ also prevents phantoms using gap locking.

### READ UNCOMMITTED

Lowest isolation. Transactions see uncommitted changes.

```sql
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
START TRANSACTION;
SELECT * FROM accounts;  -- May see uncommitted data
COMMIT;
```

**Use case:** Rarely used. Only for rough estimates where accuracy is not critical.

### READ COMMITTED

Each read sees only committed data at the time of the read.

```sql
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
START TRANSACTION;
SELECT balance FROM accounts WHERE id = 1;  -- Sees committed data only
-- If another transaction commits changes, next read sees them
SELECT balance FROM accounts WHERE id = 1;  -- Might be different
COMMIT;
```

**Use case:** Default in Oracle, SQL Server. Good for most applications.

### REPEATABLE READ

Same row always returns the same data within a transaction.

```sql
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
START TRANSACTION;
SELECT balance FROM accounts WHERE id = 1;  -- 100
-- Other transactions can commit changes
SELECT balance FROM accounts WHERE id = 1;  -- Still 100 (repeatable)
COMMIT;
```

**Use case:** Default in MySQL. Good when you need consistent reads.

### SERIALIZABLE

Highest isolation. Transactions execute as if serial (one at a time).

```sql
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
START TRANSACTION;
SELECT * FROM orders WHERE status = 'pending';
-- Database locks prevent others from inserting matching rows
COMMIT;
```

**Use case:** Financial transactions, inventory management. Slower but safest.

### Setting Isolation Level

```sql
-- For current session
SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ;

-- For next transaction only
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

-- Check current level
SELECT @@transaction_isolation;
```

### Aliases

Aliases provide shorthand names for tables and columns.

**Column Aliases:**
```sql
SELECT 
    first_name AS name,
    salary * 12 AS annual_salary,
    CONCAT(first_name, ' ', last_name) AS full_name
FROM employees;
```

**Table Aliases:**
```sql
SELECT e.name, d.department_name
FROM employees AS e
JOIN departments AS d ON e.department_id = d.id;

-- AS is optional
SELECT e.name, d.department_name
FROM employees e
JOIN departments d ON e.department_id = d.id;
```

**Self-Join Aliases (Required):**
```sql
SELECT 
    emp.name AS employee,
    mgr.name AS manager
FROM employees emp
LEFT JOIN employees mgr ON emp.manager_id = mgr.id;
```

### Sequences (AUTO_INCREMENT)

Sequences generate unique numeric values. MySQL uses AUTO_INCREMENT:

```sql
CREATE TABLE orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    customer_id INT,
    total DECIMAL(10, 2)
);

-- Insert without specifying ID
INSERT INTO orders (customer_id, total) VALUES (1, 100.00);
INSERT INTO orders (customer_id, total) VALUES (2, 150.00);

-- Check generated IDs
SELECT * FROM orders;
-- id 1 and 2 automatically assigned

-- Get last inserted ID
SELECT LAST_INSERT_ID();

-- Set starting value
ALTER TABLE orders AUTO_INCREMENT = 1000;
```

**Custom Sequences (Workaround):**
```sql
-- Create a sequence table
CREATE TABLE sequences (
    name VARCHAR(50) PRIMARY KEY,
    value BIGINT NOT NULL
);

INSERT INTO sequences VALUES ('invoice_number', 1000);

-- Get next value (in a transaction)
START TRANSACTION;
UPDATE sequences SET value = value + 1 WHERE name = 'invoice_number';
SELECT value FROM sequences WHERE name = 'invoice_number';
COMMIT;
```

### COMMIT and ROLLBACK

**COMMIT**: Make changes permanent.
```sql
START TRANSACTION;
UPDATE accounts SET balance = balance - 100 WHERE id = 1;
UPDATE accounts SET balance = balance + 100 WHERE id = 2;
COMMIT;  -- Both updates are now permanent
```

**ROLLBACK**: Undo all changes in the transaction.
```sql
START TRANSACTION;
DELETE FROM orders WHERE id = 100;
DELETE FROM order_items WHERE order_id = 100;
-- Oops, wrong order!
ROLLBACK;  -- Nothing was deleted
```

**SAVEPOINT**: Partial rollback.
```sql
START TRANSACTION;

INSERT INTO orders (customer_id, total) VALUES (1, 100);
SAVEPOINT after_order;

INSERT INTO order_items (order_id, product_id) VALUES (1, 5);
-- Problem with this item
ROLLBACK TO after_order;

INSERT INTO order_items (order_id, product_id) VALUES (1, 6);
COMMIT;
```

### Choosing Isolation Levels

| Scenario | Recommended Level |
|----------|-------------------|
| Reports/analytics (approximate) | READ UNCOMMITTED |
| General web applications | READ COMMITTED |
| Financial reports, auditing | REPEATABLE READ |
| Banking, inventory, critical data | SERIALIZABLE |

**Trade-offs:**
- Higher isolation = More consistency, less concurrency
- Lower isolation = More concurrency, potential inconsistencies

### Best Practices

1. **Use the minimum isolation needed** for your use case
2. **Keep transactions short** to reduce lock contention
3. **Handle deadlocks** with retry logic
4. **Avoid long-running transactions** at high isolation
5. **Test concurrent scenarios** before production

## Summary

- **Dirty reads**: Seeing uncommitted data
- **Non-repeatable reads**: Same query, different results
- **Phantom reads**: New/deleted rows appearing
- Four isolation levels trade off consistency vs. concurrency
- **REPEATABLE READ** is MySQL's default
- **Aliases** simplify query writing
- **AUTO_INCREMENT** generates sequences
- **COMMIT** saves changes; **ROLLBACK** undoes them

## Additional Resources

- [MySQL Transaction Isolation Levels](https://dev.mysql.com/doc/refman/8.0/en/innodb-transaction-isolation-levels.html)
- [Understanding Isolation Levels](https://www.postgresql.org/docs/current/transaction-iso.html)
- [ACID Properties Explained](https://www.databricks.com/glossary/acid-transactions)
