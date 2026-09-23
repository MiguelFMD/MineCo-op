using Unity.Netcode;
using UnityEngine;

[RequireComponent (typeof (DamagerComponent))]
[RequireComponent (typeof(Collider))]
public class TriggerDamageComponent : NetworkBehaviour
{
    DamagerComponent damagerComponent;
    Collider colliderComponent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        damagerComponent = GetComponent<DamagerComponent>();
        colliderComponent = GetComponent<Collider>();
        colliderComponent.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!IsClient) return;
        if(other.TryGetComponent(out NetworkObject networkObject) && networkObject.IsOwner)
        {
            damagerComponent.Damage(networkObject);
        }
    }
}
