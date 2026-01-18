using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class PlantPickupSound : MonoBehaviour
{
    [Header("Pickup Sound")]
    public AudioClip pickupClip;

    AudioSource source;
    XRGrabInteractable grab;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (grab != null)
            grab.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        Play();
    }

    void Play()
    {
        if (pickupClip != null)
            source.PlayOneShot(pickupClip);
    }
}