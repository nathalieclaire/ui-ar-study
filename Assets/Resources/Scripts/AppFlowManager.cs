using UnityEngine;
using UnityEngine.SceneManagement;

public class AppFlowManager : MonoBehaviour
{
    // Scene name config (edit in Inspector) 
    [Header("Scene Names")]
    public string mainMenuScene   = "MainMenu";
    public string onboardingScene = "Onboarding";
    public string session1Scene   = "Session1";
    public string session2Scene   = "Session2";
    public string setAnchorsScene = "SetAnchors";   // optional for later

    // Singleton so it survives scene loads (avoids accidential duplicates)
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
        Debug.Log("AppFlowManager: READY 🎉");
    }

    // Helpers 
    void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("AppFlowManager: scene name is empty");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
    

    // Main Menu Buttons

    // Button 1: "Set Anchors"
    // For now this can load a placeholder scene (or do nothing if setAnchorsScene left empty)
    public void GoToSetAnchors()
    {
        if (string.IsNullOrEmpty(setAnchorsScene))
        {
            Debug.Log("AppFlowManager.GoToSetAnchors called, but no scene configured yet.");
            return;
        }

        LoadSceneByName(setAnchorsScene);
    }

    // Button 2: "Onboarding"  - Onboarding scene
    public void StartOnboarding()
    {
        LoadSceneByName(onboardingScene);
    }

    // Button 3: "Start Session" - Session 1
    public void StartSession()
    {
        LoadSceneByName(session1Scene);
    }


    // Session flow methods to use inside scenes

    // Call from UI in Session 1 to go directly to Session 2
    public void GoToSession2()
    {
        LoadSceneByName(session2Scene);
    }

    // Call from UI in Session 2 to go back to main menu
    public void ReturnToMainMenu()
    {
        LoadSceneByName(mainMenuScene);
    }
}
