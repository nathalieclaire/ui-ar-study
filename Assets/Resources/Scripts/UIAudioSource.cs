using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
    public static UIAudioPlayer Instance;

    [Header("UI Sounds")]
    public AudioClip click;
    public AudioClip correct;
    public AudioClip wrong;

    AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
    }

    public void PlayClick()  => source.PlayOneShot(click);
    public void PlayCorrect() => source.PlayOneShot(correct);
    public void PlayWrong() => source.PlayOneShot(wrong);
}