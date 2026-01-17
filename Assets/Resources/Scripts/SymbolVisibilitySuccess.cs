using UnityEngine;

public class SymbolVisibilitySuccess : MonoBehaviour
{
    public SceneFlowManager flow;
    public float secondsNeeded = 3f;

    [Header("Visuals")]
    [Tooltip("Renderer on the CHILD object (e.g. drop-lowpoly) to change mat")]
    public Renderer symbolRenderer;

    public Color idleColor = Color.red;
    public Color detectedColor = Color.green;

    float visibleTime = 0f;
    bool isVisible = false;
    bool done = false;

    void OnEnable()
    {
        visibleTime = 0f;
        done = false;
        isVisible = false;

        // If not assigned manually, try to find a Renderer in children
        if (symbolRenderer == null)
            symbolRenderer = GetComponentInChildren<Renderer>();

        if (symbolRenderer != null)
            symbolRenderer.material.color = idleColor;
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
        visibleTime = 0f;

        if (symbolRenderer != null)
            symbolRenderer.material.color = idleColor;
    }

    void Update()
    {
        if (done || !isVisible) return;

        // check occlusion
        if (!IsActuallyVisible())
        {
            visibleTime = 0f;
            return;
        }

        visibleTime += Time.deltaTime;

        if (visibleTime >= secondsNeeded)
        {
            done = true;

            if (symbolRenderer != null)
                symbolRenderer.material.color = detectedColor;

            flow.ShowPage3();
        }
    }

    bool IsActuallyVisible()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 toSymbol = transform.position - cam.transform.position;
        float dist = toSymbol.magnitude;

        Ray ray = new Ray(cam.transform.position, toSymbol.normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, dist + 0.01f))
        {
            // still check visibility against the PARENT
            return hit.collider.GetComponentInParent<SymbolVisibilitySuccess>() == this;
        }

        return false;
    }
}