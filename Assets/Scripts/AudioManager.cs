using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip uiClick;
    public AudioClip uiSwitch;
    public AudioClip uiConfirm;
    public AudioClip uiError;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayUIClick()
    {
        if (uiClick != null)
        {
            audioSource.PlayOneShot(uiClick);
        }
    }

    public void PlayUISwitch()
    {
        if (uiSwitch != null)
        {
            audioSource.PlayOneShot(uiSwitch);
        }
    }

    public void PlayUIConfirm()
    {
        if (uiConfirm != null)
        {
            audioSource.PlayOneShot(uiConfirm);
        }
    }

    public void PlayUIError()
    {
        if (uiError != null)
        {
            audioSource.PlayOneShot(uiError);
        }
    }
}