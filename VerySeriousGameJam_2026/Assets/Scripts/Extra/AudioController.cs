using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    public float crossFadeTime = 0.5f;

    [Header("SFX")]
    public AudioSource sfxSource;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("UI")]
    public AudioClip uiClick;
    public AudioClip uiHover;

    [Header("Gameplay SFX")]
    public AudioClip attackSfx;
    public AudioClip hitSfx;
    public AudioClip healSfx;
    public AudioClip pickupSfx;
    public AudioClip dashSfx;
    public AudioClip killSfx;
    public AudioClip deathSfx;
    public AudioClip powerupSfx;

    [Header("Optional")]
    public bool autoSwitchBySceneName = true;
    public string menuSceneName = "MainMenu";
    public string gameplaySceneName = "GameScene";

    Coroutine _musicRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
    }

    void Start()
    {
        if (autoSwitchBySceneName)
            SwitchMusicForCurrentScene();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoSwitchBySceneName)
            SwitchMusicForCurrentScene(scene.name);
    }

    void SwitchMusicForCurrentScene()
    {
        SwitchMusicForCurrentScene(SceneManager.GetActiveScene().name);
    }

    void SwitchMusicForCurrentScene(string sceneName)
    {
        if (sceneName == menuSceneName)
            PlayMusic(menuMusic);
        else
            PlayMusic(gameplayMusic);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayUIClick()
    {
        PlaySfx(uiClick);
    }

    public void PlayUIHover()
    {
        PlaySfx(uiHover);
    }

    public void PlayAttack()
    {
        PlaySfx(attackSfx);
    }

    public void PlayHit()
    {
        PlaySfx(hitSfx);
    }

    public void PlayHeal()
    {
        PlaySfx(healSfx);
    }

    public void PlayPickup()
    {
        PlaySfx(pickupSfx);
    }

    public void PlayDash()
    {
        PlaySfx(dashSfx);
    }

    public void PlayKill()
    {
        PlaySfx(killSfx);
    }

    public void PlayDeath()
    {
        PlaySfx(deathSfx);
    }

    public void PlayPowerup()
    {
        PlaySfx(powerupSfx);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (_musicRoutine != null)
            StopCoroutine(_musicRoutine);

        _musicRoutine = StartCoroutine(CrossFadeMusic(clip));
    }

    IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        if (musicSource == null)
            yield break;

        float startVolume = musicSource.volume;

        // Fade out
        float t = 0f;
        while (t < crossFadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / crossFadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        t = 0f;
        while (t < crossFadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / crossFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
