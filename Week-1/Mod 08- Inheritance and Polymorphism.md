# Module 8: Inheritance & Polymorphism

---

## 8.1 Inheritance

A derived (child) class acquires members from a base (parent) class using `:`.

```csharp
public class Animal
{
    public string Name { get; set; }
    public void Eat() => Console.WriteLine($"{Name} is eating.");
}

public class Dog : Animal
{
    public void Bark() => Console.WriteLine($"{Name} says: Woof!");
}

var dog = new Dog { Name = "Rex" };
dog.Eat();   // Inherited from Animal
dog.Bark();  // Defined in Dog
```

**Rules:**

- C# supports **single inheritance** — one base class only
- All classes ultimately inherit from `System.Object`
- Use the **"is-a" test**: a Dog *is an* Animal ✓. An Engine *is a* Car ✗ (use composition instead).

---

## 8.2 Constructors and `base`

The base class constructor runs **first**. Use `base(...)` to call a parameterized base constructor:

```csharp
public class Animal
{
    public string Name { get; set; }
    public Animal(string name) { Name = name; }
}

public class Dog : Animal
{
    public string Breed { get; set; }

    public Dog(string name, string breed) : base(name)  // Calls Animal(name)
    {
        Breed = breed;
    }
}

var dog = new Dog("Rex", "German Shepherd");
```

> If the base has only a parameterized constructor and you don't call `base(...)`, you get a compiler error.

---

## 8.3 Overriding (Polymorphism)

Mark base methods `virtual`, derived methods `override`:

```csharp
public class Animal
{
    public string Name { get; set; }
    public virtual string Speak() => $"{Name} makes a sound.";
}

public class Dog : Animal
{
    public override string Speak() => $"{Name} says: Woof!";
}

public class Cat : Animal
{
    public override string Speak() => $"{Name} says: Meow!";
}
```

### Polymorphism in Action

```csharp
List<Animal> animals = new()
{
    new Dog { Name = "Rex" },
    new Cat { Name = "Whiskers" },
    new Animal { Name = "Unknown" }
};

foreach (var animal in animals)
    Console.WriteLine(animal.Speak());

// Output:
// Rex says: Woof!
// Whiskers says: Meow!
// Unknown makes a sound.
```

### Calling the Base Implementation

```csharp
public override string Speak()
{
    string baseMsg = base.Speak();
    return $"{baseMsg} Actually, Woof!";
}
```

### `override` vs. `new`

- **`override`** — replaces the method; polymorphism works ✓
- **`new`** — hides the method; polymorphism breaks ✗

Always prefer `override`.

---

## 8.4 Common Modifiers

### `abstract`

Can't be instantiated. Abstract methods have no body — derived classes **must** override them.

```csharp
public abstract class Shape
{
    public abstract double Area();   // No implementation
}
```

### `sealed`

Can't be inherited (class) or further overridden (method):

```csharp
public sealed class FinalClass { }
// class Child : FinalClass { }  // Error

public sealed override string Speak() => "Woof!";
// Can't override Speak in further derived classes
```

### `protected`

Accessible within the class **and its derived classes**, but not from outside:

```csharp
public class Animal
{
    protected int _age;  // Derived classes can access; external code can't
}
```

### `readonly`

Can only be assigned at declaration or in the constructor:

```csharp
public readonly string Env;
public Config(string env) { Env = env; }
// Env = "prod";  // Error elsewhere
```

---

## 8.5 Upcasting and Downcasting

### Upcasting (always safe, implicit)

Treat a derived type as its base type:

```csharp
Dog dog = new Dog { Name = "Rex" };
Animal animal = dog;       // Upcast — implicit
animal.Speak();            // Still calls Dog.Speak() (polymorphism)
```

### Downcasting (can fail, explicit)

Treat a base reference as a derived type:

```csharp
Animal animal = new Dog { Name = "Rex" };

// Option 1: Direct cast — throws if wrong type
Dog dog = (Dog)animal;

// Option 2: 'as' — returns null if wrong type
Dog dog = animal as Dog;
if (dog != null) dog.Bark();

// Option 3: 'is' with pattern matching (PREFERRED)
if (animal is Dog d)
{
    d.Bark();  // d is already cast
}
```

---

## Key Takeaways

- **Inheritance:** `class Child : Parent` — models "is-a" relationships
- Base constructor runs first; call `base(...)` for parameterized constructors
- **`virtual` + `override`** enables polymorphism — different behavior through a common interface
- **`abstract`** = must inherit; **`sealed`** = can't inherit; **`protected`** = derived access only
- Use `is` with pattern matching for safe downcasting