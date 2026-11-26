using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UnityConsent;

public class AnalyticsConsentManager : MonoBehaviour
{
    public static AnalyticsConsentManager Instance { get; private set; }

    [Header("Consent UI")]
    [SerializeField] private GameObject consentPanel;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private GameObject initButtons;

    public bool IsCollectionEnabled =>
        EndUserConsent.GetConsentState().AnalyticsIntent == ConsentStatus.Granted;

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

    private async void Start()
    {
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptConsent);
        if (declineButton != null) declineButton.onClick.AddListener(OnDeclineConsent);

        await InitializeAnalyticsAsync();
    }

    private async Task InitializeAnalyticsAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }

            ConsentState currentConsent = EndUserConsent.GetConsentState();

            if (currentConsent.AnalyticsIntent == ConsentStatus.Unspecified)
            {
                // First time launching: ask the player for permission
                if (consentPanel != null) consentPanel.SetActive(true);
            }
            else
            {
                // Consent choice was already persisted from a previous session
                if (consentPanel != null)
                {
                    consentPanel.SetActive(false);
                    initButtons.SetActive(true);
                }
                Debug.Log($"[AnalyticsConsent] Restored consent state: {currentConsent.AnalyticsIntent}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AnalyticsConsent] Failed to initialize Unity Services: {ex.Message}");
        }
    }

    public void OnAcceptConsent()
    {
        EndUserConsent.SetConsentState(new ConsentState
        {
            AnalyticsIntent = ConsentStatus.Granted
        });

        if (consentPanel != null)
        {
            consentPanel.SetActive(false);
            initButtons.SetActive(true);
        }
        Debug.Log("[AnalyticsConsent] Analytics consent granted.");
    }

    public void OnDeclineConsent()
    {
        EndUserConsent.SetConsentState(new ConsentState
        {
            AnalyticsIntent = ConsentStatus.Denied
        });

        if (consentPanel != null)
        {
            consentPanel.SetActive(false);
            initButtons.SetActive(true);
        }
        Debug.Log("[AnalyticsConsent] Analytics consent denied.");
    }

    public void RevokeConsent()
    {
        OnDeclineConsent();
        Debug.Log("[AnalyticsConsent] User revoked analytics consent.");
    }
}