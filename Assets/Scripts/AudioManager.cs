// ============================================================
//  AudioManager.cs  —  Centralized audio playback system
// ============================================================
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // Assigned via Bootstrap
    private AudioClip clipGameStart;
    private AudioClip clipFruitDrop;
    private AudioClip clipFruitMerge;
    private AudioClip clipFruitCollide;
    private AudioClip clipGameOver;
    private AudioClip clipButtonClick;

    private float lastCollideTime = -1f;

    public void Initialize(
        AudioClip start, AudioClip drop, AudioClip merge, 
        AudioClip collide, AudioClip over, AudioClip btn, AudioClip bgm)
    {
        Instance = this;
        
        // Setup BGM source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f; // Default volume for BGM

        // Setup SFX source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        clipGameStart = start;
        clipFruitDrop = drop;
        clipFruitMerge = merge;
        clipFruitCollide = collide;
        clipGameOver = over;
        clipButtonClick = btn;

        if (bgm != null)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
        }
    }

    public void PlayGameStart() => PlaySFX(clipGameStart, 0.8f);
    public void PlayDrop()      => PlaySFX(clipFruitDrop, 0.7f);
    public void PlayMerge()     => PlaySFX(clipFruitMerge, 1.0f);
    public void PlayGameOver()  => PlaySFX(clipGameOver, 1.0f);
    public void PlayButtonClick() => PlaySFX(clipButtonClick, 0.9f);

    public void PlayCollide(float volumeScale = 1.0f)
    {
        // Rate-limit collisions to prevent audio spam when fruits settle
        if (Time.time - lastCollideTime > 0.05f)
        {
            PlaySFX(clipFruitCollide, volumeScale * 0.5f);
            long now = System.DateTime.Now.Ticks;
            lastCollideTime = Time.time;
        }
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
