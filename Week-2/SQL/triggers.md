# Triggers

## Learning Objectives

- Understand what triggers are and when to use them
- Create BEFORE and AFTER triggers
- Apply triggers for INSERT, UPDATE, and DELETE operations
- Know trigger best practices and limitations

## Why This Matters

Triggers automate actions in response to data changes. They can enforce business rules, maintain audit trails, and synchronize related data without application code changes. Understanding triggers helps you implement database-level automation.

## The Concept

### What is a Trigger?

A trigger is a stored procedure that automatically executes when a specified event occurs on a table.

```
Event (INSERT/UPDATE/DELETE) --> Trigger Fires --> Actions Execute
```

### Trigger Timing

| Timing | Description |
|--------|-------------|
| BEFORE | Executes before the triggering statement |
| AFTER | Executes after the triggering statement |

**BEFORE triggers:**
- Can modify the row being inserted/updated
- Can prevent the operation (by raising an error)

**AFTER triggers:**
- Row changes are complete
- Good for logging and synchronization

### Trigger Events

| Event | When |
|-------|------|
| INSERT | New row added |
| UPDATE | Existing row modified |
| DELETE | Row removed |

### Creating Triggers

**Basic Syntax:**
```sql
CREATE TRIGGER trigger_name
{BEFORE | AFTER} {INSERT | UPDATE | DELETE}
ON table_name
FOR EACH ROW
trigger_body;
```

**Simple Example:**
```sql
DELIMITER //
CREATE TRIGGER before_employee_insert
BEFORE INSERT ON employees
FOR EACH ROW
BEGIN
    SET NEW.created_at = NOW();
    SET NEW.updated_at = NOW();
END //
DELIMITER ;
```

### NEW and OLD References

| Reference | Available In | Description |
|-----------|--------------|-------------|
| NEW | INSERT, UPDATE | The new row values |
| OLD | UPDATE, DELETE | The original row values |

```sql
-- Access new values being inserted
DELIMITER //
CREATE TRIGGER set_defaults
BEFORE INSERT ON orders
FOR EACH ROW
BEGIN
    IF NEW.status IS NULL THEN
        SET NEW.status = 'pending';
    END IF;
END //
DELIMITER ;
```

### Practical Trigger Examples

**Audit Trail:**
```sql
CREATE TABLE employee_audit (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employee_id INT,
    action VARCHAR(10),
    old_salary DECIMAL(10,2),
    new_salary DECIMAL(10,2),
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    changed_by VARCHAR(50)
);

DELIMITER //
CREATE TRIGGER after_employee_update
AFTER UPDATE ON employees
FOR EACH ROW
BEGIN
    IF OLD.salary != NEW.salary THEN
        INSERT INTO employee_audit (employee_id, action, old_salary, new_salary, changed_by)
        VALUES (OLD.id, 'UPDATE', OLD.salary, NEW.salary, USER());
    END IF;
END //
DELIMITER ;
```

**Auto-Update Timestamps:**
```sql
DELIMITER //
CREATE TRIGGER before_product_update
BEFORE UPDATE ON products
FOR EACH ROW
BEGIN
    SET NEW.updated_at = NOW();
END //
DELIMITER ;
```

**Inventory Management:**
```sql
DELIMITER //
CREATE TRIGGER after_order_item_insert
AFTER INSERT ON order_items
FOR EACH ROW
BEGIN
    UPDATE products 
    SET stock = stock - NEW.quantity
    WHERE id = NEW.product_id;
END //
DELIMITER ;

CREATE TRIGGER after_order_item_delete
AFTER DELETE ON order_items
FOR EACH ROW
BEGIN
    UPDATE products 
    SET stock = stock + OLD.quantity
    WHERE id = OLD.product_id;
END //
DELIMITER ;
```

**Data Validation:**
```sql
DELIMITER //
CREATE TRIGGER before_order_insert
BEFORE INSERT ON orders
FOR EACH ROW
BEGIN
    IF NEW.total < 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Order total cannot be negative';
    END IF;
END //
DELIMITER ;
```

### Managing Triggers

**View Triggers:**
```sql
SHOW TRIGGERS;

-- Filter by table
SHOW TRIGGERS LIKE 'employees';

-- From information_schema
SELECT * FROM information_schema.TRIGGERS
WHERE TRIGGER_SCHEMA = 'your_database';
```

**Drop Trigger:**
```sql
DROP TRIGGER IF EXISTS before_employee_insert;
```

### Trigger Limitations

1. **Cannot call stored procedures** that return result sets
2. **Cannot use transactions** (COMMIT/ROLLBACK) inside triggers
3. **Cannot modify the same table** being triggered (in most cases)
4. **Performance impact** on high-volume tables
5. **Debugging is difficult** (no direct way to step through)

### Best Practices

1. **Keep triggers simple** and fast
2. **Document trigger behavior** clearly
3. **Avoid cascading triggers** (trigger A fires trigger B)
4. **Test thoroughly** with edge cases
5. **Consider alternatives** (application logic, stored procedures)
6. **Name triggers descriptively**

```sql
-- Good naming: timing_table_event
before_employee_insert
after_order_delete
before_product_update
```

### When to Use Triggers

**Good Use Cases:**
- Automatic timestamps
- Audit logging
- Maintaining summary tables
- Enforcing complex business rules
- Synchronizing denormalized data

**Consider Alternatives When:**
- Logic is complex
- External systems need notification
- Performance is critical
- Debugging is frequent

## Summary

- Triggers automatically execute on INSERT, UPDATE, or DELETE
- BEFORE triggers can modify data; AFTER triggers are for logging
- NEW references incoming data; OLD references existing data
- Use triggers for auditing, validation, and automation
- Keep triggers simple and well-documented
- Consider performance impact on high-volume tables

## Additional Resources

- [MySQL Trigger Reference](https://dev.mysql.com/doc/refman/8.0/en/triggers.html)
- [Trigger Best Practices](https://www.mysqltutorial.org/mysql-triggers/)
- [When Not to Use Triggers](https://use-the-index-luke.com/sql/dml/triggers)
