using UnityEngine;
using UnityEngine.SceneManagement;

public class AppFlowManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string session1Scene = "Session_OA_HA";

    public static AppFlowManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[AppFlowManager] Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // Main Menu button: "Start Session"
    public void StartSession()
    {
        LoadScene(session1Scene);
    }

    // In-session button: "Return to Main Menu"
    public void ReturnToMainMenu()
    {
        LoadScene(mainMenuScene);
    }
}