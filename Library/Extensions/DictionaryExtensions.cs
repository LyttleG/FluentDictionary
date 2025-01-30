using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentDictionary.Extensions;

/// <summary>
/// Provides extension methods for safely adding, updating or removing entries in a <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The type of dictionary keys. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of dictionary values.</typeparam>
/// <remarks>
/// Created by <c>Gérôme Guillemin</c> on <c>January 29, 2025</c>.<br/>
/// </remarks>
/// <example>
/// Behavior Comparison:
/// <code>
/// Function           | TryUpdate       | TryGetOrAdd           | TryAddOrUpdate  | TryDelete
/// -------------------|-----------------|-----------------------|-----------------|------------------------------
/// Key exists         | Updates -> true | Returns existing      | Updates -> true | Returns true + correct value
/// Key doesn't exist  | Returns false   | Adds -> returns value | Adds -> false   | Returns false + default value
/// Null dictionary    | Returns false   | Returns default       | Returns false   | Returns false
/// </code>
/// </example>
public static class DictionaryExtensions
{
    /// <summary>
    /// Retrieves the value associated with the specified key from the dictionary.<br/>
    /// If the key does not exist, adds the key with the specified value and returns it.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary. Can be nullable.</typeparam>
    /// <param name="dict">The dictionary from which to retrieve or add the value.</param>
    /// <param name="key">The key whose value to retrieve or add.</param>
    /// <param name="valueToAdd">The value to add if the key does not already exist.</param>
    /// <returns>
    /// The value associated with the specified key. If the key does not exist, the value specified is added
    /// to the dictionary and then returned.
    /// </returns>
    public static TValue TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue valueToAdd)
        where TKey : notnull
    {
        if (dict is null)
            return default;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        if (!exists)
            valRef = valueToAdd;

        return valRef;
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.<br/>
    /// If the key exists, updates the value and returns <c>true</c>. Otherwise, returns <c>false</c>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary in which to update the value.</param>
    /// <param name="key">The key whose value to update.</param>
    /// <param name="valueToUpdate">The new value to associate with the specified key.</param>
    /// <returns>
    /// <c>true</c> if the key exists and the value was updated; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue valueToUpdate)
        where TKey : notnull
    {
        if (dict is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);

        if (Unsafe.IsNullRef(ref valRef))
            return false;

        valRef = valueToUpdate;

        return true;
    }

    /// <summary>
    /// Attempts to add a new key-value pair to the dictionary or update the existing value if the key already exists.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dict">The dictionary to modify.</param>
    /// <param name="key">The key to add or update.</param>
    /// <param name="valueToAddOrUpdate">The value to associate with the key.</param>
    /// <returns>
    /// Returns <c>true</c> if the key was newly added.
    /// otherwise, returns <c>false</c> if the key already existed and was updated;
    /// </returns>
    public static bool TryAddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue valueToAddOrUpdate)
        where TKey : notnull
    {
        if (dict is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool exists);

        valRef = valueToAddOrUpdate;

        return !exists;
    }

    /// <summary>
    /// Attempts to remove a key-value pair from the dictionary and retrieves the associated value if successful.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
    /// <param name="dict">The dictionary to modify.</param>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the key if found;
    /// otherwise, the default value of <typeparamref name="TValue"/>. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key was found and removed; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryDelete<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, [MaybeNullWhen(false)] out TValue value)
        where TKey : notnull
    {
        if (dict is null)
        {
            value = default;

            return false;
        }

        return dict.Remove(key, out value);
    }

    /// <summary>
    /// Attempts to remove a key-value pair from the dictionary without retrieving the value.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
    /// <param name="dict">The dictionary to modify.</param>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// <c>true</c> if the key was found and removed; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryDelete<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        where TKey : notnull => dict.TryDelete(key, out var _);
}