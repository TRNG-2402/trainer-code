# Transactions and ACID Properties

## Learning Objectives

- Understand what a database transaction is
- Master the ACID properties (Atomicity, Consistency, Isolation, Durability)
- Apply COMMIT and ROLLBACK for transaction control
- Recognize when to use transactions

## Why This Matters

When multiple database operations must succeed or fail together, transactions ensure data integrity. Consider a bank transfer: if debiting one account succeeds but crediting another fails, money would disappear. Transactions prevent such disasters by treating related operations as a single unit.

## The Concept

### What is a Transaction?

A transaction is a sequence of database operations that are treated as a single logical unit. Either all operations succeed, or all fail.

**Example Scenario:**
```
Bank Transfer: Move $100 from Account A to Account B

1. Debit $100 from Account A
2. Credit $100 to Account B

Both must succeed, or neither should happen.
```

### ACID Properties

ACID is an acronym for four essential transaction properties:

```
A - Atomicity      : All or nothing
C - Consistency    : Valid state to valid state
I - Isolation      : Transactions don't interfere
D - Durability     : Committed changes persist
```

### Atomicity

All operations in a transaction complete successfully, or none do.

**Without Atomicity (Problem):**
```
1. UPDATE accounts SET balance = balance - 100 WHERE id = 1;  -- Succeeds
2. [System Crash]
3. UPDATE accounts SET balance = balance + 100 WHERE id = 2;  -- Never runs
-- $100 has vanished!
```

**With Atomicity (Solution):**
```sql
START TRANSACTION;
UPDATE accounts SET balance = balance - 100 WHERE id = 1;
UPDATE accounts SET balance = balance + 100 WHERE id = 2;
COMMIT;
-- Either both happen, or neither does
```

### Consistency

A transaction brings the database from one valid state to another valid state. Constraints are always enforced.

```sql
-- If Account A has only $50:
START TRANSACTION;
UPDATE accounts SET balance = balance - 100 WHERE id = 1;
-- This might violate a CHECK (balance >= 0) constraint
-- Transaction fails, maintaining consistency
ROLLBACK;
```

Consistency ensures:
- All constraints are satisfied
- Referential integrity is maintained
- Data remains valid

### Isolation

Concurrent transactions do not interfere with each other. Each transaction sees a consistent snapshot of the database.

**Without Isolation (Problem):**
```
Transaction 1:                  Transaction 2:
Read balance: $100             
                                Read balance: $100
Debit $50, balance now $50     
                                Debit $30, balance now $70
Write balance: $50             
                                Write balance: $70
-- Final balance: $70 (should be $20!)
```

**With Isolation:** Each transaction sees a consistent view, and the database correctly handles concurrent updates.

### Durability

Once a transaction is committed, the changes are permanent, even if the system crashes.

```sql
START TRANSACTION;
INSERT INTO orders (customer_id, total) VALUES (1, 500);
COMMIT;  -- After this, the order is permanently saved

-- Even if the server crashes immediately after COMMIT,
-- the order will be there when the system restarts
```

Durability is achieved through:
- Transaction logs
- Write-ahead logging (WAL)
- Checkpointing

### Transaction Control Commands

**START TRANSACTION:**
```sql
START TRANSACTION;
-- or
BEGIN;

-- Operations here are not permanent until COMMIT
```

**COMMIT:**
```sql
START TRANSACTION;
INSERT INTO orders (customer_id, total) VALUES (1, 100);
UPDATE inventory SET stock = stock - 1 WHERE product_id = 5;
COMMIT;  -- All changes are now permanent
```

**ROLLBACK:**
```sql
START TRANSACTION;
DELETE FROM orders WHERE status = 'cancelled';
-- Oops, wrong query!
ROLLBACK;  -- Undo everything since START TRANSACTION
```

**SAVEPOINT:**
```sql
START TRANSACTION;

INSERT INTO orders (customer_id, total) VALUES (1, 100);
SAVEPOINT order_created;

INSERT INTO order_items (order_id, product_id) VALUES (1, 5);
-- Something wrong with this item
ROLLBACK TO SAVEPOINT order_created;

-- Order still exists, but order_items rolled back
INSERT INTO order_items (order_id, product_id) VALUES (1, 6);
COMMIT;
```

### Practical Transaction Examples

**Bank Transfer:**
```sql
START TRANSACTION;

-- Check sufficient balance
SELECT balance INTO @current_balance 
FROM accounts WHERE id = 1;

IF @current_balance >= 100 THEN
    UPDATE accounts SET balance = balance - 100 WHERE id = 1;
    UPDATE accounts SET balance = balance + 100 WHERE id = 2;
    COMMIT;
ELSE
    ROLLBACK;
END IF;
```

**Order Processing:**
```sql
START TRANSACTION;

-- Create order
INSERT INTO orders (customer_id, total, status)
VALUES (1, 150.00, 'pending');

SET @order_id = LAST_INSERT_ID();

-- Add items
INSERT INTO order_items (order_id, product_id, quantity, price)
VALUES 
    (@order_id, 101, 2, 50.00),
    (@order_id, 102, 1, 50.00);

-- Update inventory
UPDATE products SET stock = stock - 2 WHERE id = 101;
UPDATE products SET stock = stock - 1 WHERE id = 102;

-- Check if stock went negative
IF (SELECT MIN(stock) FROM products WHERE id IN (101, 102)) < 0 THEN
    ROLLBACK;
    -- Signal error: insufficient stock
ELSE
    UPDATE orders SET status = 'confirmed' WHERE id = @order_id;
    COMMIT;
END IF;
```

### CRUD Operations Within Transactions

| Operation | Within Transaction |
|-----------|-------------------|
| CREATE (INSERT) | Can be rolled back |
| READ (SELECT) | Sees current transaction state |
| UPDATE | Can be rolled back |
| DELETE | Can be rolled back |

### Auto-Commit Mode

By default, MySQL auto-commits each statement:

```sql
-- Each statement is its own transaction
INSERT INTO logs (message) VALUES ('Event 1');  -- Committed
INSERT INTO logs (message) VALUES ('Event 2');  -- Committed

-- To disable auto-commit:
SET autocommit = 0;
INSERT INTO logs (message) VALUES ('Event 3');  -- Not committed yet
INSERT INTO logs (message) VALUES ('Event 4');  -- Not committed yet
COMMIT;  -- Now both are committed
SET autocommit = 1;
```

### When to Use Transactions

**Use transactions when:**
- Multiple related changes must succeed or fail together
- Transferring resources between records
- Creating parent and child records together
- Processing batches that should be atomic
- Any operation where partial completion is unacceptable

**Transaction Best Practices:**
1. Keep transactions as short as possible
2. Avoid user interaction during transactions
3. Handle errors and rollback explicitly
4. Be aware of lock contention in high-traffic systems

## Summary

- Transactions group operations into atomic units
- **Atomicity**: All operations complete or none do
- **Consistency**: Database moves between valid states
- **Isolation**: Concurrent transactions do not interfere
- **Durability**: Committed changes are permanent
- Use `START TRANSACTION`, `COMMIT`, and `ROLLBACK` for control
- `SAVEPOINT` allows partial rollbacks
- Keep transactions short and handle errors properly

## Additional Resources

- [MySQL Transaction Reference](https://dev.mysql.com/doc/refman/8.0/en/commit.html)
- [Understanding ACID Properties](https://www.databricks.com/glossary/acid-transactions)
- [Transaction Isolation Levels](https://dev.mysql.com/doc/refman/8.0/en/innodb-transaction-isolation-levels.html)
