# User-Defined Functions

## Learning Objectives

- Understand what user-defined functions (UDFs) are
- Differentiate between functions and stored procedures
- Create scalar functions
- Apply functions in SQL queries

## Why This Matters

User-defined functions extend SQL with custom calculations and logic. Unlike procedures, functions can be used directly in SELECT statements, WHERE clauses, and expressions. They enable reusable, testable business logic that integrates seamlessly with your queries.

## The Concept

### What is a User-Defined Function?

A UDF is a routine that:
- Accepts parameters
- Performs calculations
- Returns a single value (scalar function)
- Can be used in SQL expressions

### Functions vs. Stored Procedures

| Feature | Function | Stored Procedure |
|---------|----------|------------------|
| Return value | Required (single value) | Optional (multiple via OUT) |
| Use in SELECT | Yes | No |
| Use in WHERE | Yes | No |
| DML statements | Limited | Allowed |
| Transaction control | Not allowed | Allowed |
| CALL statement | No | Yes |

```sql
-- Function: use in expressions
SELECT full_name(first_name, last_name) FROM employees;

-- Procedure: call separately
CALL GetEmployeeReport(@result);
```

### Creating a Basic Function

```sql
DELIMITER //
CREATE FUNCTION calculate_tax(amount DECIMAL(10,2))
RETURNS DECIMAL(10,2)
DETERMINISTIC
BEGIN
    RETURN amount * 0.08;
END //
DELIMITER ;

-- Use the function
SELECT product_name, price, calculate_tax(price) AS tax FROM products;
```

### Function Characteristics

**DETERMINISTIC vs. NOT DETERMINISTIC:**
- DETERMINISTIC: Same inputs always produce same output
- NOT DETERMINISTIC: Results may vary (e.g., uses RAND(), NOW())

```sql
-- Deterministic: always same result
CREATE FUNCTION double_value(n INT)
RETURNS INT
DETERMINISTIC
BEGIN
    RETURN n * 2;
END;

-- Not deterministic: depends on current data
CREATE FUNCTION get_order_count()
RETURNS INT
NOT DETERMINISTIC
READS SQL DATA
BEGIN
    RETURN (SELECT COUNT(*) FROM orders);
END;
```

**Data Access Characteristics:**
- NO SQL: No SQL statements
- READS SQL DATA: Only SELECT
- MODIFIES SQL DATA: INSERT, UPDATE, DELETE (limited in functions)
- CONTAINS SQL: Default, non-data SQL

### Practical Function Examples

**Full Name Function:**
```sql
DELIMITER //
CREATE FUNCTION full_name(fname VARCHAR(50), lname VARCHAR(50))
RETURNS VARCHAR(101)
DETERMINISTIC
BEGIN
    RETURN CONCAT(fname, ' ', lname);
END //
DELIMITER ;

-- Usage
SELECT full_name(first_name, last_name) AS name FROM employees;
```

**Age Calculation:**
```sql
DELIMITER //
CREATE FUNCTION calculate_age(birthdate DATE)
RETURNS INT
DETERMINISTIC
BEGIN
    RETURN TIMESTAMPDIFF(YEAR, birthdate, CURDATE());
END //
DELIMITER ;

-- Usage
SELECT name, calculate_age(birth_date) AS age FROM customers;
```

**Order Status Text:**
```sql
DELIMITER //
CREATE FUNCTION get_status_label(status_code CHAR(1))
RETURNS VARCHAR(20)
DETERMINISTIC
BEGIN
    DECLARE label VARCHAR(20);
    
    CASE status_code
        WHEN 'P' THEN SET label = 'Pending';
        WHEN 'A' THEN SET label = 'Approved';
        WHEN 'S' THEN SET label = 'Shipped';
        WHEN 'D' THEN SET label = 'Delivered';
        WHEN 'C' THEN SET label = 'Cancelled';
        ELSE SET label = 'Unknown';
    END CASE;
    
    RETURN label;
END //
DELIMITER ;

-- Usage
SELECT order_id, get_status_label(status) AS status FROM orders;
```

**Discount Calculation:**
```sql
DELIMITER //
CREATE FUNCTION calculate_discount(
    subtotal DECIMAL(10,2),
    customer_type VARCHAR(20)
)
RETURNS DECIMAL(10,2)
DETERMINISTIC
BEGIN
    DECLARE discount_rate DECIMAL(5,2);
    
    CASE customer_type
        WHEN 'VIP' THEN SET discount_rate = 0.20;
        WHEN 'Premium' THEN SET discount_rate = 0.10;
        WHEN 'Regular' THEN SET discount_rate = 0.05;
        ELSE SET discount_rate = 0.00;
    END CASE;
    
    RETURN subtotal * discount_rate;
END //
DELIMITER ;

-- Usage
SELECT 
    order_id,
    subtotal,
    calculate_discount(subtotal, customer_type) AS discount,
    subtotal - calculate_discount(subtotal, customer_type) AS final_total
FROM orders o
JOIN customers c ON o.customer_id = c.id;
```

**Formatting Functions:**
```sql
DELIMITER //
CREATE FUNCTION format_phone(phone VARCHAR(20))
RETURNS VARCHAR(20)
DETERMINISTIC
BEGIN
    DECLARE clean_phone VARCHAR(20);
    
    -- Remove non-numeric characters
    SET clean_phone = REGEXP_REPLACE(phone, '[^0-9]', '');
    
    -- Format as (XXX) XXX-XXXX
    IF LENGTH(clean_phone) = 10 THEN
        RETURN CONCAT('(', LEFT(clean_phone, 3), ') ', 
                      SUBSTRING(clean_phone, 4, 3), '-', 
                      RIGHT(clean_phone, 4));
    ELSE
        RETURN phone;
    END IF;
END //
DELIMITER ;
```

### Using Functions in Queries

```sql
-- In SELECT
SELECT name, calculate_age(birth_date) AS age FROM customers;

-- In WHERE
SELECT * FROM customers WHERE calculate_age(birth_date) >= 21;

-- In ORDER BY
SELECT * FROM customers ORDER BY calculate_age(birth_date) DESC;

-- In computed columns
SELECT 
    product_name,
    price,
    calculate_tax(price) AS tax,
    price + calculate_tax(price) AS total
FROM products;
```

### Managing Functions

**View Functions:**
```sql
SHOW FUNCTION STATUS WHERE Db = 'your_database';

-- View definition
SHOW CREATE FUNCTION calculate_tax;
```

**Drop Function:**
```sql
DROP FUNCTION IF EXISTS calculate_tax;
```

### Limitations

1. **Cannot use COMMIT or ROLLBACK**
2. **Cannot call procedures** (in most cases)
3. **Limited DML operations** in MySQL
4. **Must return a value**
5. **Cannot return result sets** (tables)

### Best Practices

1. **Keep functions simple** and focused
2. **Use DETERMINISTIC** when applicable (helps optimization)
3. **Specify data access** characteristics
4. **Handle NULL inputs** appropriately
5. **Name descriptively** (calculate_*, get_*, format_*)
6. **Test edge cases** thoroughly

```sql
-- Good: NULL-safe function
DELIMITER //
CREATE FUNCTION safe_divide(a DECIMAL(10,2), b DECIMAL(10,2))
RETURNS DECIMAL(10,4)
DETERMINISTIC
BEGIN
    IF b = 0 OR b IS NULL THEN
        RETURN NULL;
    END IF;
    RETURN a / b;
END //
DELIMITER ;
```

## Summary

- UDFs encapsulate reusable calculations and logic
- Functions return exactly one value (scalar)
- Can be used in SELECT, WHERE, ORDER BY, and expressions
- DETERMINISTIC functions always return same output for same input
- Functions are more limited than procedures (no transactions, limited DML)
- Use appropriate data access characteristics

## Additional Resources

- [MySQL Function Reference](https://dev.mysql.com/doc/refman/8.0/en/create-function-udf.html)
- [Stored Functions Tutorial](https://www.mysqltutorial.org/mysql-stored-function/)
- [Function vs Procedure](https://dev.mysql.com/doc/refman/8.0/en/stored-routines.html)
