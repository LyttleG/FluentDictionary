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
public sealed class FluentDictionary<TKey, TValue> : IObservable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    #region Private

    /// <summary>
    /// A list of observers that have subscribed to receive notifications for changes in the dictionary.
    /// </summary>
    private readonly List<IObserver<KeyValuePair<TKey, TValue?>>> _observers = new(1);

    /// <summary>
    /// Serializes the dictionary to a lazy JSON string using optional serializer settings.
    /// </summary>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to customize the serialization. If not provided, default options are used.</param>
    /// <returns>A JSON string representing the dictionary.</returns>
    private Lazy<string> LazyJson(JsonSerializerOptions options = null!)
        => new(() => JsonSerializer.Serialize(Dictionary, options));

    /// <summary>
    /// Notifies all subscribed observers of a change in the dictionary.
    /// </summary>
    /// <param name="change">
    /// A <see cref="KeyValuePair{TKey, TValue}"/> representing the key and the new value that was added, updated, or deleted.
    /// </param>
    private void NotifyObservers(KeyValuePair<TKey, TValue?> change)
    {
        foreach (var observer in _observers)
            observer.OnNext(change);
    }

    #endregion

    #region Constructors

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
        => Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

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

    #region Data

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

    #endregion

    #region Subscription to observable events

    /// <summary>
    /// Subscribes an observer to receive notifications.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes the observer when disposed.</returns>
    public IDisposable Subscribe(IObserver<KeyValuePair<TKey, TValue>>? observer)
    {
        if (observer is not null && !_observers.Contains(observer!))
            _observers.Add(observer!);

        return new InternalDelegateDisposable(() =>
        {
            if (observer is not null)
                _observers.Remove(observer!);
        });
    }

    /// <summary>
    /// Subscribes to dictionary changes using the provided callback(s) without requiring a custom observer.
    /// </summary>
    /// <param name="onNext">Action to invoke when a change occurs.</param>
    /// <param name="onError">Optional action to invoke if an error occurs.</param>
    /// <param name="onCompleted">Optional action to invoke when the subscription is completed.</param>
    /// <returns>An IDisposable that unsubscribes when disposed.</returns>
    public IDisposable Subscribe(Action<KeyValuePair<TKey, TValue>> onNext, Action<Exception>? onError = null, Action? onCompleted = null)
    {
        if (onNext is null)
            throw new ArgumentNullException(nameof(onNext));

        return Subscribe(new InternalObserver(onNext, onError, onCompleted));
    }

    #endregion

    #region Internal Delegate-Based Disposable Implementation

    /// <summary>
    /// Represents a disposable that executes a delegate when disposed.
    /// </summary>
    /// <remarks>
    /// This class is useful for scenarios where you want to execute custom cleanup logic when an object is no longer needed.<br/>
    /// The provided delegate is executed only once, and subsequent calls to <see cref="Dispose"/> have no effect.
    /// </remarks>
    private sealed class InternalDelegateDisposable : IDisposable
    {
        private Action? _disposeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalDelegateDisposable"/> class with the specified dispose action.
        /// </summary>
        /// <param name="disposeAction">The action to execute when the instance is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="disposeAction"/> is <c>null</c>.</exception>
        public InternalDelegateDisposable(Action disposeAction)
            => _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.<br/>
        /// Invokes the provided delegate and ensures it is executed only once.
        /// </summary>
        public void Dispose()
        {
            _disposeAction?.Invoke();
            _disposeAction = null;
        }
    }

    #endregion

    #region Internal Observer Implementation

    /// <summary>
    /// Represents an internal observer that wraps lambda expressions for handling dictionary change notifications.
    /// </summary>
    /// <remarks>
    /// This observer implements the <see cref="IObserver{T}"/> interface to handle notifications for additions, updates,
    /// or deletions in the dictionary. Custom actions can be provided for processing new values, errors, and completion signals.
    /// </remarks>
    private sealed class InternalObserver : IObserver<KeyValuePair<TKey, TValue>>
    {
        private readonly Action<KeyValuePair<TKey, TValue>> _onNext;
        private readonly Action<Exception>? _onError;
        private readonly Action? _onCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalObserver"/> class with the specified callback actions.
        /// </summary>
        /// <param name="onNext">The action to invoke when a new dictionary value is observed.</param>
        /// <param name="onError">The action to invoke when an error occurs during observation.</param>
        /// <param name="onCompleted">The action to invoke when the observation is completed.</param>
        public InternalObserver(Action<KeyValuePair<TKey, TValue>> onNext, Action<Exception>? onError, Action? onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        /// <summary>
        /// Provides the observer with a new value.
        /// </summary>
        /// <param name="value">The current notification containing the key-value pair change.</param>
        public void OnNext(KeyValuePair<TKey, TValue> value)
            => _onNext(value);

        /// <summary>
        /// Notifies the observer that an error has occurred.
        /// </summary>
        /// <param name="error">An exception that contains information about the error.</param>
        public void OnError(Exception error)
            => _onError?.Invoke(error);

        /// <summary>
        /// Notifies the observer that the provider has finished sending notifications.
        /// </summary>
        public void OnCompleted()
            => _onCompleted?.Invoke();
    }

    #endregion

    #region Single values

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
        NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueToAdd));

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
        NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueFactoryToAdd!(key)));

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
        if (Dictionary.TryUpdate(key, valueToUpdate))
            NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueToUpdate));

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
        if (Dictionary.TryUpdate(key, valueFactoryToUpdate))
            NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueFactoryToUpdate!(key)));

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
        NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueToAddOrUpdate));

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
        NotifyObservers(new KeyValuePair<TKey, TValue?>(key, valueFactoryToAddOrUpdate!(key)));

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
        if (Dictionary.TryDelete(key))
            NotifyObservers(new KeyValuePair<TKey, TValue?>(key, default));

        return this;
    }

    #endregion

    #region IEnumerable values

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
            {
                Dictionary.TryGetOrAdd(keyList[i], valueList[i]);
                NotifyObservers(new KeyValuePair<TKey, TValue?>(keyList[i], valueList[i]));
            }
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAdd.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                Dictionary.TryGetOrAdd(keyEnumerator.Current, valueEnumerator.Current);
                NotifyObservers(new KeyValuePair<TKey, TValue?>(keyEnumerator.Current, valueEnumerator.Current));
            }
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
            {
                var value = valueFactoryToAdd(key);

                Dictionary.TryGetOrAdd(key, value);
                NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToAdd(key);

                Dictionary.TryGetOrAdd(key, value);
                NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
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
                if (Dictionary.TryUpdate(keyList[i], valueList[i]))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(keyList[i], valueList[i]));
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                if (Dictionary.TryUpdate(keyEnumerator.Current, valueEnumerator.Current))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(keyEnumerator.Current, valueEnumerator.Current));
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
            {
                var value = valueFactoryToUpdate(key);
                if (Dictionary.TryUpdate(key, value))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToUpdate(key);
                if (Dictionary.TryUpdate(key, value))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
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
                if (Dictionary.TryAddOrUpdate(keyList[i], valueList[i]))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(keyList[i], valueList[i]));
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAddOrUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
                if (Dictionary.TryAddOrUpdate(keyEnumerator.Current, valueEnumerator.Current))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(keyEnumerator.Current, valueEnumerator.Current));
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
            {
                var value = valueFactoryToAddOrUpdate(key);
                if (Dictionary.TryAddOrUpdate(key, value))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToAddOrUpdate(key);

                if (Dictionary.TryAddOrUpdate(key, value))
                    NotifyObservers(new KeyValuePair<TKey, TValue?>(key, value));
            }
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
            if (Dictionary.TryDelete(key))
                NotifyObservers(new KeyValuePair<TKey, TValue?>(key, default));

        return this;
    }

    #endregion
}