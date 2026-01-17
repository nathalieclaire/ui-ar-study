using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationCheck : MonoBehaviour
{
    public string myStationTag;               // "WaterStation" or "SunStation"
    public Transform snapPoint;
    public Renderer stationRenderer;

    [Header("Flash Colors")]
    public Color okColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Flash Timing")]
    public float flashFadeDuration = 0.15f;
    public float flashHoldDuration = 0.25f;

    [Header("Flash Cooldowns (per plant)")]
    public float wrongFlashCooldown = 5f;
    public float okFlashCooldown = 1.5f;

    SceneFlowManager flow;
    Color originalColor;
    Coroutine flashRoutine;

    // cooldown tracking per plant
    readonly Dictionary<int, float> wrongFlashNextAllowed = new();
    readonly Dictionary<int, float> okFlashNextAllowed = new();

    void Start()
    {
        flow = FindFirstObjectByType<SceneFlowManager>();
        if (stationRenderer == null)
            stationRenderer = GetComponentInChildren<Renderer>();

        if (stationRenderer != null)
            originalColor = stationRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (flow == null) return;

        var trial = other.GetComponentInParent<CubeTrial>();
        if (trial == null) return;
        if (flow.GetActiveTrial() != trial) return;
        if (trial.snappedCorrectly) return;

        bool correct = (trial.requiredStationTag == myStationTag);
        int id = trial.GetInstanceID();
        float now = Time.time;

        if (correct)
        {
            // cooldown for GREEN flash
            if (!okFlashNextAllowed.TryGetValue(id, out float nextOk) || now >= nextOk)
            {
                okFlashNextAllowed[id] = now + okFlashCooldown;
                TriggerFlash(okColor);
            }

            StartCoroutine(SnapWhenReleased(trial));
            return;
        }

        // WRONG station logic
        if (!trial.stationErrorLogged)
        {
            trial.stationErrorLogged = true;
            var logger = trial.GetComponent<TrialLogger>();
            if (logger != null) logger.LogStationError();
        }

        // cooldown for RED flash
        if (!wrongFlashNextAllowed.TryGetValue(id, out float nextWrong) || now >= nextWrong)
        {
            wrongFlashNextAllowed[id] = now + wrongFlashCooldown;
            TriggerFlash(wrongColor);
        }
    }

    void TriggerFlash(Color targetColor)
    {
        if (stationRenderer == null) return;

        // safety: if material changed at runtime, refresh baseline
        originalColor = stationRenderer.material.color;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(targetColor));
    }

    IEnumerator FlashRoutine(Color target)
    {
        yield return FadeColor(originalColor, target, flashFadeDuration);
        yield return new WaitForSeconds(flashHoldDuration);
        yield return FadeColor(target, originalColor, flashFadeDuration);
    }

    IEnumerator FadeColor(Color from, Color to, float duration)
    {
        if (stationRenderer == null) yield break;

        if (duration <= 0f)
        {
            stationRenderer.material.color = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            stationRenderer.material.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }

        stationRenderer.material.color = to;
    }

    IEnumerator SnapWhenReleased(CubeTrial trial)
    {
        if (trial.snappedCorrectly) yield break;

        var grab = trial.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        while (grab != null && grab.isSelected)
            yield return null;

        if (trial.snappedCorrectly) yield break;

        if (snapPoint != null)
            trial.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);

        var symbol = trial.symbol?.GetComponent<SymbolVisibilitySuccess>();
        if (symbol != null) symbol.enabled = false;

        trial.snappedCorrectly = true;

        // reset cooldown tracking once success is locked in
        int id = trial.GetInstanceID();
        wrongFlashNextAllowed.Remove(id);
        okFlashNextAllowed.Remove(id);

        flow.OnPlantSnappedCorrectly();
        flow.ShowPage4();

        var rb = trial.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (grab != null) grab.enabled = false;

        trial.transform.SetParent(transform, true);
    }
}