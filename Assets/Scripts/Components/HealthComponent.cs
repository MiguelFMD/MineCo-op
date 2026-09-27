using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += OnHealthChanged;
        if(!IsServer) return;
        currentHealth.Value = maxHealth;
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
        EventManager.OnHealthChanged?.Invoke(NetworkObjectId, currentHealth.Value);
    }
    
    /// <summary>
    /// Adds (or reduce) health points to the current health points. Min: 0, Max: MaxHealth.
    /// </summary>
    /// <param name="points">The amount of health to be added to the current health. It can be a negative value.</param>
    public void AddHealth(float points)
    {
        if(!IsServer) return; //Make sure only affects the player owner.
        currentHealth.Value = Math.Clamp(currentHealth.Value + points, 0, maxHealth);
    }
}
