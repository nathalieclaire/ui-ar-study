using UnityEngine;

public class LoggingButtonBridge : MonoBehaviour
{
    public void BeginNewPlayer()
    {
        LoggingManager.Instance?.BeginNewPlayer();
    }

    public void SendObjectAnchoredResult()
    {
        LoggingManager.Instance?.SendOAResult();
    }

    public void SendHeadAnchoredResult()
    {
        LoggingManager.Instance?.SendHAResult();
    }

    public void SyncPendingNow()
    {
        LoggingManager.Instance?.SyncPendingNow();
    }
}