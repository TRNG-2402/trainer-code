# Scalar Functions

## Learning Objectives

- Understand the difference between scalar and aggregate functions
- Apply string manipulation functions
- Use date and time functions
- Apply numeric and conversion functions

## Why This Matters

While aggregate functions work on groups of rows, scalar functions operate on individual values. They help you transform, format, and manipulate data within your queries. Scalar functions are essential for data cleaning, formatting output, and performing calculations.

## The Concept

### Scalar vs. Aggregate Functions

| Type | Input | Output | Example |
|------|-------|--------|---------|
| Aggregate | Many rows | Single value | SUM(column) |
| Scalar | Single value | Single value | UPPER(column) |

```sql
-- Aggregate: one result from many rows
SELECT AVG(salary) FROM employees;  -- 65000

-- Scalar: applied to each row
SELECT UPPER(name) FROM employees;  -- ALICE, BOB, CAROL
```

### String Functions

**Case Functions:**
```sql
SELECT UPPER('hello');    -- HELLO
SELECT LOWER('HELLO');    -- hello
SELECT INITCAP('hello');  -- Hello (PostgreSQL; use workaround for MySQL)
```

**Length:**
```sql
SELECT LENGTH('Hello');      -- 5 (bytes)
SELECT CHAR_LENGTH('Hello'); -- 5 (characters)
```

**Trimming:**
```sql
SELECT TRIM('  hello  ');       -- 'hello'
SELECT LTRIM('  hello');        -- 'hello'
SELECT RTRIM('hello  ');        -- 'hello'
SELECT TRIM(BOTH 'x' FROM 'xxxhelloxxx');  -- 'hello'
```

**Substring:**
```sql
SELECT SUBSTRING('Hello World', 1, 5);   -- 'Hello'
SELECT SUBSTRING('Hello World', 7);      -- 'World'
SELECT LEFT('Hello World', 5);           -- 'Hello'
SELECT RIGHT('Hello World', 5);          -- 'World'
```

**Concatenation:**
```sql
SELECT CONCAT('Hello', ' ', 'World');        -- 'Hello World'
SELECT CONCAT_WS(', ', 'Alice', 'Bob');      -- 'Alice, Bob'
SELECT 'Hello' || ' World';                   -- (PostgreSQL syntax)
```

**Find and Replace:**
```sql
SELECT LOCATE('World', 'Hello World');        -- 7 (position)
SELECT REPLACE('Hello World', 'World', 'SQL'); -- 'Hello SQL'
SELECT INSTR('Hello World', 'o');             -- 5 (first occurrence)
```

**Padding:**
```sql
SELECT LPAD('42', 5, '0');   -- '00042'
SELECT RPAD('Hi', 5, '.');   -- 'Hi...'
```

**Practical String Examples:**
```sql
-- Format names
SELECT 
    CONCAT(UPPER(LEFT(first_name, 1)), LOWER(SUBSTRING(first_name, 2))) AS formatted_name
FROM employees;

-- Extract domain from email
SELECT 
    SUBSTRING(email, LOCATE('@', email) + 1) AS domain
FROM customers;

-- Clean phone numbers
SELECT 
    REPLACE(REPLACE(REPLACE(phone, '-', ''), '(', ''), ')', '') AS clean_phone
FROM contacts;
```

### Date and Time Functions

**Current Date/Time:**
```sql
SELECT NOW();              -- 2024-06-15 14:30:00
SELECT CURRENT_DATE();     -- 2024-06-15
SELECT CURRENT_TIME();     -- 14:30:00
SELECT CURRENT_TIMESTAMP;  -- 2024-06-15 14:30:00
```

**Extract Components:**
```sql
SELECT YEAR('2024-06-15');      -- 2024
SELECT MONTH('2024-06-15');     -- 6
SELECT DAY('2024-06-15');       -- 15
SELECT HOUR('14:30:00');        -- 14
SELECT MINUTE('14:30:00');      -- 30
SELECT SECOND('14:30:00');      -- 0
SELECT DAYOFWEEK('2024-06-15'); -- 7 (Saturday)
SELECT DAYNAME('2024-06-15');   -- Saturday
SELECT MONTHNAME('2024-06-15'); -- June
```

**Date Arithmetic:**
```sql
-- Add/subtract intervals
SELECT DATE_ADD('2024-06-15', INTERVAL 7 DAY);    -- 2024-06-22
SELECT DATE_SUB('2024-06-15', INTERVAL 1 MONTH);  -- 2024-05-15
SELECT '2024-06-15' + INTERVAL 7 DAY;             -- 2024-06-22

-- Difference between dates
SELECT DATEDIFF('2024-06-15', '2024-06-01');      -- 14 (days)
SELECT TIMESTAMPDIFF(MONTH, '2024-01-01', '2024-06-15'); -- 5
```

**Formatting:**
```sql
-- Format date to string
SELECT DATE_FORMAT('2024-06-15', '%M %d, %Y');    -- June 15, 2024
SELECT DATE_FORMAT('2024-06-15', '%Y-%m-%d');     -- 2024-06-15
SELECT DATE_FORMAT(NOW(), '%W, %M %d');           -- Saturday, June 15

-- Parse string to date
SELECT STR_TO_DATE('June 15, 2024', '%M %d, %Y'); -- 2024-06-15
```

**Common Format Codes:**
| Code | Meaning | Example |
|------|---------|---------|
| %Y | 4-digit year | 2024 |
| %y | 2-digit year | 24 |
| %M | Month name | June |
| %m | Month number | 06 |
| %d | Day of month | 15 |
| %W | Weekday name | Saturday |
| %H | Hour (24h) | 14 |
| %i | Minutes | 30 |
| %s | Seconds | 00 |

**Practical Date Examples:**
```sql
-- Orders from the last 30 days
SELECT * FROM orders
WHERE order_date >= DATE_SUB(CURRENT_DATE(), INTERVAL 30 DAY);

-- Age calculation
SELECT 
    name,
    birth_date,
    TIMESTAMPDIFF(YEAR, birth_date, CURDATE()) AS age
FROM customers;

-- First day of current month
SELECT DATE_FORMAT(CURRENT_DATE(), '%Y-%m-01') AS first_of_month;
```

### Numeric Functions

**Rounding:**
```sql
SELECT ROUND(3.14159, 2);    -- 3.14
SELECT CEIL(3.2);            -- 4 (round up)
SELECT FLOOR(3.9);           -- 3 (round down)
SELECT TRUNCATE(3.14159, 2); -- 3.14 (no rounding)
```

**Absolute and Sign:**
```sql
SELECT ABS(-5);    -- 5
SELECT SIGN(-10);  -- -1
SELECT SIGN(10);   -- 1
SELECT SIGN(0);    -- 0
```

**Mathematical:**
```sql
SELECT POWER(2, 10);  -- 1024
SELECT SQRT(16);      -- 4
SELECT MOD(10, 3);    -- 1 (remainder)
SELECT 10 % 3;        -- 1 (same as MOD)
SELECT RAND();        -- Random 0-1
```

### Conversion Functions

**Type Conversion:**
```sql
-- Convert to specific type
SELECT CAST('123' AS SIGNED);        -- 123 (integer)
SELECT CAST(3.14 AS CHAR);           -- '3.14'
SELECT CAST('2024-06-15' AS DATE);   -- 2024-06-15

-- Convert implicitly
SELECT '100' + 0;   -- 100 (string to number)
SELECT 100 + '';    -- '100' (number to string)
```

**NULL Handling:**
```sql
-- Return first non-NULL value
SELECT COALESCE(NULL, NULL, 'default');  -- 'default'

-- Replace NULL with value
SELECT IFNULL(phone, 'N/A') FROM contacts;

-- Conditional NULL
SELECT NULLIF(value1, value2);  -- NULL if equal, else value1
```

**Conditional:**
```sql
-- IF function
SELECT IF(score >= 60, 'Pass', 'Fail') FROM exams;

-- CASE expression
SELECT 
    name,
    CASE 
        WHEN score >= 90 THEN 'A'
        WHEN score >= 80 THEN 'B'
        WHEN score >= 70 THEN 'C'
        WHEN score >= 60 THEN 'D'
        ELSE 'F'
    END AS grade
FROM students;
```

## Summary

- Scalar functions operate on individual values, not groups
- **String functions**: UPPER, LOWER, CONCAT, SUBSTRING, TRIM, REPLACE
- **Date functions**: NOW, YEAR, MONTH, DATE_ADD, DATEDIFF, DATE_FORMAT
- **Numeric functions**: ROUND, CEIL, FLOOR, ABS, POWER, MOD
- **Conversion**: CAST, COALESCE, IFNULL, CASE
- Combine functions for complex data transformations

## Additional Resources

- [MySQL String Functions](https://dev.mysql.com/doc/refman/8.0/en/string-functions.html)
- [MySQL Date Functions](https://dev.mysql.com/doc/refman/8.0/en/date-and-time-functions.html)
- [MySQL Numeric Functions](https://dev.mysql.com/doc/refman/8.0/en/numeric-functions.html)
