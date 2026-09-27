using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image healthbar_1;
    [SerializeField] private Image healthbar_2;
    void Start()
    {
        EventManager.OnHealthChanged += ChangeHealthBars;
        EventManager.OnPlayerDied += ShowDeathScreen;
    }

    void OnDestroy()
    {
        EventManager.OnHealthChanged -= ChangeHealthBars;
        EventManager.OnPlayerDied -= ShowDeathScreen;
    }

    private void ChangeHealthBars(ulong playerId, float value)
    {
        
        if(playerId % 2 == 0)
        {
            print("Cambiando la vida de player 1");
            healthbar_1.fillAmount = 0.5f;
        }
        else
        {
            print("Cambiando la vida de player 2");
            healthbar_2.fillAmount = 0.5f;
        }
    }

    private void ShowDeathScreen(ulong playerId)
    {
        
    }
}
