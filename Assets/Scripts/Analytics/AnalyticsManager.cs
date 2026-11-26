using Unity.Services.Analytics;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private bool CanRecord()
    {
        return AnalyticsConsentManager.Instance != null &&
               AnalyticsConsentManager.Instance.IsCollectionEnabled;
    }

    public void RecordMatchJoined(bool isHost)
    {
        if (!CanRecord()) return;

        CustomEvent matchJoinedEvent = new CustomEvent("MatchJoined")
        {
            { "role", isHost ? "Host" : "Client" }
        };

        AnalyticsService.Instance.RecordEvent(matchJoinedEvent);
        Debug.Log($"[Analytics] Recorded MatchJoined (role: {(isHost ? "Host" : "Client")})");
    }

    public void RecordItemCollected(uint itemId, string itemName, int amount, Vector3 worldPosition)
    {
        if (!CanRecord()) return;

        CustomEvent itemEvent = new CustomEvent("ItemCollected")
        {
            { "itemId", (int)itemId },
            { "itemName", itemName },
            { "amount", amount },
            { "posX", worldPosition.x },
            { "posY", worldPosition.y },
            { "posZ", worldPosition.z }
        };

        AnalyticsService.Instance.RecordEvent(itemEvent);
        Debug.Log($"[Analytics] Recorded ItemCollected: {itemName} (x{amount}) at {worldPosition}");
    }

    public void RecordPlayerDied(int itemsHeld, Vector3 deathPosition)
    {
        if (!CanRecord()) return;

        CustomEvent deathEvent = new CustomEvent("PlayerDied")
        {
            { "itemsHeld", itemsHeld },
            { "posX", deathPosition.x },
            { "posY", deathPosition.y },
            { "posZ", deathPosition.z }
        };

        AnalyticsService.Instance.RecordEvent(deathEvent);
        FlushEvents(); // Immediately upload death event before respawn or disconnect
        Debug.Log($"[Analytics] Recorded PlayerDied with {itemsHeld} item stacks at {deathPosition}");
    }

    public void FlushEvents()
    {
        if (!CanRecord()) return;
        AnalyticsService.Instance.Flush();
    }
}