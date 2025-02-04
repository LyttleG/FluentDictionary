# FluentDictionary

**Safe, atomic dictionary operations** with null safety and fluent patterns for modern C# development.
This library provides a fluent wrapper around the standard `Dictionary<TKey, TValue>` in C#, offering additional functionality for serialization and fluent creation.

## 📦 Installation

1. Add the `FluentDictionary.cs` and `DictionaryExtensions.cs` files to your project.
2. Add `using FluentDictionary;` and `using FluentDictionary.Extensions;` to your files.

## 🚀 Key Features

| Method                     | Null-Safe | Atomic  | Factory Support | Value Return | Behavior Summary |
|----------------------------|-----------|---------|-----------------|--------------|------------------|
| `TryGetOrAdd`              | ✅        | ✅     | ✅              | Current/New  | Get or initialize|
| `TryUpdate`                | ✅        | ✅     | ✅              | Success      | Mutate existing  |
| `TryAddOrUpdate`           | ✅        | ✅     | ✅              | Add Status   | Upsert operation |
| `TryDelete`                | ✅        | ✅     | ❌              | Success + Val| Remove cleanly   |
| `Subscribe`                | ✅        | ✅     | ❌              | Disposable   | Observable sub   |
| `Json`                     | ✅        | ✅     | ❌              | JSON String  | Serialize dict   |
| `TryGetOrAdd (Batch)`      | ✅        | ✅     | ✅              | Current/New  | Batch insert/get |
| `TryUpdate (Batch)`        | ✅        | ✅     | ✅              | Success      | Batch mutation   |
| `TryAddOrUpdate (Batch)`   | ✅        | ✅     | ✅              | Add Status   | Batch upsert     |
| `TryDelete (Batch)`        | ✅        | ✅     | ❌              | Success + Val| Batch delete     |


## 💻 Usage Examples

### Basic CRUD Operations

```csharp
var features = new Dictionary<string, bool>();

// Add/Retrieve
var darkMode = features.TryGetOrAdd("DarkMode", k => true);

// Update
features.TryUpdate("DarkMode", false);

// Upsert
var isNew = features.TryAddOrUpdate("Experimental", true);

// Delete
if (features.TryDelete("DarkMode", out var removedValue))
    Console.WriteLine($"Removed feature: {removedValue}");
```

### Inventory Management

```csharp
private static int GetRestockCount(string key) => 42;

var stock = new Dictionary<string, int>();

// Restock items
stock.TryAddOrUpdate("Widget", 100);
stock.TryAddOrUpdate("Gadget", key => GetRestockCount(key));

// Sell items
stock.TryUpdate("Widget", current => Math.Max(0, GetRestockCount(current) - 5));

// Check inventory
var widgetCount = stock.TryGetOrAdd("Widget", 0);
var obsoleteCount = stock.TryDelete("OldModel");
```

### Configuration Management

```csharp
private static void SecureWipe(object? key) { }

var config = new Dictionary<string, object>();

// Initialize with defaults
config.TryGetOrAdd("Timeout", 30000);
config.TryGetOrAdd("Retries", key => Environment.GetEnvironmentVariable("RETRIES") ?? "1");

// Environment override
config.TryAddOrUpdate("ApiUrl", key => Environment.GetEnvironmentVariable("API_URL") ?? "https://default.api");

// Secure cleanup
if (config.TryDelete("ApiUrl", out var token))
{
    SecureWipe(token);
}
```

### FluentDictionary Usage

```csharp
var fluentDict = FluentDictionary<string, int>.Create()
    .TryGetOrAdd("key1", 1)
    .TryUpdate("key1", 2)
    .TryAddOrUpdate("key2", 3)
    .TryDelete("key1");

string json = fluentDict.Json();
Console.WriteLine(json);  // Output: {"key2":3}
```

### Observing Dictionary Changes

```csharp
var fluentDictionary = FluentDictionary<string, int?>.Create();

// Subscribe to changes in the dictionary.
// The returned IDisposable is used in a using block to ensure proper cleanup.
using (fluentDictionary.Subscribe(
    onNext: kvp => Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}"),
    onError: ex => Console.WriteLine($"Error: {ex.Message}"),
    onCompleted: () => Console.WriteLine("Observation complete."))
)
{
    // Perform several operations that trigger notifications.
    fluentDictionary.TryGetOrAdd("Apples", 10);
    fluentDictionary.TryAddOrUpdate("Oranges", 5);
    fluentDictionary.TryUpdate("Apples", 15);
    fluentDictionary.TryDelete("Oranges");

    // You can also retrieve a JSON representation of the dictionary.
    Console.WriteLine("JSON representation:");
    Console.WriteLine(fluentDictionary.Json());
} // The observer is unsubscribed automatically here.

// After the using block, the subscription is disposed.
// Further changes will not trigger notifications.
fluentDictionary.TryAddOrUpdate("Bananas", 7);
Console.WriteLine("Finished operations without memory leaks.");
```

### Batch Operations

```csharp
var stock = FluentDictionary<string, int>.Create();
var keys = new[] {"Widget", "Gadget"};
var values = new[] {100, 50};

// Add in batch
stock.TryGetOrAdd(keys, values);

// Update in batch
stock.TryUpdate(keys, new[] {120, 60});

// Delete in batch
stock.TryDelete(keys);
```

### 🛡️ Null-Safe Patterns

```csharp
FluentDictionary<string, int>? nullableDict = GetPotentialNullDictionary();

// Safe invocation
var value = nullableDict?.TryGetOrAdd("safe_key", 42);

// Chained operations
nullableDict?
    .TryUpdate("key1", 100)
    .TryAddOrUpdate("key2", 200)
    .TryDelete("key3");
```

## 📊 Behavior Matrix

| Scenario           | TryGetOrAdd       | TryUpdate         | TryAddOrUpdate    | TryDelete         |
|--------------------|-------------------|-------------------|-------------------|-------------------|
| **Key Exists**     | Returns existing  | Updates → `true`  | Updates → `false` | Removes → `true`  |
| **Key Missing**    | Adds → Returns    | No-op → `false`   | Adds → `true`     | No-op → `false`   |
| **Null Dictionary**| Returns default   | Returns `false`   | Returns `false`   | Returns `false`   |

## ✅ Best Practices

1. **Factory Methods** for expensive value creation:
   ```csharp
   // Only invoked when needed
   cache.TryGetOrAdd("heavy-data", key => GenerateExpensiveResource());
   ```
2. **Atomic Batch Updates**:
   ```csharp
   dict?.TryUpdate("a", 1)
        .TryUpdate("b", 2)
        .TryAddOrUpdate("c", 3);
   ```
3. **Defensive Retrieval**:
   ```csharp
   var value = dangerousDict?.TryGetOrAdd("key", fallback) ?? fallback;
   ```

## ⚙️ Requirements

- .NET Core 6.0+
- C# 8.0+ (Nullable reference types)
- `System.Runtime.CompilerServices.Unsafe` package (if not already referenced)

---

## Dictionary<TKey, TValue> Class Documentation

The `Dictionary<TKey, TValue>` class is a collection of key-value pairs that are organized based on the hash code of the key.
It provides fast lookups, additions, and deletions.

### Key Features

- **Fast Lookups**: Provides O(1) average time complexity for lookups.
- **Key-Value Pairs**: Stores elements as key-value pairs.
- **Generic**: Supports any non-nullable type for keys and any type for values.

### Common Methods

- **Add(TKey, TValue)**: Adds the specified key and value to the dictionary.
- **Remove(TKey)**: Removes the value with the specified key from the dictionary.
- **ContainsKey(TKey)**: Determines whether the dictionary contains the specified key.
- **TryGetValue(TKey, out TValue)**: Gets the value associated with the specified key.
- **Clear()**: Removes all keys and values from the dictionary.

### Example Usage

```csharp
var dictionary = new Dictionary<string, int>();

// Adding elements
dictionary.TryAdd("one", 1);
dictionary.TryAdd("two", 2);

// Accessing elements
if (dictionary.TryGetValue("one", out int value))
{
    Console.WriteLine($"Value for 'one': {value}");
}

// Removing elements
dictionary.TryDelete("two");

// Checking for keys
if (dictionary.ContainsKey("two"))
{
    Console.WriteLine("Key 'two' exists.");
}
else
{
    Console.WriteLine("Key 'two' does not exist.");
}

// Clearing the dictionary
dictionary.Clear();
```

### Best Practices

1. **Avoid Null Keys**: Ensure that keys are non-null to avoid runtime exceptions.
2. **Use TryGetValue for Safe Lookups**: Prefer `TryGetValue` over direct indexing to handle missing keys gracefully.
3. **Consider Capacity**: If the number of elements is known in advance, set the initial capacity to avoid resizing.

---

**📄 Full Documentation**: See inline XML comments in [`DictionaryExtensions.cs`](DictionaryExtensions.cs) and [`FluentDictionary.cs`](FluentDictionary.cs).

**🐛 Issue Tracking**: [Project Issues](https://github.com/LyttleG/FluentDictionary/issues)

**🔄 Changelog**: [CHANGELOG.md](CHANGELOG.md)

Created by [**Gérôme Guillemin**](https://github.com/LyttleG/) on *January 29, 2025*  
Part of the `FluentDictionary` Namespace

## License

MIT