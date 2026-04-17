# Stored Procedures

## Learning Objectives

- Understand what stored procedures are and their benefits
- Create procedures with IN, OUT, and INOUT parameters
- Use variables and control flow within procedures
- Apply stored procedure best practices

## Why This Matters

Stored procedures encapsulate business logic in the database, improving performance, security, and maintainability. They reduce network traffic, enforce consistency, and allow complex operations to be called with a simple statement.

## The Concept

### What is a Stored Procedure?

A stored procedure is a prepared SQL code that you save and call by name. It can:

- Accept parameters
- Contain multiple SQL statements
- Include control flow (IF, LOOP, etc.)
- Return results or output parameters

### Creating a Basic Procedure

```sql
DELIMITER //
CREATE PROCEDURE GetAllEmployees()
BEGIN
    SELECT * FROM employees;
END //
DELIMITER ;

-- Call the procedure
CALL GetAllEmployees();
```

**Note:** DELIMITER changes the statement terminator temporarily because procedures contain semicolons.

### Parameters

**IN Parameters (Input):**
```sql
DELIMITER //
CREATE PROCEDURE GetEmployeesByDepartment(IN dept_name VARCHAR(50))
BEGIN
    SELECT * FROM employees WHERE department = dept_name;
END //
DELIMITER ;

-- Call with parameter
CALL GetEmployeesByDepartment('Engineering');
```

**OUT Parameters (Output):**
```sql
DELIMITER //
CREATE PROCEDURE GetEmployeeCount(OUT emp_count INT)
BEGIN
    SELECT COUNT(*) INTO emp_count FROM employees;
END //
DELIMITER ;

-- Call and retrieve output
CALL GetEmployeeCount(@count);
SELECT @count;
```

**INOUT Parameters:**
```sql
DELIMITER //
CREATE PROCEDURE DoubleValue(INOUT value INT)
BEGIN
    SET value = value * 2;
END //
DELIMITER ;

-- Use INOUT
SET @num = 5;
CALL DoubleValue(@num);
SELECT @num;  -- Returns 10
```

### Variables

Declare and use local variables:

```sql
DELIMITER //
CREATE PROCEDURE CalculateBonus(IN emp_id INT, OUT bonus DECIMAL(10,2))
BEGIN
    DECLARE emp_salary DECIMAL(10,2);
    DECLARE bonus_rate DECIMAL(5,2) DEFAULT 0.10;
    
    SELECT salary INTO emp_salary FROM employees WHERE id = emp_id;
    
    SET bonus = emp_salary * bonus_rate;
END //
DELIMITER ;
```

### Control Flow

**IF Statement:**
```sql
DELIMITER //
CREATE PROCEDURE GetEmployeeLevel(IN emp_id INT, OUT level VARCHAR(20))
BEGIN
    DECLARE emp_salary DECIMAL(10,2);
    
    SELECT salary INTO emp_salary FROM employees WHERE id = emp_id;
    
    IF emp_salary >= 100000 THEN
        SET level = 'Senior';
    ELSEIF emp_salary >= 50000 THEN
        SET level = 'Mid-Level';
    ELSE
        SET level = 'Junior';
    END IF;
END //
DELIMITER ;
```

**CASE Statement:**
```sql
DELIMITER //
CREATE PROCEDURE GetQuarter(IN month INT, OUT quarter VARCHAR(2))
BEGIN
    CASE 
        WHEN month BETWEEN 1 AND 3 THEN SET quarter = 'Q1';
        WHEN month BETWEEN 4 AND 6 THEN SET quarter = 'Q2';
        WHEN month BETWEEN 7 AND 9 THEN SET quarter = 'Q3';
        ELSE SET quarter = 'Q4';
    END CASE;
END //
DELIMITER ;
```

**LOOP:**
```sql
DELIMITER //
CREATE PROCEDURE InsertTestData(IN num_records INT)
BEGIN
    DECLARE i INT DEFAULT 1;
    
    insert_loop: LOOP
        IF i > num_records THEN
            LEAVE insert_loop;
        END IF;
        
        INSERT INTO test_table (value) VALUES (CONCAT('Value ', i));
        SET i = i + 1;
    END LOOP;
END //
DELIMITER ;
```

**WHILE Loop:**
```sql
DELIMITER //
CREATE PROCEDURE ProcessBatch()
BEGIN
    DECLARE remaining INT;
    
    SELECT COUNT(*) INTO remaining FROM pending_tasks;
    
    WHILE remaining > 0 DO
        -- Process one task
        UPDATE pending_tasks SET status = 'complete' 
        WHERE status = 'pending' LIMIT 1;
        
        SET remaining = remaining - 1;
    END WHILE;
END //
DELIMITER ;
```

### Practical Examples

**Order Processing:**
```sql
DELIMITER //
CREATE PROCEDURE ProcessOrder(
    IN p_customer_id INT,
    IN p_product_id INT,
    IN p_quantity INT,
    OUT p_order_id INT,
    OUT p_message VARCHAR(100)
)
BEGIN
    DECLARE v_stock INT;
    DECLARE v_price DECIMAL(10,2);
    
    -- Check stock
    SELECT stock, price INTO v_stock, v_price 
    FROM products WHERE id = p_product_id;
    
    IF v_stock < p_quantity THEN
        SET p_order_id = NULL;
        SET p_message = 'Insufficient stock';
    ELSE
        -- Create order
        INSERT INTO orders (customer_id, total, status)
        VALUES (p_customer_id, v_price * p_quantity, 'pending');
        
        SET p_order_id = LAST_INSERT_ID();
        
        -- Add order item
        INSERT INTO order_items (order_id, product_id, quantity, unit_price)
        VALUES (p_order_id, p_product_id, p_quantity, v_price);
        
        -- Update stock
        UPDATE products SET stock = stock - p_quantity WHERE id = p_product_id;
        
        SET p_message = 'Order created successfully';
    END IF;
END //
DELIMITER ;
```

**User Registration:**
```sql
DELIMITER //
CREATE PROCEDURE RegisterUser(
    IN p_username VARCHAR(50),
    IN p_email VARCHAR(100),
    OUT p_user_id INT,
    OUT p_success BOOLEAN
)
BEGIN
    DECLARE existing_count INT;
    
    -- Check if username or email exists
    SELECT COUNT(*) INTO existing_count 
    FROM users 
    WHERE username = p_username OR email = p_email;
    
    IF existing_count > 0 THEN
        SET p_user_id = NULL;
        SET p_success = FALSE;
    ELSE
        INSERT INTO users (username, email, created_at)
        VALUES (p_username, p_email, NOW());
        
        SET p_user_id = LAST_INSERT_ID();
        SET p_success = TRUE;
    END IF;
END //
DELIMITER ;
```

### Managing Procedures

**View Procedures:**
```sql
SHOW PROCEDURE STATUS WHERE Db = 'your_database';

-- View definition
SHOW CREATE PROCEDURE ProcessOrder;
```

**Drop Procedure:**
```sql
DROP PROCEDURE IF EXISTS ProcessOrder;
```

**Alter (Must Drop and Recreate):**
```sql
DROP PROCEDURE IF EXISTS MyProcedure;
CREATE PROCEDURE MyProcedure() ...
```

### Error Handling

```sql
DELIMITER //
CREATE PROCEDURE SafeTransfer(
    IN from_account INT,
    IN to_account INT,
    IN amount DECIMAL(10,2)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SELECT 'Transaction failed' AS message;
    END;
    
    START TRANSACTION;
    
    UPDATE accounts SET balance = balance - amount WHERE id = from_account;
    UPDATE accounts SET balance = balance + amount WHERE id = to_account;
    
    COMMIT;
    SELECT 'Transfer successful' AS message;
END //
DELIMITER ;
```

### Best Practices

1. **Name procedures clearly** (verb_noun: GetEmployees, ProcessOrder)
2. **Document with comments**
3. **Use transactions** for multi-statement operations
4. **Validate input parameters**
5. **Handle errors** with exception handlers
6. **Avoid very long procedures** (split into smaller ones)
7. **Test thoroughly** before deployment

## Summary

- Stored procedures encapsulate SQL logic in the database
- Parameters can be IN (input), OUT (output), or INOUT (both)
- Variables (DECLARE) store intermediate values
- Control flow includes IF, CASE, LOOP, and WHILE
- Use transactions and error handlers for reliability
- Procedures improve performance, security, and maintainability

## Additional Resources

- [MySQL Stored Procedure Reference](https://dev.mysql.com/doc/refman/8.0/en/stored-routines.html)
- [Stored Procedure Tutorial](https://www.mysqltutorial.org/mysql-stored-procedure-tutorial.aspx)
- [Error Handling in Procedures](https://dev.mysql.com/doc/refman/8.0/en/declare-handler.html)
