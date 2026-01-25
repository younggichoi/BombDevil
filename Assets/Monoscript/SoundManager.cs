using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource loopSource; 
    public AudioSource loopSource2; // Second source for crossfading

    [Header("Audio Clips")]
    public AudioClip boomClip;
    public AudioClip clickClip;

    [Space(10)]
    public AudioClip startRealBombClip;
    public AudioClip loopRealBombClip;

    [Header("Settings")]
    [Range(0.1f, 3.0f)]
    public float fadeDuration = 0.261f; // Overlap duration

    private Coroutine realBombRoutine;
    private float initialLoopVolume;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loopSource == null)
                loopSource = gameObject.AddComponent<AudioSource>();
            
            if (loopSource2 == null)
                loopSource2 = gameObject.AddComponent<AudioSource>();
            
            initialLoopVolume = loopSource.volume;
            loopSource2.volume = initialLoopVolume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ==========================================
    //  RealBomb Sound Logic
    // ==========================================

    public void PlayRealBombSound()
    {
        StopRealBombSound();
        realBombRoutine = StartCoroutine(PlayRealBombSequence());
    }

    public void StopRealBombSound()
    {
        if (realBombRoutine != null)
        {
            StopCoroutine(realBombRoutine);
            realBombRoutine = null;
        }

        loopSource.Stop();
        loopSource.clip = null;
        
        loopSource2.Stop();
        loopSource2.clip = null;
    }

    private IEnumerator PlayRealBombSequence()
    {
        // 1. Play Start Sound using loopSource (so it can be stopped)
        if (startRealBombClip != null)
        {
            loopSource.clip = startRealBombClip;
            loopSource.loop = false;
            loopSource.Play();

            float waitTime = startRealBombClip.length - fadeDuration;
            if (waitTime < 0) waitTime = 0;

            yield return new WaitForSeconds(waitTime);
        }

        // 2. Play Loop Sound with Crossfading (Ping-Pong)
        if (loopRealBombClip != null)
        {
            // Start the first loop on loopSource2 because loopSource might still be fading out/playing the tail of start sound
            bool useSource1 = false; 
            float loopWaitTime = loopRealBombClip.length - fadeDuration;
            if (loopWaitTime < 0) loopWaitTime = 0;

            while (true)
            {
                AudioSource currentSource = useSource1 ? loopSource : loopSource2;
                
                currentSource.clip = loopRealBombClip;
                currentSource.loop = false; // We manually loop
                currentSource.Play();

                yield return new WaitForSeconds(loopWaitTime);

                useSource1 = !useSource1;
            }
        }
    }
}