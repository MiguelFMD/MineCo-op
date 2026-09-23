using UnityEngine;
using Unity.Netcode;
public class CollectableItem : NetworkBehaviour
{
    public NetworkVariable<Color> m_NetworkColor = new NetworkVariable<Color>(Color.white);
    public ItemData itemData;
    public int amount;

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        gameObject.SetActive(false); //Turn off the game object
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;

        if(other.TryGetComponent(out PlayerInventory inventory))
        {
           
            if(!inventory.IsOwner) return; //Make sure only the owners inventory gets the object

            inventory.PickupItemRpc(itemData.ItemID,this.amount,NetworkObjectId);
        }
    }
}