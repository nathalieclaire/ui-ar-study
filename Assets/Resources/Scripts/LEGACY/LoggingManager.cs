using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking; // UnityWebRequest

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
    //  Pending queue (offline safe)
    // ─────────────────────────────────────────────

    const string PendingKey = "LOG_PENDING_ROWS_V1";

    [System.Serializable]
    class PendingWrapper
    {
        public List<string> rows = new List<string>();
    }

    List<string> pendingRows = new List<string>();

    [Header("Debug only")]
    [SerializeField] int pendingCount;

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

        LoadPendingQueue();
    }

    void LoadPendingQueue()
    {
        string json = PlayerPrefs.GetString(PendingKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<PendingWrapper>(json);
                if (wrapper != null && wrapper.rows != null)
                    pendingRows = wrapper.rows;
            }
            catch
            {
                pendingRows = new List<string>();
            }
        }

        pendingCount = pendingRows.Count;
        Debug.Log($"[LOG] Loaded pending queue. Rows={pendingCount}");
    }

    void SavePendingQueue()
    {
        var wrapper = new PendingWrapper { rows = pendingRows };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PendingKey, json);
        PlayerPrefs.Save();
        pendingCount = pendingRows.Count;
    }

    void EnqueueRow(string jsonBody)
    {
        pendingRows.Add(jsonBody);
        SavePendingQueue();
        Debug.Log($"[LOG] Enqueued row. Pending now = {pendingRows.Count}");
    }

    // Public API to manually sync
    public void SyncPendingNow()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[LOG] Cannot sync, LoggingManager object inactive.");
            return;
        }

        StartCoroutine(SendPendingRoutine());
    }

    IEnumerator SendPendingRoutine()
    {
        if (string.IsNullOrEmpty(sheetDbUrl))
        {
            Debug.LogWarning("[LOG] SheetDB URL empty – cannot sync.");
            yield break;
        }

        if (pendingRows.Count == 0)
        {
            Debug.Log("[LOG] No pending rows to sync.");
            yield break;
        }

        Debug.Log($"[LOG] Sync: trying to send {pendingRows.Count} pending rows...");

        int index = 0;
        while (index < pendingRows.Count)
        {
            string body = pendingRows[index];

            using (UnityWebRequest req = new UnityWebRequest(sheetDbUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
                req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[LOG] Row synced: " + req.downloadHandler.text);
                    pendingRows.RemoveAt(index);  // list shrinks
                    SavePendingQueue();           // update PlayerPrefs + pendingCount
                }
                else
                {
                    Debug.LogWarning("[LOG] Sync failed, keeping row: " + req.error);
                    index++; // try this row again next time
                }
            }
        }

        Debug.Log($"[LOG] Sync finished. Pending rows left = {pendingRows.Count}");
    }

    // ─────────────────────────────────────────────
    //  Start of experiment  (called from MainMenu)
    // ─────────────────────────────────────────────

    public void BeginNewPlayer()
    {
        StartCoroutine(BeginNewPlayerRoutine());
    }

    // still uses Google Sheet as source of truth for playerId
    private IEnumerator BeginNewPlayerRoutine()
    {
        if (string.IsNullOrEmpty(sheetDbUrl))
        {
            Debug.LogError("[LOG] sheetDbUrl is empty – cannot fetch last player ID.");
            yield break;
        }

        string url = sheetDbUrl +
                     "?sort_by=playerId" +
                     "&sort_order=desc" +
                     "&limit=1" +
                     "&single_object=true";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[LOG] Failed to GET last playerId from SheetDB: " + req.error);
                yield break; // no playerId → don’t start participant
            }

            string json = req.downloadHandler.text;

            string lastPlayerId = ExtractPlayerIdFromJson(json);

            int newNumber = 1;
            if (!string.IsNullOrEmpty(lastPlayerId) &&
                lastPlayerId.StartsWith(headsetPrefix))
            {
                string numberPart = lastPlayerId.Substring(headsetPrefix.Length);
                if (int.TryParse(numberPart, out int parsed))
                {
                    newNumber = parsed + 1;
                }
            }

            playerId = headsetPrefix + newNumber;

            ResetAllCounters();

            Debug.Log($"[LOG] ▶ New Player Started — PlayerID = {playerId} (via SheetDB)");
        }
    }

    private string ExtractPlayerIdFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        var match = Regex.Match(json, "\"playerId\"\\s*:\\s*\"([^\"]+)\"");
        if (match.Success)
            return match.Groups[1].Value;
        return null;
    }

    void ResetAllCounters()
    {
        System.Array.Clear(oaStation, 0, 3);
        System.Array.Clear(oaMC,      0, 3);
        System.Array.Clear(haStation, 0, 3);
        System.Array.Clear(haMC,      0, 3);
    }

    // ─────────────────────────────────────────────
    //  Error logging (same as before)
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
    //  OA / HA result → enqueue + (try) sync
    // ─────────────────────────────────────────────

    public void SendOAResult()
    {
        int total =
            oaStation[0] + oaStation[1] + oaStation[2] +
            oaMC[0]      + oaMC[1]      + oaMC[2];

        float mean = total / 6f;

        Debug.Log(
            $"[LOG] ✅ OA SUMMARY (Player {playerId}) — " +
            $"S1:{oaStation[0]}/MC1:{oaMC[0]} | " +
            $"S2:{oaStation[1]}/MC2:{oaMC[1]} | " +
            $"S3:{oaStation[2]}/MC3:{oaMC[2]} | " +
            $"Total={total}, Mean={mean}"
        );

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

        EnqueueRow(jsonBody);
        // try once immediately (if offline it just fails & keeps row)
        SyncPendingNow();
    }

    public void SendHAResult()
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

        EnqueueRow(jsonBody);
        SyncPendingNow();
    }
}