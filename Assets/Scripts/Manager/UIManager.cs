using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image healthbar_1;
    [SerializeField] private Image healthbar_2;
    private Material healthBarMaterial_1;
    private Material healthBarMaterial_2;
    void Start()
    {
        EventManager.OnHealthChanged += ChangeHealthBars;
        EventManager.OnPlayerDied += ShowDeathScreen;
        healthBarMaterial_1 = new Material(healthbar_1.material);
        healthbar_1.material = healthBarMaterial_1;
        healthBarMaterial_2 = new Material(healthbar_2.material);
        healthbar_2.material = healthBarMaterial_2;
    }

    void OnDestroy()
    {
        EventManager.OnHealthChanged -= ChangeHealthBars;
        EventManager.OnPlayerDied -= ShowDeathScreen;
    }

    private void ChangeHealthBars(ulong playerId, float value, float maxValue)
    {
        float healthNormalized = value / maxValue;
        if(playerId % 2 == 0)
        {
            healthBarMaterial_2.SetFloat("_Health", healthNormalized);
        }
        else
        {
            healthBarMaterial_1.SetFloat("_Health", healthNormalized);
        }
    }

    private void ShowDeathScreen(ulong playerId)
    {
        
    }
}
