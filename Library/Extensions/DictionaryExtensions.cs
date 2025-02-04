using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FluentDictionary.Extensions;
#nullable enable

/// <summary>
/// Provides extension methods for safely adding, updating, or removing entries in a <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
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
    /// Retrieves the value associated with the specified key from the dictionary.
    /// If the key does not exist, adds the key with the specified value and returns it.
    /// </summary>
    /// <param name="dict">The dictionary to retrieve or add the value.</param>
    /// <param name="key">The key whose value to retrieve or add.</param>
    /// <param name="valueToAdd">The value to add if the key does not exist.</param>
    /// <returns>The retrieved or newly added value.</returns>
    public static TValue? TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, TValue? valueToAdd)
        where TKey : notnull
    {
        if (dict is null)
            return default!;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        if (!exists)
            valRef = valueToAdd;

        return valRef;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key or adds a new value using the provided factory function.
    /// </summary>
    /// <param name="dict">The dictionary to retrieve or add the value.</param>
    /// <param name="key">The key whose value to retrieve or add.</param>
    /// <param name="valueFactory">A function to generate a new value if the key does not exist.</param>
    /// <returns>The retrieved or newly added value.</returns>
    public static TValue? TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, Func<TKey, TValue?>? valueFactory)
        where TKey : notnull
    {
        if (dict is null || valueFactory is null)
            return default!;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        if (!exists)
            valRef = valueFactory(key);

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
    public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, TValue? valueToUpdate)
        where TKey : notnull
    {
        if (dict is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);

        if (Unsafe.IsNullRef(ref valRef))
            return false;

        valRef = valueToUpdate!;

        return true;
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key using a factory function.<br/>
    /// If the key exists, invokes the factory to generate a new value and updates the entry, returning <c>true</c>. Otherwise, returns <c>false</c>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary in which to update the value.</param>
    /// <param name="key">The key whose value to update.</param>
    /// <param name="updateFactory">A function that generates a new value based on the key. Only invoked if the key exists.</param>
    /// <returns>
    /// <c>true</c> if the key existed and the value was updated; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryUpdate<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, Func<TKey, TValue>? updateFactory)
        where TKey : notnull
    {
        if (dict is null || updateFactory is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);

        if (Unsafe.IsNullRef(ref valRef))
            return false;

        valRef = updateFactory(key);

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
    public static bool TryAddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, TValue? valueToAddOrUpdate)
        where TKey : notnull
    {
        if (dict is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        valRef = valueToAddOrUpdate;

        return !exists; // "true" if added, "false" if updated
    }

    /// <summary>
    /// Attempts to add or update a key-value pair in the dictionary using a value factory function.<br/>
    /// If the key exists, updates its value using the factory's output. If not, adds the key with the factory-generated value.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dict">The dictionary to modify.</param>
    /// <param name="key">The key to add or update.</param>
    /// <param name="valueFactory">A function that generates a value based on the key. Invoked for both add and update scenarios.</param>
    /// <returns>
    /// <c>true</c> if the key was newly added; <c>false</c> if the key already existed and was updated.
    /// </returns>
    public static bool TryAddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key, Func<TKey, TValue?>? valueFactory)
        where TKey : notnull
    {
        if (dict is null || valueFactory is null)
            return false;

        ref var valRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);

        var newValue = valueFactory(key);
        valRef = newValue;

        return !exists; // "true" if added, "false" if updated
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
        switch (dict)
        {
            case null:
                value = default;
                return false;

            default:
                return dict.Remove(key, out value);
        }
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
    public static bool TryDelete<TKey, TValue>(this Dictionary<TKey, TValue>? dict, TKey key)
        where TKey : notnull
        => dict is not null && dict.TryDelete(key, out _);
}