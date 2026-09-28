using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if(newValue <= 0 && oldValue > 0)
        {
            EventManager.OnPlayerDied?.Invoke(NetworkObjectId);
        }
        EventManager.OnHealthChanged?.Invoke(NetworkObjectId, currentHealth.Value, maxHealth);
    }
    
    /// <summary>
    /// Adds (or reduce) health points to the current health points. Min: 0, Max: MaxHealth.
    /// </summary>
    /// <param name="points">The amount of health to be added to the current health. It can be a negative value.</param>
    public void AddHealth(float points)
    {
        ChangeHealthServerRpc(points);
    }

    [Rpc(SendTo.Server)]
    private void ChangeHealthServerRpc(float points)
    {
        currentHealth.Value = Math.Clamp(currentHealth.Value + points, 0, maxHealth);
        print(this.name + "se quita vida");
    }
}
