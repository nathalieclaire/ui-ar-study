using UnityEngine;

public class TrialLogger : MonoBehaviour
{
    public int trialIndex;      
    public bool headAnchored;   

    public void LogStationError()
    {
        LoggingManager.Instance?.AddStationError(trialIndex, headAnchored);
    }

    public void LogMCError()
    {
        LoggingManager.Instance?.AddMCError(trialIndex, headAnchored);
    }
}
