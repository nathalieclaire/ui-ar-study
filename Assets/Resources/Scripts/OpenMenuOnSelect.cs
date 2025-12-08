using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenMenuOnSelect : MonoBehaviour
{
    public Canvas menuCanvas;
    public bool headAnchored = true;
    public Transform objectAnchor;   // only used for cube case

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        menuCanvas.gameObject.SetActive(true);

        if (headAnchored)
        {
            var cam = Camera.main.transform;
            menuCanvas.transform.SetParent(cam, false);
            menuCanvas.transform.localPosition = new Vector3(0, 0, 0.5f);
            menuCanvas.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (objectAnchor == null) return;
            menuCanvas.transform.SetParent(objectAnchor, false);
            menuCanvas.transform.localPosition = Vector3.zero; // the canvas becomes a child of the objectAnchor
            menuCanvas.transform.localRotation = Quaternion.identity;
            // Vector3 worldPos = objectAnchor.position + Vector3.up * 0.1f;
            // menuCanvas.transform.position = worldPos;
            // menuCanvas.transform.SetParent(objectAnchor, true);
        }
    }
}