using UnityEngine;

public class LoggingManager : MonoBehaviour
{
    public static LoggingManager Instance { get; private set; }

    public int playerId = 0;

    public int[] oaStation = new int[3];
    public int[] oaMC      = new int[3];

    public int[] haStation = new int[3];
    public int[] haMC      = new int[3];

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

    // start
    public void BeginNewPlayer()
    {
        playerId++;          // only in RAM
        ResetAllCounters();

        Debug.Log($"[LOG] ▶ New Player Started — PlayerID = {playerId}");
    }

    void ResetAllCounters()
    {
        System.Array.Clear(oaStation, 0, 3);
        System.Array.Clear(oaMC, 0, 3);
        System.Array.Clear(haStation, 0, 3);
        System.Array.Clear(haMC, 0, 3);
    }

    // ERROR LOGGING 
    public void AddStationError(int trialIndex, bool headAnchored)
    {
        if (trialIndex < 0 || trialIndex > 2) return;

        if (headAnchored)
        {
            haStation[trialIndex]++;
            Debug.Log($"[LOG] ❌ HA Station error — Plant {trialIndex + 1} → Count = {haStation[trialIndex]}");
        }
        else
        {
            oaStation[trialIndex]++;
            Debug.Log($"[LOG] ❌ OA Station error — Plant {trialIndex + 1} → Count = {oaStation[trialIndex]}");
        }
    }

    public void AddMCError(int trialIndex, bool headAnchored)
    {
        if (trialIndex < 0 || trialIndex > 2) return;

        if (headAnchored)
        {
            haMC[trialIndex]++;
            Debug.Log($"[LOG] ❌ HA MC error — Plant {trialIndex + 1} → Count = {haMC[trialIndex]}");
        }
        else
        {
            oaMC[trialIndex]++;
            Debug.Log($"[LOG] ❌ OA MC error — Plant {trialIndex + 1} → Count = {oaMC[trialIndex]}");
        }
    }

    // SESSION SUMMARY

    public void SendOAResult()
    {
        int total =
            oaStation[0]+oaStation[1]+oaStation[2]+
            oaMC[0]+oaMC[1]+oaMC[2];

        Debug.Log(
            $"[LOG] ✅ OA SUMMARY (Player {playerId}) — " +
            $"S1:{oaStation[0]}/MC1:{oaMC[0]} | " +
            $"S2:{oaStation[1]}/MC2:{oaMC[1]} | " +
            $"S3:{oaStation[2]}/MC3:{oaMC[2]} | " +
            $"Total={total}"
        );
    }

    public void SendHAResult()
    {
        int total =
            haStation[0]+haStation[1]+haStation[2]+
            haMC[0]+haMC[1]+haMC[2];

        Debug.Log(
            $"[LOG] ✅ HA SUMMARY (Player {playerId}) — " +
            $"S1:{haStation[0]}/MC1:{haMC[0]} | " +
            $"S2:{haStation[1]}/MC2:{haMC[1]} | " +
            $"S3:{haStation[2]}/MC3:{haMC[2]} | " +
            $"Total={total}"
        );
    }
}
