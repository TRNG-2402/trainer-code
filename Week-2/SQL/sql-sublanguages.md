# SQL Sub-Languages

## Learning Objectives

- Understand the five SQL sub-languages and their purposes
- Identify which commands belong to each sub-language
- Know when to use each category of SQL commands
- Recognize the relationship between sub-languages

## Why This Matters

SQL is not a single monolithic language. It is organized into sub-languages, each with a specific purpose. Understanding this organization helps you:

- Know what type of operation you are performing
- Understand permission requirements (some operations need admin rights)
- Communicate clearly with team members about database tasks
- Structure your SQL knowledge systematically

## The Concept

### The Five SQL Sub-Languages

SQL is divided into five categories based on the type of operation:

```
+------------------------------------------+
|                   SQL                    |
+------------------------------------------+
|  DDL   |  DML   |  DQL   |  DCL   |  TCL |
|--------|--------|--------|--------|------|
| Define | Modify | Query  | Control| Trans|
| struct | data   | data   | access | -act |
+------------------------------------------+
```

### DDL - Data Definition Language

DDL commands define and modify database structure:

| Command | Purpose |
|---------|---------|
| CREATE | Create databases, tables, indexes |
| ALTER | Modify existing structures |
| DROP | Delete databases, tables |
| TRUNCATE | Remove all data from a table |
| RENAME | Rename database objects |

```sql
-- DDL Examples
CREATE TABLE customers (id INT, name VARCHAR(100));
ALTER TABLE customers ADD email VARCHAR(255);
DROP TABLE customers;
TRUNCATE TABLE logs;
```

**Key Characteristic**: DDL changes are typically auto-committed (cannot be rolled back in MySQL).

### DML - Data Manipulation Language

DML commands modify the data within tables:

| Command | Purpose |
|---------|---------|
| INSERT | Add new records |
| UPDATE | Modify existing records |
| DELETE | Remove records |

```sql
-- DML Examples
INSERT INTO customers (name, email) VALUES ('Alice', 'alice@mail.com');
UPDATE customers SET email = 'newemail@mail.com' WHERE id = 1;
DELETE FROM customers WHERE id = 1;
```

**Key Characteristic**: DML changes can be rolled back if within a transaction.

### DQL - Data Query Language

DQL is used to retrieve data:

| Command | Purpose |
|---------|---------|
| SELECT | Retrieve data from tables |

```sql
-- DQL Examples
SELECT * FROM customers;
SELECT name, email FROM customers WHERE country = 'USA';
SELECT COUNT(*) FROM orders WHERE status = 'completed';
```

**Key Characteristic**: DQL does not modify data, only reads it.

**Note**: Some sources combine DQL into DML since SELECT is often used alongside INSERT/UPDATE/DELETE.

### DCL - Data Control Language

DCL manages access permissions:

| Command | Purpose |
|---------|---------|
| GRANT | Give permissions to users |
| REVOKE | Remove permissions from users |

```sql
-- DCL Examples
GRANT SELECT, INSERT ON customers TO 'analyst_user'@'localhost';
GRANT ALL PRIVILEGES ON mydb.* TO 'admin_user'@'localhost';
REVOKE DELETE ON customers FROM 'analyst_user'@'localhost';
```

**Key Characteristic**: DCL is typically used by database administrators.

### TCL - Transaction Control Language

TCL manages transactions:

| Command | Purpose |
|---------|---------|
| COMMIT | Save transaction changes permanently |
| ROLLBACK | Undo transaction changes |
| SAVEPOINT | Create a point to rollback to |
| SET TRANSACTION | Configure transaction properties |

```sql
-- TCL Examples
START TRANSACTION;
INSERT INTO accounts (id, balance) VALUES (1, 1000);
UPDATE accounts SET balance = balance - 100 WHERE id = 1;
COMMIT;

-- Or if something goes wrong:
ROLLBACK;
```

**Key Characteristic**: TCL ensures data integrity by grouping operations.

### Sub-Language Summary

```
+----------+------------------------+---------------------------+
| Category | Commands               | Purpose                   |
+----------+------------------------+---------------------------+
| DDL      | CREATE, ALTER, DROP    | Define structure          |
|          | TRUNCATE, RENAME       |                           |
+----------+------------------------+---------------------------+
| DML      | INSERT, UPDATE, DELETE | Modify data               |
+----------+------------------------+---------------------------+
| DQL      | SELECT                 | Query/retrieve data       |
+----------+------------------------+---------------------------+
| DCL      | GRANT, REVOKE          | Control access            |
+----------+------------------------+---------------------------+
| TCL      | COMMIT, ROLLBACK       | Manage transactions       |
|          | SAVEPOINT              |                           |
+----------+------------------------+---------------------------+
```

### Practical Workflow Example

A typical database workflow uses all sub-languages:

```sql
-- 1. DDL: Create structure
CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100),
    price DECIMAL(10, 2)
);

-- 2. DCL: Set up access
GRANT SELECT, INSERT ON products TO 'app_user'@'localhost';

-- 3. DML: Add data
START TRANSACTION;  -- TCL: Begin transaction
INSERT INTO products (name, price) VALUES ('Widget', 29.99);
INSERT INTO products (name, price) VALUES ('Gadget', 49.99);
COMMIT;  -- TCL: Save changes

-- 4. DQL: Query data
SELECT * FROM products WHERE price < 40;

-- 5. DML: Update data
UPDATE products SET price = 34.99 WHERE name = 'Widget';

-- 6. DDL: Modify structure
ALTER TABLE products ADD stock_quantity INT DEFAULT 0;
```

### Permission Requirements

Different sub-languages require different privileges:

| Sub-Language | Typical Users |
|--------------|---------------|
| DDL | Database administrators, developers (dev only) |
| DML | Application users, analysts |
| DQL | Almost everyone (read access) |
| DCL | Database administrators only |
| TCL | Application users within their scope |

### Relationship Diagram

```
User Request
     |
     v
+----+----+                  +--------+
|   DQL   | ----------------> | Read   |
| SELECT  |                  | Data   |
+---------+                  +--------+

+----+----+                  +--------+
|   DML   | ----------------> | Change |
| INSERT  |                  | Data   |
| UPDATE  |     +------+     +--------+
| DELETE  | --> | TCL  |
+---------+     | ---- |
                | COMMIT   |
                | ROLLBACK |
                +----------+

+----+----+                  +--------+
|   DDL   | ----------------> | Change |
| CREATE  |                  | Schema |
| ALTER   |                  +--------+
| DROP    |
+---------+

+----+----+                  +--------+
|   DCL   | ----------------> | Manage |
| GRANT   |                  | Access |
| REVOKE  |                  +--------+
+---------+
```

## Summary

- SQL is organized into five sub-languages: DDL, DML, DQL, DCL, TCL
- DDL (CREATE, ALTER, DROP) defines database structure
- DML (INSERT, UPDATE, DELETE) modifies data
- DQL (SELECT) retrieves data
- DCL (GRANT, REVOKE) controls access permissions
- TCL (COMMIT, ROLLBACK) manages transactions
- Understanding these categories helps organize SQL knowledge

## Additional Resources

- [SQL Commands Overview (W3Schools)](https://www.w3schools.com/sql/)
- [DDL, DML, DCL, TCL Explained (GeeksforGeeks)](https://www.geeksforgeeks.org/sql-ddl-dql-dml-dcl-tcl-commands/)
- [MySQL SQL Statement Syntax](https://dev.mysql.com/doc/refman/8.0/en/sql-statements.html)
