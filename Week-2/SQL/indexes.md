# Indexes

## Learning Objectives

- Understand what indexes are and how they improve performance
- Learn different types of indexes
- Create and manage indexes effectively
- Know when to use and when to avoid indexes

## Why This Matters

As tables grow to millions of rows, queries without proper indexing become unacceptably slow. Indexes are the primary tool for query optimization. Understanding indexes is essential for building high-performance database applications.

## The Concept

### What is an Index?

An index is a data structure that improves the speed of data retrieval. Think of it like a book's index: instead of reading every page, you look up a term in the index to find the exact page.

**Without Index:**
```
SELECT * FROM employees WHERE last_name = 'Smith';
-- Must scan all rows (table scan): O(n)
```

**With Index on last_name:**
```
-- Uses index to find matching rows quickly: O(log n)
```

### How Indexes Work

Most indexes use B-tree (balanced tree) structure:

```
                    [M]
                   /   \
              [D-G]     [P-T]
             /  |  \       \
          [A-C][E-F][H-L]  [Q-S]
```

- Fast lookup: O(log n)
- Ordered access for range queries
- Overhead for insertions and updates

### Creating Indexes

**Basic Index:**
```sql
CREATE INDEX idx_last_name ON employees(last_name);
```

**Unique Index:**
```sql
CREATE UNIQUE INDEX idx_email ON employees(email);
```

**Composite Index (Multi-column):**
```sql
CREATE INDEX idx_name ON employees(last_name, first_name);
```

**Index at Table Creation:**
```sql
CREATE TABLE products (
    id INT PRIMARY KEY,
    name VARCHAR(100),
    sku VARCHAR(50),
    category_id INT,
    INDEX idx_category (category_id),
    UNIQUE INDEX idx_sku (sku)
);
```

### Types of Indexes

| Type | Description | Use Case |
|------|-------------|----------|
| PRIMARY | Unique, not null, one per table | Row identification |
| UNIQUE | Unique values allowed | Prevent duplicates |
| INDEX | Regular non-unique index | Speed up lookups |
| FULLTEXT | Text search index | Search in text fields |
| SPATIAL | Geographic data | Location queries |

### Viewing Indexes

```sql
-- Show all indexes on a table
SHOW INDEX FROM employees;

-- Using information_schema
SELECT * FROM information_schema.STATISTICS
WHERE table_name = 'employees';
```

### Dropping Indexes

```sql
DROP INDEX idx_last_name ON employees;

-- Or using ALTER TABLE
ALTER TABLE employees DROP INDEX idx_last_name;
```

### Index Performance Analysis

Use EXPLAIN to see how queries use indexes:

```sql
EXPLAIN SELECT * FROM employees WHERE last_name = 'Smith';
```

**Key Columns to Check:**
- `type`: ALL (full scan) vs. ref/range/const (using index)
- `key`: Which index is used
- `rows`: Estimated rows scanned

```sql
-- Without index
EXPLAIN SELECT * FROM employees WHERE last_name = 'Smith';
-- type: ALL, key: NULL, rows: 1000000 (bad)

-- With index
-- type: ref, key: idx_last_name, rows: 50 (good)
```

### Composite Index Order Matters

```sql
CREATE INDEX idx_name ON employees(last_name, first_name);
```

This index helps:
```sql
WHERE last_name = 'Smith'                    -- Uses index
WHERE last_name = 'Smith' AND first_name = 'John'  -- Uses index
```

This does NOT help:
```sql
WHERE first_name = 'John'   -- Cannot use index (leftmost prefix rule)
```

### When to Create Indexes

**Good Candidates:**
- Columns in WHERE clauses
- Columns used in JOIN conditions
- Columns in ORDER BY
- Columns with high cardinality (many unique values)
- Foreign key columns

**Avoid Indexing:**
- Small tables (table scan is fast enough)
- Columns with low cardinality (few unique values)
- Columns rarely used in queries
- Columns frequently updated

### Index Trade-offs

**Benefits:**
- Faster SELECT queries
- Faster ORDER BY
- Faster joins

**Costs:**
- Slower INSERT/UPDATE/DELETE (index must be updated)
- Storage space
- Memory usage

### Best Practices

```sql
-- 1. Index foreign keys
ALTER TABLE orders ADD INDEX idx_customer (customer_id);

-- 2. Cover common queries with composite indexes
CREATE INDEX idx_status_date ON orders(status, order_date);

-- 3. Use EXPLAIN to verify index usage
EXPLAIN SELECT * FROM orders WHERE status = 'pending' ORDER BY order_date;

-- 4. Remove unused indexes
DROP INDEX idx_unused ON products;
```

### Covering Indexes

When an index contains all columns needed by a query, MySQL can retrieve data from the index alone:

```sql
CREATE INDEX idx_cover ON employees(department_id, salary);

-- This query uses only the index (no table access)
SELECT department_id, salary FROM employees WHERE department_id = 5;
```

## Summary

- Indexes speed up data retrieval using tree structures
- PRIMARY, UNIQUE, and regular indexes serve different purposes
- Composite indexes follow the leftmost prefix rule
- Use EXPLAIN to analyze query performance
- Balance query speed against write overhead
- Index columns used in WHERE, JOIN, and ORDER BY

## Additional Resources

- [MySQL Index Reference](https://dev.mysql.com/doc/refman/8.0/en/optimization-indexes.html)
- [Use The Index, Luke](https://use-the-index-luke.com/)
- [MySQL EXPLAIN Guide](https://dev.mysql.com/doc/refman/8.0/en/explain.html)
