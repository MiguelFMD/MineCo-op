using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof (DamagerComponent))]
[RequireComponent (typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileTriggerComponent : NetworkBehaviour
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
    public void OnTriggerEnter(Collider other)
    {
        if(!IsClient) return;
        if(other.TryGetComponent(out NetworkObject networkObject))
        {
            print("Se chocado: " + networkObject.name);
            damagerComponent.Damage(networkObject);
        }
        GetComponent<NetworkObject>().Despawn();
    }
}
