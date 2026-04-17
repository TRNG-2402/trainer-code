# SQL Data Types

## Learning Objectives

- Understand the major categories of SQL data types
- Learn common numeric, string, and date/time types
- Choose appropriate data types for different scenarios
- Understand the implications of data type choices

## Why This Matters

Choosing the right data type is one of the most important decisions when designing a database. It affects:

- **Storage space**: Wrong types waste disk space
- **Performance**: Queries on appropriate types run faster
- **Data integrity**: Types prevent invalid data
- **Application behavior**: Types determine how data is processed

Selecting `VARCHAR(255)` for everything is a common beginner mistake that leads to poor database design.

## The Concept

### Data Type Categories

SQL data types fall into several categories:

| Category | Purpose | Examples |
|----------|---------|----------|
| Numeric | Numbers | INT, DECIMAL, FLOAT |
| String | Text | VARCHAR, CHAR, TEXT |
| Date/Time | Temporal data | DATE, TIME, DATETIME |
| Boolean | True/False | BOOLEAN, TINYINT(1) |
| Binary | Raw binary data | BLOB, BINARY |

### Numeric Types

**Integer Types**

| Type | Bytes | Range (Signed) | Use Case |
|------|-------|----------------|----------|
| TINYINT | 1 | -128 to 127 | Small counters, flags |
| SMALLINT | 2 | -32,768 to 32,767 | Limited ranges |
| MEDIUMINT | 3 | -8M to 8M | Medium values |
| INT | 4 | -2B to 2B | Most common for IDs |
| BIGINT | 8 | -9 quintillion+ | Very large numbers |

```sql
CREATE TABLE products (
    id INT UNSIGNED AUTO_INCREMENT,     -- Never negative
    quantity SMALLINT DEFAULT 0,         -- Stock count
    views BIGINT DEFAULT 0,              -- Could be very large
    PRIMARY KEY (id)
);
```

**UNSIGNED**: Doubles the positive range by eliminating negative numbers.

**Decimal Types**

| Type | Description | Use Case |
|------|-------------|----------|
| DECIMAL(M,D) | Exact precision | Money, financial data |
| FLOAT | 4-byte approximate | Scientific measurements |
| DOUBLE | 8-byte approximate | High-precision calculations |

```sql
-- DECIMAL(M, D): M = total digits, D = decimal places
-- DECIMAL(10, 2) can store up to 99999999.99

CREATE TABLE transactions (
    id INT PRIMARY KEY,
    amount DECIMAL(10, 2) NOT NULL,      -- Exact: $12345678.99
    tax_rate DECIMAL(5, 4),              -- Exact: 0.0825 (8.25%)
    approximate_value FLOAT              -- Approximate only
);
```

**Important**: Always use DECIMAL for money. FLOAT/DOUBLE can have rounding errors.

```sql
-- Bad: FLOAT can cause issues
SELECT 0.1 + 0.2 AS result;  -- Might show 0.30000000000000004

-- Good: DECIMAL is exact
DECIMAL(10,2) stores 0.30 exactly
```

### String Types

**Fixed vs. Variable Length**

| Type | Storage | Padding | Use Case |
|------|---------|---------|----------|
| CHAR(n) | Always n bytes | Yes (spaces) | Fixed-length: codes, abbreviations |
| VARCHAR(n) | Actual + 1-2 bytes | No | Variable-length: names, emails |

```sql
CREATE TABLE users (
    id INT PRIMARY KEY,
    country_code CHAR(2),           -- Always 2 chars: 'US', 'UK'
    state_code CHAR(3),             -- Always 3 chars: 'NYC', 'CAL'
    name VARCHAR(100),              -- Variable: 'Al' or 'Alexander'
    email VARCHAR(255)              -- Variable length
);
```

**CHAR(10)** always uses 10 bytes.  
**VARCHAR(10)** uses actual length + overhead (1-2 bytes).

**Text Types**

| Type | Max Length | Use Case |
|------|------------|----------|
| TINYTEXT | 255 bytes | Very short text |
| TEXT | 65,535 bytes | Articles, descriptions |
| MEDIUMTEXT | 16 MB | Large documents |
| LONGTEXT | 4 GB | Very large content |

```sql
CREATE TABLE articles (
    id INT PRIMARY KEY,
    title VARCHAR(200),
    summary VARCHAR(500),
    content TEXT,                    -- Full article body
    raw_html MEDIUMTEXT              -- Could be large
);
```

### Date and Time Types

| Type | Format | Range | Use Case |
|------|--------|-------|----------|
| DATE | YYYY-MM-DD | 1000-01-01 to 9999-12-31 | Birthdays, deadlines |
| TIME | HH:MM:SS | -838:59:59 to 838:59:59 | Duration, time of day |
| DATETIME | YYYY-MM-DD HH:MM:SS | 1000 to 9999 | Events, timestamps |
| TIMESTAMP | YYYY-MM-DD HH:MM:SS | 1970 to 2038 | Auto-updating times |
| YEAR | YYYY | 1901 to 2155 | Year only |

```sql
CREATE TABLE events (
    id INT PRIMARY KEY,
    event_name VARCHAR(100),
    event_date DATE,                    -- 2024-06-15
    start_time TIME,                    -- 14:30:00
    created_at DATETIME,                -- 2024-06-15 14:30:00
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

**DATETIME vs. TIMESTAMP:**
- TIMESTAMP converts to UTC and back (timezone-aware)
- DATETIME stores exactly what you provide
- TIMESTAMP has limited range (until 2038)
- Use TIMESTAMP for tracking modifications, DATETIME for events

### Boolean Type

MySQL uses `TINYINT(1)` for boolean values:

```sql
CREATE TABLE features (
    id INT PRIMARY KEY,
    name VARCHAR(50),
    is_enabled BOOLEAN DEFAULT TRUE,    -- Stored as TINYINT(1)
    is_premium TINYINT(1) DEFAULT 0     -- 0 = false, 1 = true
);

-- Query examples
SELECT * FROM features WHERE is_enabled = TRUE;
SELECT * FROM features WHERE is_premium = 1;
```

### Binary Types

| Type | Max Length | Use Case |
|------|------------|----------|
| BINARY(n) | n bytes | Fixed-length binary (hashes) |
| VARBINARY(n) | n bytes | Variable binary |
| BLOB | 65,535 bytes | Images, files |
| LONGBLOB | 4 GB | Large binary data |

```sql
CREATE TABLE files (
    id INT PRIMARY KEY,
    filename VARCHAR(255),
    content_type VARCHAR(100),
    file_data MEDIUMBLOB              -- Store file content
);
```

**Note**: Storing large files in databases is often not recommended. Consider storing file paths and keeping files in file storage.

### Choosing the Right Type

**Decision Guide:**

```
Is it a number?
  |
  +-- Whole number? --> TINYINT/SMALLINT/INT/BIGINT
  |
  +-- Money/exact decimal? --> DECIMAL(M,D)
  |
  +-- Scientific/approximate? --> FLOAT/DOUBLE

Is it text?
  |
  +-- Fixed length (codes)? --> CHAR(n)
  |
  +-- Variable but bounded? --> VARCHAR(n)
  |
  +-- Unbounded/long? --> TEXT/MEDIUMTEXT

Is it a date/time?
  |
  +-- Date only? --> DATE
  |
  +-- Time only? --> TIME
  |
  +-- Both needed? --> DATETIME/TIMESTAMP
  |
  +-- Auto-update needed? --> TIMESTAMP
```

### Common Mistakes

| Mistake | Problem | Better Choice |
|---------|---------|---------------|
| `VARCHAR(255)` for everything | Wastes space, unclear intent | Size to actual need |
| `FLOAT` for money | Rounding errors | `DECIMAL(10,2)` |
| `TEXT` for short fields | Performance overhead | `VARCHAR(n)` |
| `VARCHAR` for fixed codes | Inconsistent data | `CHAR(n)` |
| Giant columns | Slow queries | Right-size columns |

### MySQL-Specific Types

MySQL offers additional types:

```sql
-- ENUM: Predefined list of values
status ENUM('pending', 'approved', 'rejected')

-- SET: Multiple values from a list
tags SET('featured', 'sale', 'new', 'popular')

-- JSON: Native JSON storage and querying
metadata JSON
```

## Summary

- Choose data types based on actual data requirements
- Use INT for most numeric IDs, BIGINT for large-scale systems
- Use DECIMAL for exact values like money
- Use VARCHAR for variable-length strings, CHAR for fixed-length codes
- Use DATE, TIME, or DATETIME based on what you need to store
- TIMESTAMP auto-updates and is timezone-aware
- Right-sizing data types improves performance and storage

## Additional Resources

- [MySQL Data Types Reference](https://dev.mysql.com/doc/refman/8.0/en/data-types.html)
- [Choosing the Right MySQL Data Type](https://www.mysqltutorial.org/mysql-data-types.aspx)
- [SQL Data Types Comparison](https://www.w3schools.com/sql/sql_datatypes.asp)
