using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIProximityFader : MonoBehaviour
{
    [Header("Target (the plant/cube this UI belongs to)")]
    public Transform target;                 // drag the cube/plant here

    [Header("Distances (meters)")]
    public float fadeOutDistance = 1.4f;     // beyond this -> fade OUT
    public float fadeInDistance  = 1.1f;     // closer than this -> fade IN (hysteresis)

    [Header("Fade")]
    public float fadeDuration = 0.25f;

    [Header("Optional")]
    public bool useXZOnly = false;           // ignore height difference
    public bool disableInteractionWhenHidden = true;

    CanvasGroup cg;
    float targetAlpha = 1f;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        // start consistent
        cg.alpha = Mathf.Clamp01(cg.alpha);
        ApplyInteractionFromAlpha();
    }

    void Update()
    {
        if (target == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 a = cam.transform.position;
        Vector3 b = target.position;

        if (useXZOnly)
        {
            a.y = 0f;
            b.y = 0f;
        }

        float d = Vector3.Distance(a, b);

        // hysteresis: prevents flickering around a single threshold
        if (cg.alpha > 0.5f)
        {
            if (d >= fadeOutDistance) targetAlpha = 0f;
        }
        else
        {
            if (d <= fadeInDistance) targetAlpha = 1f;
        }

        // smooth fade
        float speed = (fadeDuration <= 0f) ? 9999f : (1f / fadeDuration);
        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, speed * Time.deltaTime);

        ApplyInteractionFromAlpha();
    }

    void ApplyInteractionFromAlpha()
    {
        if (!disableInteractionWhenHidden) return;

        bool visible = cg.alpha >= 0.99f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}