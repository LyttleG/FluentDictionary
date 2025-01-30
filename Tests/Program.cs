using FluentDictionary;
using FluentDictionary.Extensions;
using System.Collections.Generic;

namespace TestsFluentDictionary;

public static class Program
{
    static void Main()
    {
        // ----------------------------------------------------
        // Batch add/update operations
        // ----------------------------------------------------
        var keys = new[] { 1, 2, 3 };
        var values = new[] { "One", "Two", "Three" };

        var std = FluentDictionary<int, string>.Create()
            .TryGetOrAdd(keys, values)
            .TryUpdate([2], ["Due"])
            .TryDelete([3]);

        var json = std.Json; // {"1":"One","2":"Due"}
        var dico = std.Dictionary;

        // ----------------------------------------------------
        // Mixed single and batch operations
        // ----------------------------------------------------
        var mixed = FluentDictionary<int, string>.Create()
            .TryAddOrUpdate(4, "Four")
            .TryAddOrUpdate([5, 6], ["Five", "Six"])
            .TryDelete([4, 5]);

        json = mixed.Json; // => {"6":"Six"}
        dico = mixed.Dictionary;

        // ----------------------------------------------------
        // Create new fluent dictionary
        // ----------------------------------------------------
        var fluentDict = FluentDictionary<int, string>.Create()
            .TryGetOrAdd(1, "One")
            .TryUpdate(1, "Uno")
            .TryAddOrUpdate(2, "Two")
            .TryDelete(2);

        json = fluentDict.Json; // => {"1":"Uno"}
        dico = fluentDict.Dictionary;

        // ----------------------------------------------------
        // Create from existing dictionary
        // ----------------------------------------------------
        var existingDict = new Dictionary<int, string> { { 3, "Three" } };
        var fluentFromExisting = FluentDictionary<int, string>.Create(existingDict)
            .TryUpdate(3, "Tres");

        json = fluentFromExisting.Json; // => {"3":"Tres"}
        dico = fluentFromExisting.Dictionary;

        // ----------------------------------------------------
        // Dictionary extensions
        // ----------------------------------------------------
        var inventory = new Dictionary<string, int>();

        // TryGetOrAdd
        var apples = inventory.TryGetOrAdd("apples", 5);  // Adds 5, returns 5
        var oranges = inventory.TryGetOrAdd("apples", 3); // Returns existing 5

        // TryUpdate
        var updated = inventory.TryUpdate("apples", 10);      // true
        var failedUpdate = inventory.TryUpdate("bananas", 7); // false

        // TryAddOrUpdate
        var wasUpdated = inventory.TryAddOrUpdate("apples", 15); // added: false (but updated)
        var wasAdded = inventory.TryAddOrUpdate("pears", 20);    // added: true (not updated)
        wasAdded = inventory.TryAddOrUpdate("pears", 25);        // added: false (but updated)

        // TryDelete
        var deleted = inventory.TryDelete("apples", out int deletedValue); // true
    }
}
