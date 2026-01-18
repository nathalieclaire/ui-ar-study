using UnityEngine;

public class AppFlowButtonBridge : MonoBehaviour
{
    public void StartSession()
    {
        AppFlowManager.Instance?.StartSession();
    }

    public void ReturnToMainMenu()
    {
        AppFlowManager.Instance?.ReturnToMainMenu();
    }
}