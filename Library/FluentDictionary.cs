using FluentDictionary.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FluentDictionary;

/// <summary>
/// A fluent wrapper around <see cref="Dictionary{TKey, TValue}"/>, providing additional functionality for serialization and fluent creation.
/// </summary>
/// <typeparam name="TKey">The type of dictionary keys. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of dictionary values.</typeparam>
/// <remarks>
/// Created by <c>Gérôme Guillemin</c> on <c>January 29, 2025</c>.<br/>
/// </remarks>
public sealed class FluentDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Gets the underlying dictionary instance.
    /// </summary>
    public Dictionary<TKey, TValue> Dictionary
        => _dictionary;

    /// <summary>
    /// Gets the JSON representation of the dictionary.
    /// </summary>
    public string Json
        => LazyJson().Value;

    /// <summary>
    /// Serializes the dictionary to a lazy JSON string using optional serializer settings.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to customize the serialization. If not provided, default options are used.</param>
    /// <returns>A JSON string representing the dictionary.</returns>
    public Lazy<string> LazyJson(JsonSerializerOptions options = default!)
        => new(() => JsonSerializer.Serialize(_dictionary, options));

    #region Ctors
    private readonly Dictionary<TKey, TValue> _dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentDictionary{TKey, TValue}"/> class with an empty dictionary.
    /// </summary>
    private FluentDictionary()
        => _dictionary = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentDictionary{TKey, TValue}"/> class with the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> is <c>null</c>.</exception>
    private FluentDictionary(Dictionary<TKey, TValue> dictionary)
        => _dictionary = dictionary
            ?? throw new ArgumentNullException(nameof(dictionary));

    /// <summary>
    /// Creates a new instance of <see cref="FluentDictionary{TKey, TValue}"/> with an empty dictionary.
    /// </summary>
    /// <returns>A new <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    public static FluentDictionary<TKey, TValue> Create()
        => new();

    /// <summary>
    /// Creates a new instance of <see cref="FluentDictionary{TKey, TValue}"/> with the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    /// <returns>A new <see cref="FluentDictionary{TKey, TValue}"/> instance containing the specified dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> is <c>null</c>.</exception>
    public static FluentDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> dictionary)
        => new(dictionary);
    #endregion

    #region Single
    /// <summary>
    /// Attempts to retrieve the value for the specified key; if the key does not exist, adds it with the provided value.
    /// </summary>
    /// <param name="key">The key to retrieve or add.</param>
    /// <param name="valueToAdd">The value to add if the key does not exist.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// Uses <see cref="DictionaryExtensions.TryGetOrAdd"/> to perform the operation.<br/>
    /// If the dictionary already contains the key, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryGetOrAdd(TKey key, TValue valueToAdd)
    {
        DictionaryExtensions.TryGetOrAdd(_dictionary, key, valueToAdd);

        return this;
    }

    /// <summary>
    /// Attempts to update the value for the specified key if it exists in the dictionary.
    /// </summary>
    /// <param name="key">The key whose value should be updated.</param>
    /// <param name="valueToUpdate">The new value to update.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// Uses <see cref="DictionaryExtensions.TryUpdate"/> to perform the operation.<br/>
    /// If the key does not exist, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryUpdate(TKey key, TValue valueToUpdate)
    {
        DictionaryExtensions.TryUpdate(_dictionary, key, valueToUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to add or update the value for the specified key.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="valueToAddOrUpdate">The value to associate with the key.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// Uses <see cref="DictionaryExtensions.TryAddOrUpdate"/> to perform the operation.<br/>
    /// If the key exists, its value is updated; otherwise, a new key-value pair is added.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(TKey key, TValue valueToAddOrUpdate)
    {
        DictionaryExtensions.TryAddOrUpdate(_dictionary, key, valueToAddOrUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to remove the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// Uses <see cref="DictionaryExtensions.TryDelete"/> to perform the operation.<br/>
    /// If the key exists, it is removed; otherwise, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryDelete(TKey key)
    {
        DictionaryExtensions.TryDelete(_dictionary, key);

        return this;
    }
    #endregion

    #region IEnumerable
    /// <summary>
    /// Attempts to retrieve existing values for the specified keys; if a key does not exist, it adds the corresponding value from the provided collection.
    /// </summary>
    /// <param name="keys">The keys to retrieve or add.</param>
    /// <param name="valuesToAdd">The values to add if the keys do not exist.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If either <paramref name="keys"/> or <paramref name="valuesToAdd"/> is <c>null</c>, no operation is performed.<br/>
    /// Iterates through both collections in parallel, stopping when the shorter collection is exhausted.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryGetOrAdd(IEnumerable<TKey> keys, IEnumerable<TValue> valuesToAdd)
    {
        if (keys == null || valuesToAdd is null)
            return this;

        // Try to cast to IList for better performance
        if (keys is IList<TKey> keyList && valuesToAdd is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                TryGetOrAdd(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAdd.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                TryGetOrAdd(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to update the values for the specified keys if they exist in the dictionary.
    /// </summary>
    /// <param name="keys">The keys whose values should be updated.</param>
    /// <param name="valuesToUpdate">The new values to update.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If either <paramref name="keys"/> or <paramref name="valuesToUpdate"/> is <c>null</c>, no operation is performed.<br/>
    /// Iterates through both collections in parallel, stopping when the shorter collection is exhausted.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryUpdate(IEnumerable<TKey> keys, IEnumerable<TValue> valuesToUpdate)
    {
        if (keys is null || valuesToUpdate is null)
            return this;

        // Try to cast to IList for better performance
        if (keys is IList<TKey> keyList && valuesToUpdate is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                TryUpdate(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                TryUpdate(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to add or update key-value pairs in the dictionary.
    /// </summary>
    /// <param name="keys">The keys to add or update.</param>
    /// <param name="valuesToAddOrUpdate">The values to associate with the keys.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If either <paramref name="keys"/> or <paramref name="valuesToAddOrUpdate"/> is <c>null</c>, no operation is performed.<br/>
    /// Iterates through both collections in parallel, stopping when the shorter collection is exhausted.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(IEnumerable<TKey> keys, IEnumerable<TValue> valuesToAddOrUpdate)
    {
        if (keys is null || valuesToAddOrUpdate is null)
            return this;

        // Try to cast to IList for better performance
        if (keys is IList<TKey> keyList && valuesToAddOrUpdate is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                TryAddOrUpdate(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAddOrUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                TryAddOrUpdate(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to remove the specified keys from the dictionary.
    /// </summary>
    /// <param name="keys">The keys to remove.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If <paramref name="keys"/> is <c>null</c>, no operation is performed.
    /// Iterates through the collection and removes each key individually.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryDelete(IEnumerable<TKey> keys)
    {
        if (keys is null)
            return this;

        foreach (var key in keys)
            TryDelete(key);

        return this;
    }
    #endregion
}