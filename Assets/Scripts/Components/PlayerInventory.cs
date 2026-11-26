using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour
{
    public NetworkList<NetworkItem> Inventory;
    [SerializeField, Tooltip("Add all the items that the player can pick up.")] 
    private ItemData[] itemDatabase;
    
    private bool isDirty = false;
    private bool isSaving = false;
    private readonly float saveInterval = 30.0f;


    private void Awake()
    {
        Inventory = new NetworkList<NetworkItem>();
    }

    public override async void OnNetworkSpawn()
    {
        // Only owner manages its own Cloud Save
        if (!IsOwner) return;

        //----------NEW-----------------
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.RecordMatchJoined(IsHost);
        }
        //--------------------------------

        // 1. Load saved inventory
        List<NetworkItem> savedItems = await CloudSaveManager.Instance.LoadInventoryAsync();
        if (savedItems.Count > 0)
        {
            InitializeInventoryServerRpc(savedItems.ToArray());
        }

        // 2. Listen applied changes
        Inventory.OnListChanged += HandleInventoryChanged;

        // 3. Auto save 
        StartCoroutine(SaveInventoryCoroutine());
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Inventory.OnListChanged -= HandleInventoryChanged;
        }
    }

    private void HandleInventoryChanged(NetworkListEvent<NetworkItem> changeEvent)
    {
        isDirty = true;
        if (AnalyticsManager.Instance == null) return;

        if (changeEvent.Type == NetworkListEvent<NetworkItem>.EventType.Add)
        {
            ItemData data = GetItemDataByID(changeEvent.Value.ItemID);
            string itemName = data != null ? data.ItemName : $"Item_{changeEvent.Value.ItemID}";
            AnalyticsManager.Instance.RecordItemCollected(
                changeEvent.Value.ItemID,
                itemName,
                changeEvent.Value.Amount,
                transform.position
            );
        }
        else if (changeEvent.Type == NetworkListEvent<NetworkItem>.EventType.Value)
        {
            int delta = changeEvent.Value.Amount - changeEvent.PreviousValue.Amount;
            if (delta > 0)
            {
                ItemData data = GetItemDataByID(changeEvent.Value.ItemID);
                string itemName = data != null ? data.ItemName : $"Item_{changeEvent.Value.ItemID}";
                AnalyticsManager.Instance.RecordItemCollected(
                    changeEvent.Value.ItemID,
                    itemName,
                    delta,
                    transform.position
                );
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void InitializeInventoryServerRpc(NetworkItem[] initialItems)
    {
        Inventory.Clear();
        foreach (var item in initialItems)
        {
            Inventory.Add(item);
        }
    }

    [Rpc(SendTo.Server)]
    public void PickupItemRpc(uint pickedItemID, int amount, ulong networkObjectId)
    {

        // Stack items in the list
        bool found = false;
        for (int i = 0; i < Inventory.Count; i++)
        {
            if (Inventory[i].ItemID == pickedItemID)
            {
                NetworkItem updated = Inventory[i];
                updated.Amount += amount;
                Inventory[i] = updated; // This shoots network syncronization
                found = true;
                break;
            }
        }

        if (!found)
        {
            Inventory.Add(new NetworkItem
            {
                ItemID = pickedItemID,
                Amount = amount
            });
        }

        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject itemToDespawn))
        {
            itemToDespawn.Despawn(false); //We don't destroy the game object
        }
    }

    private IEnumerator SaveInventoryCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(saveInterval);
        while (true)
        {
            yield return wait;

            if (isDirty && !isSaving)
            {
                _ = FlushInventorySaveAsync(); //We discard the result: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/discard
            }
        }
    }

    private async Task FlushInventorySaveAsync()
    {
        if (!isDirty || isSaving || CloudSaveManager.Instance == null) return;

        isSaving = true;
        isDirty = false;

        // Copy NetworkList to a standard List to serialize it
        List<NetworkItem> snapshot = new List<NetworkItem>(Inventory.Count);
        foreach (var item in Inventory)
        {
            snapshot.Add(item);
        }

        bool success = await CloudSaveManager.Instance.SaveInventoryAsync(snapshot);

        //In case of error, we will try to save next cycle
        if (!success)
        {
            isDirty = true;
        }

        isSaving = false;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && IsOwner && isDirty)
        {
            _ = FlushInventorySaveAsync();
        }
    }

    private void OnApplicationQuit()
    {
        if (IsOwner && isDirty)
        {
            _ = FlushInventorySaveAsync();
        }
    }

    public void ShowInventory()
    {
        foreach (var networkItem in Inventory)
        {
            // Translate the ID to an item
            ItemData itemData = GetItemDataByID(networkItem.ItemID);
            
            Debug.Log($"I have {networkItem.Amount} of {itemData.ItemName}");
        }
    }

    private ItemData GetItemDataByID(uint id)
    {
        foreach (var item in itemDatabase)
        {
            if (item.ItemID == id) return item;
        }
        return null;
    }
}
