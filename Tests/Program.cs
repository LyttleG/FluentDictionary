﻿using FluentDictionary;
using FluentDictionary.Extensions;
using System;
using System.Collections.Generic;
// ReSharper disable NotAccessedVariable
// ReSharper disable RedundantAssignment

namespace TestsFluentDictionary;

public static class Program
{
    private static void Main()
    {
        // ----------------------------------------------------
        // Using dictionary change subscription operations
        // ----------------------------------------------------
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

        // ----------------------------------------------------
        // Batch add/update operations
        // ----------------------------------------------------
        var keys = new[] { 1, 2, 3 };
        var values = new[] { "One", "Two", "Three" };

        var std = FluentDictionary<int, string>.Create()
            .TryGetOrAdd(keys, values)
            .TryUpdate([2], ["Due"])
            .TryDelete([3]);

        var json = std.Json(); // {"1":"One","2":"Due"}
        var dico = std.Dictionary;

        // ----------------------------------------------------
        // Mixed single and batch operations
        // ----------------------------------------------------
        var mixed = FluentDictionary<int, string>.Create()
            .TryAddOrUpdate(4, "Four")
            .TryAddOrUpdate([5, 6], ["Five", "Six"])
            .TryDelete([4, 5]);

        json = mixed.Json(); // => {"6":"Six"}
        // ReSharper disable once RedundantAssignment
        dico = mixed.Dictionary;

        // ----------------------------------------------------
        // Null-Safe Patterns
        // ----------------------------------------------------
        FluentDictionary<string, int>? nullableDict = null;

        // Safe invocation
        var value = nullableDict?.TryGetOrAdd("safe_key", 42);

        // Chained operations
        nullableDict?
            //.TryUpdate("key1", 100)
            .TryAddOrUpdate("key2", 200)
            .TryDelete("key3");

        // ----------------------------------------------------
        // Create new fluent dictionary
        // ----------------------------------------------------
        var fluentDict = FluentDictionary<int, string>.Create()
            .TryGetOrAdd(1, "One")
            .TryUpdate(1, "Uno")
            .TryAddOrUpdate(2, "Two")
            .TryDelete(2);

        json = fluentDict.Json(); // => {"1":"Uno"}
        dico = fluentDict.Dictionary;

        // ----------------------------------------------------
        // Create from existing dictionary
        // ----------------------------------------------------
        var existingDict = new Dictionary<int, string> { { 3, "Three" } };
        var fluentFromExisting = FluentDictionary<int, string>.Create(existingDict)
            .TryUpdate(3, "Tres");

        json = fluentFromExisting.Json(); // => {"3":"Tres"}
        dico = fluentFromExisting.Dictionary;

        // ----------------------------------------------------
        // Dictionary extensions
        // ----------------------------------------------------
        var inventory = new Dictionary<string, int>();

        // TryGetOrAdd
        var apples = inventory.TryGetOrAdd("apples", 5);  // Adds 5, returns 5
        var oranges = inventory.TryGetOrAdd("apples", 3); // Returns existing 5

        // TryUpdate
        var updated = inventory.TryUpdate("apples", k => inventory[k] + 5); // true
        var failedUpdate = inventory.TryUpdate("bananas", 7);               // false (bananas don't exist)

        // TryAddOrUpdate
        var wasUpdated = inventory.TryAddOrUpdate("apples", k => inventory[k] + 5); // added: false (but updated)
        var wasAdded = inventory.TryAddOrUpdate("pears", k => inventory[k] = 20);   // added: true (not updated)
        wasAdded = inventory.TryAddOrUpdate("pears", 25);                                // added: false (but updated)

        // TryDelete
        var deleted = inventory.TryDelete("apples", out var deletedValue); // true
    }
}