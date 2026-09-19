using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    private const string GameSceneName = "MainScene";

    void Update()
    {
        if(Keyboard.current.gKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvents;
        
    }

    public void StartClient()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvents;
        
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvents;
        }
    }

    // New StartGame but only works for the host to initiate the lobby
    public void StartGame()
    {

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the host/server can initiate the game.");
            return;
        }

        // Syncronize all clients.
        SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.LoadScene(
            GameSceneName, 
            LoadSceneMode.Single
        );

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Error trying to load the scene: {status}");
        }
    }

    // Handles when the clients are ready
    private void HandleSceneEvents(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            // When each client and sever have completed the scene load
            case SceneEventType.LoadComplete:
                Debug.Log($"The client {sceneEvent.ClientId} has finished loading the scene {sceneEvent.SceneName}.");
                break;

            // Only fires on server side when ALL connected clients have finished loading the scene
            case SceneEventType.LoadEventCompleted:
                if (NetworkManager.Singleton.IsServer && sceneEvent.SceneName == GameSceneName)
                {
                    Debug.Log("Every playes has loaded the scene. The game can begin.");
                }
                break;
        }
    }
}