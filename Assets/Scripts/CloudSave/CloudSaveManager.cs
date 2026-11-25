using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }
    private const string InventoryKey = "inventory";
    private string currentInventoryWriteLock = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    #region READ DATA
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

    public async Task<bool> SaveInventoryAsync(List<NetworkItem> inventory)
    {
        try
        {
            var dataToSave = new Dictionary<string, SaveItem>
            {
                { InventoryKey, new SaveItem(inventory, currentInventoryWriteLock) }
            };

            Dictionary<string, string> result = await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            currentInventoryWriteLock = result[InventoryKey];

            Debug.Log($"Inventory saved ({inventory.Count} stacks). New writeLock: {currentInventoryWriteLock}");
            return true;
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogWarning($"WriteLock conflict or validation saving inventory: {e.Message}");
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogWarning($"Rate limit reached in Cloud Save: {e.Message}");
        }
        catch (CloudSaveException e)
        {
            Debug.LogError($"Cloud save error: {e}");
        }

        return false;
    }
    #endregion

    #region LOAD DATA
    //Loads from player data and deserialize it transforming it into <T>
    public async Task<List<NetworkItem>> LoadInventoryAsync()
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { InventoryKey }
            );

            if (results.TryGetValue(InventoryKey, out Item item))
            {
                currentInventoryWriteLock = item.WriteLock;
                return item.Value.GetAs<List<NetworkItem>>() ?? new List<NetworkItem>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading inventory: {e}");
        }

        return new List<NetworkItem>();
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
