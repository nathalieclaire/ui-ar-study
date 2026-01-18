using UnityEngine;

public class StationAudio : MonoBehaviour
{
    [Header("Station Sounds")]
    public AudioClip appear;
    public AudioClip correct;
    public AudioClip wrong;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        PlayAppear();
    }

    public void PlayAppear()
    {
        if (appear != null)
            source.PlayOneShot(appear);
    }

    public void PlayCorrect()
    {
        if (correct != null)
            source.PlayOneShot(correct);
    }

    public void PlayWrong()
    {
        if (wrong != null)
            source.PlayOneShot(wrong);
    }
}