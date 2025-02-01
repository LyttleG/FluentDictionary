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
#nullable enable
public sealed class FluentDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Gets the underlying dictionary instance.
    /// </summary>
    public Dictionary<TKey, TValue> Dictionary { get; }

    /// <summary>
    /// Gets the JSON representation of the dictionary.
    /// </summary>
    public string Json()
        => LazyJson().Value;

    /// <summary>
    /// Gets the JSON representation of the dictionary.
    /// </summary>
    public string Json(JsonSerializerOptions options)
        => LazyJson(options).Value;

    /// <summary>
    /// Serializes the dictionary to a lazy JSON string using optional serializer settings.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to customize the serialization. If not provided, default options are used.</param>
    /// <returns>A JSON string representing the dictionary.</returns>
    private Lazy<string> LazyJson(JsonSerializerOptions options = null!)
        => new(() => JsonSerializer.Serialize(Dictionary, options));

    #region Ctors

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentDictionary{TKey, TValue}"/> class with an empty dictionary.
    /// </summary>
    private FluentDictionary()
        => Dictionary = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentDictionary{TKey, TValue}"/> class with the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> is <c>null</c>.</exception>
    private FluentDictionary(Dictionary<TKey, TValue> dictionary)
        => Dictionary = dictionary
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
    public static FluentDictionary<TKey, TValue> Create(Dictionary<TKey, TValue>? dictionary)
        => dictionary is not null
            ? new(dictionary)
            : new();
    #endregion

    #region Single
    /// <summary>
    /// Attempts to retrieve the value for the specified key; if the key does not exist, adds it with the provided value.
    /// </summary>
    /// <param name="key">The key to retrieve or add.</param>
    /// <param name="valueToAdd">The value to add if the key does not exist.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If the dictionary already contains the key, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryGetOrAdd(TKey key, TValue? valueToAdd)
    {
        Dictionary.TryGetOrAdd(key, valueToAdd);

        return this;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.  
    /// If the key does not exist, a new value is generated using the specified factory function  
    /// and added to the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="key">The key of the value to retrieve or add.</param>
    /// <param name="valueFactoryToAdd">
    /// A function that generates a value to add if the key does not exist.  
    /// If null, no value is added when the key is missing.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryGetOrAdd(TKey key, Func<TKey, TValue?>? valueFactoryToAdd)
    {
        Dictionary.TryGetOrAdd(key, valueFactoryToAdd);

        return this;
    }

    /// <summary>
    /// Attempts to update the value for the specified key if it exists in the dictionary.
    /// </summary>
    /// <param name="key">The key whose value should be updated.</param>
    /// <param name="valueToUpdate">The new value to update.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If the key does not exist, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryUpdate(TKey key, TValue? valueToUpdate)
    {
        Dictionary.TryUpdate(key, valueToUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key.  
    /// If the key exists, the value is updated using the provided factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="valueFactoryToUpdate">
    /// A function that generates the new value if the key exists.  
    /// If null, no update is performed.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryUpdate(TKey key, Func<TKey, TValue>? valueFactoryToUpdate)
    {
        Dictionary.TryUpdate(key, valueFactoryToUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to add or update the value for the specified key.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="valueToAddOrUpdate">The value to associate with the key.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If the key exists, its value is updated; otherwise, a new key-value pair is added.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(TKey key, TValue? valueToAddOrUpdate)
    {
        Dictionary.TryAddOrUpdate(key, valueToAddOrUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to add a new key-value pair to the dictionary or update the existing value if the key already exists.  
    /// The value is generated using the provided factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="key">The key of the value to add or update.</param>
    /// <param name="valueFactoryToAddOrUpdate">
    /// A function that generates a value to add if the key does not exist,  
    /// or updates the value if the key is already present.  
    /// If null, no change is made to the dictionary.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(TKey key, Func<TKey, TValue?>? valueFactoryToAddOrUpdate)
    {
        Dictionary.TryAddOrUpdate(key, valueFactoryToAddOrUpdate);

        return this;
    }

    /// <summary>
    /// Attempts to remove the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>The current <see cref="FluentDictionary{TKey, TValue}"/> instance.</returns>
    /// <remarks>
    /// If the key exists, it is removed; otherwise, no modification occurs.
    /// </remarks>
    public FluentDictionary<TKey, TValue> TryDelete(TKey key)
    {
        Dictionary.TryDelete(key);

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
    public FluentDictionary<TKey, TValue> TryGetOrAdd(IEnumerable<TKey>? keys, IEnumerable<TValue?>? valuesToAdd)
    {
        if (keys is null || valuesToAdd is null)
            return this;

        // Optimize for IList<TKey> to avoid allocation of enumerators
        if (keys is IList<TKey> keyList && valuesToAdd is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                Dictionary.TryGetOrAdd(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAdd.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                Dictionary.TryGetOrAdd(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to retrieve the values associated with the specified keys.  
    /// If a key does not exist, a new value is generated using the specified factory function  
    /// and added to the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="keys">
    /// A collection of keys to retrieve or add.  
    /// If null, no operation is performed.
    /// </param>
    /// <param name="valueFactoryToAdd">
    /// A function that generates a value to add if a key does not exist.  
    /// If null, no values are added.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryGetOrAdd(IEnumerable<TKey>? keys, Func<TKey, TValue?>? valueFactoryToAdd)
    {
        if (keys is null || valueFactoryToAdd is null)
            return this;

        // Optimize for IList<TKey> to avoid allocation of enumerators
        if (keys is IList<TKey> keyList)
        {
            foreach (var key in keyList)
                Dictionary.TryGetOrAdd(key, valueFactoryToAdd(key));
        }
        else
        {
            foreach (var key in keys)
                Dictionary.TryGetOrAdd(key, valueFactoryToAdd(key));
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
    public FluentDictionary<TKey, TValue> TryUpdate(IEnumerable<TKey>? keys, IEnumerable<TValue?>? valuesToUpdate)
    {
        if (keys is null || valuesToUpdate is null)
            return this;

        // Optimize for IList<TKey> to avoid allocation of enumerators
        if (keys is IList<TKey> keyList && valuesToUpdate is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                Dictionary.TryUpdate(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                Dictionary.TryUpdate(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to update the values associated with the specified keys.  
    /// If a key exists in the dictionary, its value is updated using the provided factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="keys">
    /// A collection of keys whose values should be updated.  
    /// If null, no operation is performed.
    /// </param>
    /// <param name="valueFactoryToUpdate">
    /// A function that generates a new value if the key exists.  
    /// If null, no updates are performed.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryUpdate(IEnumerable<TKey>? keys, Func<TKey, TValue?>? valueFactoryToUpdate)
    {
        if (keys is null || valueFactoryToUpdate is null)
            return this;

        if (keys is IList<TKey> keyList)
        {
            foreach (var key in keyList)
                Dictionary.TryUpdate(key, valueFactoryToUpdate(key));
        }
        else
        {
            foreach (var key in keys)
                Dictionary.TryUpdate(key, valueFactoryToUpdate(key));
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
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(IEnumerable<TKey>? keys, IEnumerable<TValue?>? valuesToAddOrUpdate)
    {
        if (keys is null || valuesToAddOrUpdate is null)
            return this;

        if (keys is IList<TKey> keyList && valuesToAddOrUpdate is IList<TValue> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
                Dictionary.TryAddOrUpdate(keyList[i], valueList[i]);
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAddOrUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                Dictionary.TryAddOrUpdate(keyEnumerator.Current, valueEnumerator.Current);
        }

        return this;
    }

    /// <summary>
    /// Attempts to add new key-value pairs to the dictionary or update existing values if the keys already exist.  
    /// The values are generated using the provided factory function.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
    /// <param name="keys">
    /// A collection of keys to add or update.  
    /// If null, no operation is performed.
    /// </param>
    /// <param name="valueFactoryToAddOrUpdate">
    /// A function that generates a value to add if a key does not exist,  
    /// or updates the value if the key is already present.  
    /// If null, no changes are made to the dictionary.
    /// </param>
    /// <returns>The current instance of <see cref="FluentDictionary{TKey, TValue}"/> to allow method chaining.</returns>
    public FluentDictionary<TKey, TValue> TryAddOrUpdate(IEnumerable<TKey>? keys, Func<TKey, TValue?>? valueFactoryToAddOrUpdate)
    {
        if (keys is null || valueFactoryToAddOrUpdate is null)
            return this;

        if (keys is IList<TKey> keyList)
        {
            foreach (var key in keyList)
                Dictionary.TryAddOrUpdate(key, valueFactoryToAddOrUpdate(key));
        }
        else
        {
            foreach (var key in keys)
                Dictionary.TryAddOrUpdate(key, valueFactoryToAddOrUpdate(key));
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
    public FluentDictionary<TKey, TValue> TryDelete(IEnumerable<TKey>? keys)
    {
        if (keys is null)
            return this;

        foreach (var key in keys)
            Dictionary.TryDelete(key);

        return this;
    }
    #endregion
}