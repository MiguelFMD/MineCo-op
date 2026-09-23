using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] private float maxHealth;
    private float currentHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Adds (or reduce) health points to the current health points. Min: 0, Max: MaxHealth.
    /// </summary>
    /// <param name="points">The amount of health to be added to the current health. It can be a negative value.</param>
    private void AddHealth(float points)
    {
        currentHealth = Math.Clamp(currentHealth + points, 0, maxHealth);
    }
}
