using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class GameSfxPlayer : MonoBehaviour
{
    public static GameSfxPlayer Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Pickup")]
    [SerializeField] private AudioClip bottleCollectClip;

    [Header("Hazards")]
    [SerializeField] private AudioClip carHitClip;
    [SerializeField] private AudioClip ratHitClip;
    [SerializeField] private AudioClip[] deathClips = new AudioClip[2];
    [SerializeField] private float hitSfxCooldown = 0.1f;
    [SerializeField] private float deathSfxCooldown = 0.1f;

    [Header("Passing")]
    [SerializeField] private AudioClip carsPassingClip;
    [SerializeField] private AudioClip ratsPassingClip;
    [SerializeField] private float passingSfxCooldown = 0.35f;

    [Header("Player")]
    [SerializeField] private AudioClip footStepsClip;
    [SerializeField] private AudioClip guyRollingClip;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.45f;
    [Range(0f, 1f)]
    [SerializeField] private float passingVolume = 0.7f;

    private float lastCarsPassingTime = -999f;
    private float lastRatsPassingTime = -999f;
    private float lastCarHitTime = -999f;
    private float lastRatHitTime = -999f;
    private float lastDeathTime = -999f;

    private void Reset()
    {
        oneShotSource = GetComponent<AudioSource>();

#if UNITY_EDITOR
        AutoFillClipsFromSoundPack();
#endif
    }

    private void OnValidate()
    {
        passingSfxCooldown = Mathf.Max(0f, passingSfxCooldown);
        hitSfxCooldown = Mathf.Max(0f, hitSfxCooldown);
        deathSfxCooldown = Mathf.Max(0f, deathSfxCooldown);

#if UNITY_EDITOR
        AutoFillClipsFromSoundPack();
#endif
    }

    private void Awake()
    {
        Instance = this;
        EnsureAudioSources();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void PlayBottleCollectSfx()
    {
        Instance?.PlayOneShot(Instance.bottleCollectClip);
    }

    public static void PlayCarHitSfx()
    {
        if (Instance == null || Time.time - Instance.lastCarHitTime < Instance.hitSfxCooldown)
        {
            return;
        }

        Instance.lastCarHitTime = Time.time;
        Instance?.PlayOneShot(Instance.carHitClip);
    }

    public static void PlayRatHitSfx()
    {
        if (Instance == null || Time.time - Instance.lastRatHitTime < Instance.hitSfxCooldown)
        {
            return;
        }

        Instance.lastRatHitTime = Time.time;
        Instance?.PlayOneShot(Instance.ratHitClip);
    }

    public static void PlayDeathSfx()
    {
        if (Instance == null || Time.time - Instance.lastDeathTime < Instance.deathSfxCooldown)
        {
            return;
        }

        Instance.lastDeathTime = Time.time;
        AudioClip deathClip = Instance.GetRandomDeathClip();
        Instance.PlayOneShot(deathClip);
    }

    public static void PlayCarsPassingSfx()
    {
        if (Instance == null || Time.time - Instance.lastCarsPassingTime < Instance.passingSfxCooldown)
        {
            return;
        }

        Instance.lastCarsPassingTime = Time.time;
        Instance.PlayOneShot(Instance.carsPassingClip, Instance.passingVolume);
    }

    public static void PlayRatsPassingSfx()
    {
        if (Instance == null || Time.time - Instance.lastRatsPassingTime < Instance.passingSfxCooldown)
        {
            return;
        }

        Instance.lastRatsPassingTime = Time.time;
        Instance.PlayOneShot(Instance.ratsPassingClip, Instance.passingVolume);
    }

    public static void PlayGuyRollingSfx()
    {
        Instance?.PlayOneShot(Instance.guyRollingClip);
    }

    public static void SetFootstepsPlaying(bool shouldPlay)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.SetFootsteps(shouldPlay);
    }

    public static void StopFootstepsSfx()
    {
        SetFootstepsPlaying(false);
    }

    private void EnsureAudioSources()
    {
        if (oneShotSource == null)
        {
            oneShotSource = GetComponent<AudioSource>();
        }

        if (oneShotSource == null)
        {
            oneShotSource = gameObject.AddComponent<AudioSource>();
        }

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
        }

        oneShotSource.playOnAwake = false;
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.volume = footstepVolume * masterVolume;
    }

    private AudioClip GetRandomDeathClip()
    {
        if (deathClips == null || deathClips.Length == 0)
        {
            return null;
        }

        int assignedClipCount = 0;

        for (int i = 0; i < deathClips.Length; i++)
        {
            if (deathClips[i] != null)
            {
                assignedClipCount++;
            }
        }

        if (assignedClipCount == 0)
        {
            return null;
        }

        int selectedClipIndex = Random.Range(0, assignedClipCount);

        for (int i = 0; i < deathClips.Length; i++)
        {
            if (deathClips[i] == null)
            {
                continue;
            }

            if (selectedClipIndex == 0)
            {
                return deathClips[i];
            }

            selectedClipIndex--;
        }

        return null;
    }

    private void PlayOneShot(AudioClip clip)
    {
        PlayOneShot(clip, 1f);
    }

    private void PlayOneShot(AudioClip clip, float volumeScale)
    {
        if (clip == null || oneShotSource == null)
        {
            return;
        }

        oneShotSource.PlayOneShot(clip, masterVolume * volumeScale);
    }

    private void SetFootsteps(bool shouldPlay)
    {
        if (footStepsClip == null || footstepSource == null)
        {
            return;
        }

        footstepSource.volume = footstepVolume * masterVolume;

        if (shouldPlay)
        {
            if (footstepSource.clip != footStepsClip)
            {
                footstepSource.clip = footStepsClip;
            }

            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

#if UNITY_EDITOR
    private void AutoFillClipsFromSoundPack()
    {
        bottleCollectClip = bottleCollectClip != null ? bottleCollectClip : LoadClip("Assets/Sound Pack/SFXs bottle collect.mp3");
        carHitClip = carHitClip != null ? carHitClip : LoadClip("Assets/Sound Pack/SFXs CarHit.mp3");
        ratHitClip = ratHitClip != null ? ratHitClip : LoadClip("Assets/Sound Pack/SFXs RatHit.mp3");
        carsPassingClip = carsPassingClip != null ? carsPassingClip : LoadClip("Assets/Sound Pack/SFXs CarsPassing.mp3");
        ratsPassingClip = ratsPassingClip != null ? ratsPassingClip : LoadClip("Assets/Sound Pack/SFXs RatsPassing.mp3");
        footStepsClip = footStepsClip != null ? footStepsClip : LoadClip("Assets/Sound Pack/SFXs FootSteps.mp3");
        guyRollingClip = guyRollingClip != null ? guyRollingClip : LoadClip("Assets/Sound Pack/SFXs GuyRolling.mp3");

        if (deathClips == null || deathClips.Length < 2)
        {
            deathClips = new AudioClip[2];
        }

        deathClips[0] = deathClips[0] != null ? deathClips[0] : LoadClip("Assets/Sound Pack/SFXs death1.mp3");
        deathClips[1] = deathClips[1] != null ? deathClips[1] : LoadClip("Assets/Sound Pack/SFXs death2.mp3");
    }

    private AudioClip LoadClip(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    }
#endif
}
