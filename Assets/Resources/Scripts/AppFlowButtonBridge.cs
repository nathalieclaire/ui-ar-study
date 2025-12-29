using UnityEngine;

public class AppFlowButtonBridge : MonoBehaviour
{
    public void GoToSetAnchors()
    {
        AppFlowManager.Instance?.GoToSetAnchors();
    }

    public void StartOnboarding()
    {
        AppFlowManager.Instance?.StartOnboarding();
    }

    public void StartSession()
    {
        AppFlowManager.Instance?.StartSession();
    }

    public void GoToSession2()
    {
        AppFlowManager.Instance?.GoToSession2();
    }

    public void ReturnToMainMenu()
    {
        AppFlowManager.Instance?.ReturnToMainMenu();
    }
}
