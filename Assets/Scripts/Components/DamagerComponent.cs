using Unity.Netcode;
using UnityEngine;

public class DamagerComponent : NetworkBehaviour
{
    [SerializeField] private float damageAmount;

    public void Damage(NetworkObject objetive)
    {
        if(objetive.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.AddHealth(-damageAmount);
        }
    }
}
