using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(-3f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    public bool playOnAwake = false;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private SoundClip[] musicClips;
    [SerializeField] private SoundClip[] sfxClips;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Dictionary<string, SoundClip> musicDictionary = new Dictionary<string, SoundClip>();
    private Dictionary<string, SoundClip> sfxDictionary = new Dictionary<string, SoundClip>();

    // Cache audio sources for pooling (optional)
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    [SerializeField] private int poolSize = 10;

    private void Awake()
    {
        // Force enable the GameObject and component
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    
        if (!enabled)
            enabled = true;

        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Initialize SFX pool
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.gameObject.SetActive(false);
            sfxPool.Enqueue(source);
        }

        // Build dictionaries
        BuildMusicDictionary();
        BuildSFXDictionary();

        // Apply volumes
        UpdateVolumes();
    }

    private void Start()
    {
        Debug.Log($"SoundManager - GameObject Active: {gameObject.activeSelf}");
        Debug.Log($"SoundManager - Component Enabled: {enabled}");
        Debug.Log($"SoundManager - MusicSource Active: {(musicSource != null ? musicSource.gameObject.activeSelf.ToString() : "null")}");
        Debug.Log($"SoundManager - SFXSource Active: {(sfxSource != null ? sfxSource.gameObject.activeSelf.ToString() : "null")}");
    }

    #region Dictionary Building
    private void BuildMusicDictionary()
    {
        musicDictionary.Clear();
        if (musicClips != null)
        {
            foreach (var sound in musicClips)
            {
                if (sound != null && sound.clip != null && !musicDictionary.ContainsKey(sound.name))
                {
                    musicDictionary.Add(sound.name, sound);
                }
            }
        }
    }

    private void BuildSFXDictionary()
    {
        sfxDictionary.Clear();
        if (sfxClips != null)
        {
            foreach (var sound in sfxClips)
            {
                if (sound != null && sound.clip != null && !sfxDictionary.ContainsKey(sound.name))
                {
                    sfxDictionary.Add(sound.name, sound);
                }
            }
        }
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    #endregion

    #region Music Methods
    public void PlayMusic(string clipName, float fadeDuration = 0f)
    {
        if (musicDictionary.TryGetValue(clipName, out SoundClip sound))
        {
            PlayMusic(sound, fadeDuration);
        }
        else
        {
            Debug.LogWarning($"Music clip '{clipName}' not found!");
        }
    }

    public void PlayMusic(SoundClip sound, float fadeDuration = 0f)
    {
        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning("Music clip is null!");
            return;
        }

        if (musicSource == null) return;

        // Apply clip settings
        musicSource.clip = sound.clip;
        musicSource.loop = sound.loop;
        musicSource.pitch = sound.pitch;
        musicSource.volume = sound.volume * musicVolume * masterVolume;

        if (fadeDuration > 0)
        {
            StartCoroutine(FadeMusic(sound.clip, fadeDuration));
        }
        else
        {
            musicSource.Play();
        }
    }

    public void StopMusic(float fadeDuration = 0f)
    {
        if (musicSource == null) return;

        if (fadeDuration > 0)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public void SetMusicLoop(bool loop)
    {
        if (musicSource != null)
            musicSource.loop = loop;
    }
    #endregion

    #region SFX Methods
    public void PlaySFX(string clipName)
    {
        if (sfxDictionary.TryGetValue(clipName, out SoundClip sound))
        {
            PlaySFX(sound);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void PlaySFX(SoundClip sound)
    {
        if (sound == null || sound.clip == null || sfxSource == null) return;

        // Use the main SFX source for one-shot
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
    }

    public void PlaySFX(string clipName, float volumeScale)
    {
        if (sfxDictionary.TryGetValue(clipName, out SoundClip sound))
        {
            PlaySFX(sound, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void PlaySFX(SoundClip sound, float volumeScale)
    {
        if (sound == null || sound.clip == null || sfxSource == null) return;

        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume * volumeScale);
    }

    public void PlaySFX(string clipName, Vector3 position)
    {
        if (sfxDictionary.TryGetValue(clipName, out SoundClip sound))
        {
            PlaySFXAtPoint(sound, position);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void PlaySFXAtPoint(SoundClip sound, Vector3 position)
    {
        if (sound == null || sound.clip == null) return;
        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
    }

    public void PlaySFXAtPoint(SoundClip sound, Vector3 position, float volumeScale)
    {
        if (sound == null || sound.clip == null) return;
        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume * volumeScale);
    }

    // Pooled SFX (for overlapping sounds)
    public void PlaySFXPooled(string clipName)
    {
        if (sfxDictionary.TryGetValue(clipName, out SoundClip sound))
        {
            PlaySFXPooled(sound);
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void PlaySFXPooled(SoundClip sound)
    {
        if (sound == null || sound.clip == null) return;

        AudioSource source = GetPooledSource();
        if (source != null)
        {
            source.clip = sound.clip;
            source.volume = sound.volume * sfxVolume * masterVolume;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.gameObject.SetActive(true);
            source.Play();

            if (!sound.loop)
            {
                StartCoroutine(ReturnSourceToPool(source, sound.clip.length));
            }
        }
        else
        {
            // Fallback to regular play if pool is empty
            PlaySFX(sound);
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
            sfxSource.Stop();
    }

    public void StopAllPooledSFX()
    {
        foreach (var source in sfxPool)
        {
            if (source.isPlaying)
            {
                source.Stop();
                source.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region Pool Management
    private AudioSource GetPooledSource()
    {
        if (sfxPool.Count == 0) return null;

        AudioSource source = sfxPool.Dequeue();
        source.gameObject.SetActive(true);
        sfxPool.Enqueue(source);
        return source;
    }

    private System.Collections.IEnumerator ReturnSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
    }

    // Reset pool (call when scene changes if needed)
    public void ResetPool()
    {
        StopAllPooledSFX();
        // Ensure all sources are deactivated
        foreach (var source in sfxPool)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Coroutines
    private System.Collections.IEnumerator FadeMusic(AudioClip clip, float duration)
    {
        if (musicSource == null) yield break;

        // Fade out current music
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            musicSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        // Switch clip
        musicSource.clip = clip;
        musicSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            musicSource.volume = Mathf.Lerp(0, musicVolume * masterVolume, t);
            yield return null;
        }

        musicSource.volume = musicVolume * masterVolume;
    }

    private System.Collections.IEnumerator FadeOutMusic(float duration)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = musicVolume * masterVolume;
    }
    #endregion
}