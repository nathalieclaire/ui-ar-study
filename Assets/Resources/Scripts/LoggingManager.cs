using UnityEngine;
using System.Collections;
using UnityEngine.Networking;   // for UnityWebRequest

public class LoggingManager : MonoBehaviour
{
    public static LoggingManager Instance { get; private set; }

    // ─────────────────────────────────────────────
    //  SheetDB + Headset config
    // ─────────────────────────────────────────────

    [Header("SheetDB")]
    [Tooltip("Base URL of your SheetDB API, e.g. https://sheetdb.io/api/v1/XXXXXXX")]
    public string sheetDbUrl;   // set this in Inspector

    [Header("Headset ID prefix (e.g. PA, PB)")]
    public string headsetPrefix = "PA";   // Headset A: "PA", another headset later: "PB"

    [Header("Current Player (debug only)")]
    public string playerId = "";          // e.g. "PA3"


    // ─────────────────────────────────────────────
    //  Error arrays: 3 plants per condition
    // ─────────────────────────────────────────────

    [Header("Object-anchored errors (Session 1)")]
    public int[] oaStation = new int[3];
    public int[] oaMC      = new int[3];

    [Header("Head-anchored errors (Session 2)")]
    public int[] haStation = new int[3];
    public int[] haMC      = new int[3];


    // ─────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────

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


    // ─────────────────────────────────────────────
    //  Start of experiment  (called from MainMenu)
    // ─────────────────────────────────────────────
    //
    //  Call via LoggingButtonBridge.BeginNewPlayer()
    //

    public void BeginNewPlayer()
    {
        // Each headset keeps its own local counter:
        // e.g. "LastPlayerNumber_PA" on this device
        string key = "LastPlayerNumber_" + headsetPrefix;

        int lastNumber = PlayerPrefs.GetInt(key, 0);  // default 0 if none yet
        lastNumber++;
        PlayerPrefs.SetInt(key, lastNumber);
        PlayerPrefs.Save();

        playerId = headsetPrefix + lastNumber;        // e.g. "PA3"

        ResetAllCounters();

        Debug.Log($"[LOG] ▶ New Player Started — PlayerID = {playerId}");
    }

    void ResetAllCounters()
    {
        System.Array.Clear(oaStation, 0, 3);
        System.Array.Clear(oaMC,      0, 3);
        System.Array.Clear(haStation, 0, 3);
        System.Array.Clear(haMC,      0, 3);
    }


    // ─────────────────────────────────────────────
    //  Error logging  (called from TrialLogger)
    // ─────────────────────────────────────────────

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


    // ─────────────────────────────────────────────
    //  SESSION SUMMARY + POST OA (Session 1)
    //  Call from Session1 end button:
    //   LoggingButtonBridge.SendObjectAnchoredResult()
    // ─────────────────────────────────────────────

    public void SendOAResult()
    {
        StartCoroutine(SendOAResultRoutine());
    }

    IEnumerator SendOAResultRoutine()
    {
        int total =
            oaStation[0] + oaStation[1] + oaStation[2] +
            oaMC[0]      + oaMC[1]      + oaMC[2];

        // 6 values total (3 station + 3 MC)
        float mean = total / 6f;

        Debug.Log(
            $"[LOG] ✅ OA SUMMARY (Player {playerId}) — " +
            $"S1:{oaStation[0]}/MC1:{oaMC[0]} | " +
            $"S2:{oaStation[1]}/MC2:{oaMC[1]} | " +
            $"S3:{oaStation[2]}/MC3:{oaMC[2]} | " +
            $"Total={total}, Mean={mean}"
        );

        if (string.IsNullOrEmpty(sheetDbUrl))
        {
            Debug.LogWarning("[LOG] SheetDB URL is empty, skipping OA POST.");
            yield break;
        }

        // Google Sheet columns:
        // playerId | condition | station1 | mc1 | station2 | mc2 | station3 | mc3 | totalErrors | meanError
        string jsonBody =
            "{ \"data\": [ {" +
            $"\"playerId\":\"{playerId}\"," +
            "\"condition\":\"OA\"," +
            $"\"station1\":\"{oaStation[0]}\"," +
            $"\"mc1\":\"{oaMC[0]}\"," +
            $"\"station2\":\"{oaStation[1]}\"," +
            $"\"mc2\":\"{oaMC[1]}\"," +
            $"\"station3\":\"{oaStation[2]}\"," +
            $"\"mc3\":\"{oaMC[2]}\"," +
            $"\"totalErrors\":\"{total}\"," +
            $"\"meanError\":\"{mean}\"" +
            "} ] }";

        using (UnityWebRequest req = new UnityWebRequest(sheetDbUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[LOG] OA POST failed: " + req.error);
            }
            else
            {
                Debug.Log("[LOG] OA row sent to SheetDB: " + req.downloadHandler.text);
            }
        }
    }


    // ─────────────────────────────────────────────
    //  SESSION SUMMARY + POST HA (Session 2)
    //  Call from Session2 end button:
    //   LoggingButtonBridge.SendHeadAnchoredResult()
    // ─────────────────────────────────────────────

    public void SendHAResult()
    {
        StartCoroutine(SendHAResultRoutine());
    }

    IEnumerator SendHAResultRoutine()
    {
        int total =
            haStation[0] + haStation[1] + haStation[2] +
            haMC[0]      + haMC[1]      + haMC[2];

        float mean = total / 6f;

        Debug.Log(
            $"[LOG] ✅ HA SUMMARY (Player {playerId}) — " +
            $"S1:{haStation[0]}/MC1:{haMC[0]} | " +
            $"S2:{haStation[1]}/MC2:{haMC[1]} | " +
            $"S3:{haStation[2]}/MC3:{haMC[2]} | " +
            $"Total={total}, Mean={mean}"
        );

        if (string.IsNullOrEmpty(sheetDbUrl))
        {
            Debug.LogWarning("[LOG] SheetDB URL is empty, skipping HA POST.");
            yield break;
        }

        string jsonBody =
            "{ \"data\": [ {" +
            $"\"playerId\":\"{playerId}\"," +
            "\"condition\":\"HA\"," +
            $"\"station1\":\"{haStation[0]}\"," +
            $"\"mc1\":\"{haMC[0]}\"," +
            $"\"station2\":\"{haStation[1]}\"," +
            $"\"mc2\":\"{haMC[1]}\"," +
            $"\"station3\":\"{haStation[2]}\"," +
            $"\"mc3\":\"{haMC[2]}\"," +
            $"\"totalErrors\":\"{total}\"," +
            $"\"meanError\":\"{mean}\"" +
            "} ] }";

        using (UnityWebRequest req = new UnityWebRequest(sheetDbUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[LOG] HA POST failed: " + req.error);
            }
            else
            {
                Debug.Log("[LOG] HA row sent to SheetDB: " + req.downloadHandler.text);
            }
        }
    }
}