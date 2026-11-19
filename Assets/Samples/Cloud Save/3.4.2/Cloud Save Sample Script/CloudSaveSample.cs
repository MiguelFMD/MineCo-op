using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

[Serializable]
public class SampleObject
{
    public string SophisticatedString;
    public int SparklingInt;
    public float AmazingFloat;
}

public class CloudSaveSample : MonoBehaviour
{
    private async void Awake()
    {

        // first saves without write lock
        await ForceSaveSingleData("primitive_key", "value!");

        SampleObject firstSample = new SampleObject
        {
            AmazingFloat = 13.37f,
            SparklingInt = 1337,
            SophisticatedString = "hi there!"
        };
        string writeLock = await ForceSaveObjectData("object_key", firstSample);
        SampleObject incomingSample = await RetrieveSpecificData<SampleObject>("object_key");
        Debug.Log($"Loaded sample object: {incomingSample.AmazingFloat}, {incomingSample.SparklingInt}, {incomingSample.SophisticatedString}, write lock {writeLock}");

        // second save with write lock
        SampleObject secondSample = new SampleObject
        {
            AmazingFloat = 42.26f,
            SparklingInt = 4226,
            SophisticatedString = "hi there... again!"
        };
        string updatedWriteLock = await SaveObjectData("object_key", secondSample, writeLock);
        SampleObject updatedSample = await RetrieveSpecificData<SampleObject>("object_key");
        Debug.Log($"Loaded updated sample object: {updatedSample.AmazingFloat}, {updatedSample.SparklingInt}, {updatedSample.SophisticatedString}, write lock {updatedWriteLock}");

        await ListAllKeys();
        await RetrieveEverything();

        // Custom Data, read-only

        await ListAllCustomKeys("custom-test-id");
        await RetrieveAllCustomData("custom-test-id");
    }
    #region LIST
    //List all player data keys
    private async Task ListAllKeys()
    {
        try
        {
            var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();

            Debug.Log($"Keys count: {keys.Count}\n" +
                $"Keys: {String.Join(", ", keys)}");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    //List all game data keys
    private async Task ListAllCustomKeys(string customId)
    {
        try
        {
            var keys = await CloudSaveService.Instance.Data.Custom.ListAllKeysAsync(customId);

            Debug.Log($"Keys count for custom ID {customId}: {keys.Count}\n" +
                $"Keys: {String.Join(", ", keys)}");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region SAVE DATA
    //Force saving simple player data. Use example. Save player name: ("PlayerName", "Hero123"),
    private async Task ForceSaveSingleData(string key, string value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();

            // It's a text input field, but let's see if you actually entered a number.
            if (Int32.TryParse(value, out int wholeNumber))
            {
                oneElement.Add(key, wholeNumber);
            }
            else if (Single.TryParse(value, out float fractionalNumber))
            {
                oneElement.Add(key, fractionalNumber);
            }
            else
            {
                oneElement.Add(key, value);
            }

            // Saving the data without write lock validation by passing the data as an object instead of a SaveItem
            Dictionary<string, string> result = await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);

            Debug.Log($"Successfully saved {key}:{value} with updated write lock {result[key]}");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    //Force saving objects in player data. Use example: Saving players inventory 
    private async Task<string> ForceSaveObjectData(string key, SampleObject value)
    {
        try
        {
            // Although we are only saving a single value here, you can save multiple keys
            // and values in a single batch.
            Dictionary<string, object> oneElement = new Dictionary<string, object>
            {
                { key, value }
            };

            // Saving data without write lock validation by passing the data as an object instead of a SaveItem
            Dictionary<string, string> result = await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);
            string writeLock = result[key];

            Debug.Log($"Successfully saved {key}:{value} with updated write lock {writeLock}");

            return writeLock;
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }

        return null;
    }

    //Save/Updates player object data by using a writelock for security purposes
    private async Task<string> SaveObjectData(string key, SampleObject value, string writeLock)
    {
        try
        {
            // Although we are only saving a single value here, you can save multiple keys
            // and values in a single batch.
            // Use SaveItem to specify a write lock. The request will fail if the provided write lock
            // does not match the one currently saved on the server.
            Dictionary<string, SaveItem> oneElement = new Dictionary<string, SaveItem>
            {
                { key, new SaveItem(value, writeLock) }
            };

            // Saving data with write lock validation by using a SaveItem with the write lock specified
            Dictionary<string, string> result = await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);
            string newWriteLock = result[key];

            Debug.Log($"Successfully saved {key}:{value} with updated write lock {newWriteLock}");

            return newWriteLock;
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }

        return null;
    }

    #endregion

    #region LOAD DATA
    //Loads from player data and deserialize it transforming it into <T>
    private async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> {key});

            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAs<T>();
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }

        return default;
    }

    //Loads all keys and values from player data
    private async Task RetrieveEverything()
    {
        try
        {
            // If you wish to load only a subset of keys rather than everything, you
            // can call a method LoadAsync and pass a HashSet of keys into it.
            var results = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            Debug.Log($"{results.Count} elements loaded!");

            foreach (var result in results)
            {
                Debug.Log($"Key: {result.Key}, Value: {result.Value.Value}");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    //Loads all keys and values from game data
    private async Task RetrieveAllCustomData(string customId)
    {
        try
        {
            // If you wish to load only a subset of keys rather than everything, you
            // can call a method LoadAsync and pass a HashSet of keys into it.
            var results = await CloudSaveService.Instance.Data.Custom.LoadAllAsync(customId);

            Debug.Log($"{results.Count} elements loaded from custom Id {customId}!");

            foreach (var result in results)
            {
                Debug.Log($"Key: {result.Key}, Value: {result.Value.Value}");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }
    #endregion
}
