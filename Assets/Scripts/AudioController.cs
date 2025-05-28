using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("Efectos de Sonido")]
    public AudioClip selectClip;
    public AudioClip deselectClip;
    public AudioClip confirmClip;

    [Header("Música")]
    public AudioClip backgroundMusic;
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.3f; // volumen más bajo para música
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlaySelect()
    {
        sfxSource.PlayOneShot(selectClip);
    }

    public void PlayDeselect()
    {
        sfxSource.PlayOneShot(deselectClip);
    }

    public void PlayConfirm()
    {
        sfxSource.PlayOneShot(confirmClip);
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
}
