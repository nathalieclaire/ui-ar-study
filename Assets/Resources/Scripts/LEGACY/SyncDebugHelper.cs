using UnityEngine;
using UnityEngine.UI;

public class SyncDebugHelper : MonoBehaviour
{
    public Button syncButton;

    public void OnSyncClicked()
    {
        Debug.Log("SYNC BUTTON CLICKED!");

        if (syncButton != null)
        {
            var colors = syncButton.colors;
            colors.normalColor = Color.green;
            syncButton.colors = colors;
        }
    }
}