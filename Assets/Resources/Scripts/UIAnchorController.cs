using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// public class OpenMenuOnSelect : MonoBehaviour
// {
//     public Transform uiRoot;        // UI_grey_parent
//     public bool headAnchored = true;
//     public Transform objectAnchor;  // Cube_UI_anchor

//     public void OnSelectEntered(SelectEnterEventArgs args)
//     {
//         uiRoot.gameObject.SetActive(true);

//         if (headAnchored)
//         {
//             var cam = Camera.main.transform;
//             uiRoot.SetParent(cam, false);
//             uiRoot.localPosition = new Vector3(0, 0, 0.5f);
//             uiRoot.localRotation = Quaternion.identity;
//         }
//         else
//         {
//             uiRoot.SetParent(objectAnchor, false);
//             uiRoot.localPosition = Vector3.zero;
//             uiRoot.localRotation = Quaternion.identity;
//         }
//     }
// }

public class UIAnchorController : MonoBehaviour
{
    public Transform uiRoot;        // UI_grey_parent
    public bool headAnchored = true;
    public Transform objectAnchor;  // Cube_UI_anchor

    public void Mount()
    {
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