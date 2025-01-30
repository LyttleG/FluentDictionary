# Dictionary Extensions

A set of extension methods for safe and efficient manipulation of `Dictionary<TKey, TValue>` in C#.

## Features

- **Null-safe operations**: Gracefully handles null dictionary references
- **Atomic operations**: Combined get/add and add/update functionality
- **Efficient**: Uses `CollectionsMarshal` for performance optimizations
- **Clear behavior**: Consistent return patterns for all operations

## Behavior Comparison

| Function               | TryGetOrAdd            | TryUpdate   | TryAddOrUpdate   | TryDelete (*)    |
|------------------------|------------------------|-------------|------------------|------------------|
| **Key exists**         | `Get → existing value` | `true`      | `Update → false` | `true (* value)` |
| **Key does not exist** | `Add → value`          | `false`     | `Add → true`     | `false`          |
| **Null dictionary**    | `default(TValue)`      | `false`     | `false`          | `false`          |

## Methods

### `TryGetOrAdd`

```csharp
public static TValue? TryGetOrAdd<TKey, TValue>(
    this Dictionary<TKey, TValue> dict, 
    TKey key, 
    TValue? valueToAdd)
```

**Description**  
Retrieves an existing value or adds a new entry if the key doesn't exist.

**Parameters**:
- `dict`: Target dictionary
- `key`: Key to lookup
- `valueToAdd`: Value to add if key doesn't exist

**Returns**:  
Existing value if found, otherwise `valueToAdd` TValue

---

### `TryUpdate`

```csharp
public static bool TryUpdate<TKey, TValue>(
    this Dictionary<TKey, TValue> dict,
    TKey key,
    TValue valueToUpdate)
```

**Description**  
Updates an existing value if the key exists.

**Parameters**:
- `dict`: Target dictionary
- `key`: Key to update
- `valueToUpdate`: New value to set

**Returns**:  
`true` if updated successfully, `false` otherwise

---

### `TryAddOrUpdate`

```csharp
public static bool TryAddOrUpdate<TKey, TValue>(
    this Dictionary<TKey, TValue> dict,
    TKey key,
    TValue valueToAddOrUpdate)
```

**Description**  
Adds or updates a value in one atomic operation.

**Parameters**:
- `dict`: Target dictionary
- `key`: Key to modify
- `valueToAddOrUpdate`: Value to set

**Returns**:  
`true` if added new key, `false` if updated existing key

---

### `TryDelete`

```csharp
public static bool TryDelete<TKey, TValue>(
    this Dictionary<TKey, TValue> dict,
    TKey key,
    [MaybeNullWhen(false)] out TValue value)

public static bool TryDelete<TKey, TValue>(
    this Dictionary<TKey, TValue> dict, 
    TKey key)
```

**Description**  
Removes an entry and optionally returns its value.

**Parameters**:
- `dict`: Target dictionary
- `key`: Key to remove
- `value`: [out] Removed value if found

**Returns**:  
`true` if key was found and removed

---

## Example Usage

```csharp
var inventory = new Dictionary<string, int>();

// TryGetOrAdd
int apples = inventory.TryGetOrAdd("apples", 5);  // Adds 5, returns 5
int oranges = inventory.TryGetOrAdd("apples", 3); // Returns existing 5

// TryUpdate
bool updated = inventory.TryUpdate("apples", 10); // true
bool failedUpdate = inventory.TryUpdate("bananas", 7); // false

// TryAddOrUpdate
bool wasUpdated = inventory.TryAddOrUpdate("apples", 15); // false
bool wasAdded = inventory.TryAddOrUpdate("pears", 20);    // true
wasAdded = inventory.TryAddOrUpdate("pears", 25);         // false

// TryDelete
bool removed = inventory.TryDelete("apples", out int removedCount); // true
```

## Requirements

- .NET 6.0+ (uses `CollectionsMarshal` methods)
- C# 7.3+ (nullable reference types)

---

Created by **Gérôme Guillemin** on *January 29, 2025*  
Part of the `FluentDictionary` Namespace

## License
MIT
