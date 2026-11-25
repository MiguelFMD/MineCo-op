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

    //Force saving one item in player data. Use example: Saving a single item 
    private async Task<string> ForceSaveObjectData(string key, NetworkItem newItem)
    {
        try
        {
            //Load current inventory
            List<NetworkItem> value = await RetrieveSpecificData<List<NetworkItem>>(key);

            if(value != null)
            {
                //If the loaded inventory already has the new item
                    
                int i = value.FindIndex(item => item.ItemID == newItem.ItemID); //Find the index
                if(i != -1)
                {
                    NetworkItem item = value[i]; //Why we do this: https://discussions.unity.com/t/cs1612-cannot-modify-the-return-value-of-because-it-is-not-variable/871674/2
                    item.Amount += newItem.Amount; 
                    value[i] = item;
                }
                
                
                else value.Add(newItem); //If is not in the inventory we symply add it
            }
            else
            {
                value = new List<NetworkItem>();
                value.Add(newItem);
            }

            
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


    private async Task<string> ForceSaveInventoryData(string key, List<NetworkItem> inventory)
    {
        try
        {
            

            Dictionary<string, object> oneElement = new Dictionary<string, object>
            {
                { key, inventory }
            };

            // Saving data without write lock validation by passing the data as an object instead of a SaveItem
            Dictionary<string, string> result = await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);
            string writeLock = result[key];

            Debug.Log($"Successfully saved {key}:{inventory} with updated write lock {writeLock}");

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
