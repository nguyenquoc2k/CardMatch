using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)] public float masterVolume = 1f; // Dùng chung cho cả music và sound
    public bool isMusicMuted = false;
    public bool isSoundMuted = false;

    public AudioSource musicSource; // Nhạc nền

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
    }

    public void SetVolume(float volume)
    {
        masterVolume = volume;
        UpdateAllAudioSources();
    }

    public void ToggleMusicMute()
    {
        isMusicMuted = !isMusicMuted;
        if (musicSource) musicSource.volume = isMusicMuted ? 0f : masterVolume;
    }

    public void ToggleSoundMute()
    {
        isSoundMuted = !isSoundMuted;
        UpdateAllSoundSources();
    }

    private void UpdateAllAudioSources()
    {
        if (musicSource) musicSource.volume = isMusicMuted ? 0f : masterVolume;
        UpdateAllSoundSources();
    }

    private void UpdateAllSoundSources()
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        foreach (var source in allSources)
        {
            if (source != musicSource) // Không ảnh hưởng đến nhạc nền
            {
                source.volume = isSoundMuted ? 0f : masterVolume;
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = isMusicMuted ? 0f : masterVolume;
            musicSource.Play();
        }
    }

    public void PlayAudio(AudioSource source)
    {
        if (source == null || isSoundMuted) return;
        source.volume = masterVolume;
        source.Play();
    }
    public void StopAudio(AudioSource source)
    {
        if (source == null || isSoundMuted) return;
        source.volume = 0;
        source.Stop();
    }
    public void MuteMusicTemporarily(bool mute)
    {
        if (!isMusicMuted) // Chỉ thực hiện nếu nhạc nền chưa bị mute
        {
            musicSource.volume = mute ? 0f : masterVolume;
        }
    }
}