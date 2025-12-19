// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class OpenMenuOnSelect : MonoBehaviour
// {
//     public Canvas menuCanvas;
//     public bool headAnchored = true;
//     public Transform objectAnchor;   // only used for cube case

//     public void OnSelectEntered(SelectEnterEventArgs args)
//     {
//         menuCanvas.gameObject.SetActive(true);

//         if (headAnchored)
//         {
//             var cam = Camera.main.transform;
//             menuCanvas.transform.SetParent(cam, false);
//             menuCanvas.transform.localPosition = new Vector3(0, 0, 0.5f);
//             menuCanvas.transform.localRotation = Quaternion.identity;
//         }
//         else
//         {
//             if (objectAnchor == null) return;
//             menuCanvas.transform.SetParent(objectAnchor, false);
//             menuCanvas.transform.localPosition = Vector3.zero; // the canvas becomes a child of the objectAnchor
//             menuCanvas.transform.localRotation = Quaternion.identity;
//             // Vector3 worldPos = objectAnchor.position + Vector3.up * 0.1f;
//             // menuCanvas.transform.position = worldPos;
//             // menuCanvas.transform.SetParent(objectAnchor, true);
//         }
//     }
// }

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenMenuOnSelect : MonoBehaviour
{
    public Transform uiRoot;        // UI_grey_parent
    public bool headAnchored = true;
    public Transform objectAnchor;  // Cube_UI_anchor

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        uiRoot.gameObject.SetActive(true);

        if (headAnchored)
        {
            var cam = Camera.main.transform;
            uiRoot.SetParent(cam, false);
            uiRoot.localPosition = new Vector3(0, 0, 0.5f);
            uiRoot.localRotation = Quaternion.identity;
        }
        else
        {
            uiRoot.SetParent(objectAnchor, false);
            uiRoot.localPosition = Vector3.zero;
            uiRoot.localRotation = Quaternion.identity;
        }
    }
}