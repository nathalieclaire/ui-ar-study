using UnityEngine;

public class ParentWatch : MonoBehaviour
{
    Transform lastParent;

    void Start()
    {
        lastParent = transform.parent;
        Debug.Log($"[ParentWatch] {name} initial parent = {lastParent?.name}");
    }

    void Update()
    {
        if (transform.parent != lastParent)
        {
            Debug.Log(
                $"[ParentWatch] {name} parent CHANGED: " +
                $"{lastParent?.name}  →  {transform.parent?.name}"
            );
            lastParent = transform.parent;
        }
    }
}