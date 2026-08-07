using System.Collections.Generic;
using UnityEngine;

public enum GameSoundCue
{
    Jump,
    ParrySuccess,
    KickHit,
    PlayerHit,
    Warning,
    StageClear,
    GameClear
}

public sealed class GameAudioFeedback : MonoBehaviour
{
    private const int SampleRate = 22050;
    private const float RunnerMusicVolume = 0.48f;
    private const float BossMusicVolume = 0.55f;

    private static GameAudioFeedback instance;

    private readonly Dictionary<GameSoundCue, AudioClip> clips =
        new Dictionary<GameSoundCue, AudioClip>();

    private AudioSource musicSource;
    private AudioSource effectsSource;
    private AudioClip escapeMusicClip;
    private AudioClip bossMusicClip;
    private bool isBossMusic;
    private float nextWarningTime;

    public static GameAudioFeedback EnsureForScene()
    {
        if (instance != null)
        {
            return instance;
        }

        GameAudioFeedback existing =
            FindFirstObjectByType<GameAudioFeedback>(
                FindObjectsInactive.Include
            );

        if (existing != null)
        {
            instance = existing;
            return instance;
        }

        GameObject audioObject =
            new GameObject("GameAudioFeedback");

        instance = audioObject.AddComponent<GameAudioFeedback>();
        return instance;
    }

    public static void Play(GameSoundCue cue)
    {
        if (instance == null)
        {
            return;
        }

        instance.PlayCue(cue);
    }

    public static void SetBossBattle(bool bossBattle)
    {
        GameAudioFeedback audio = EnsureForScene();

        if (audio != null)
        {
            audio.SwitchMusic(bossBattle);
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = RunnerMusicVolume;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.loop = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.62f;

        CreateSoundLibrary();

        escapeMusicClip = Resources.Load<AudioClip>(
            "Audio/ObstacleRunBGM"
        );
        bossMusicClip = Resources.Load<AudioClip>(
            "Audio/BossBattleBGM"
        );

        if (escapeMusicClip == null)
        {
            escapeMusicClip = CreateEscapeMusicClip();
        }

        if (bossMusicClip == null)
        {
            bossMusicClip = CreateBossMusicClip();
        }
        musicSource.clip = escapeMusicClip;
        isBossMusic = false;
        musicSource.Play();
    }

    private void SwitchMusic(bool bossBattle)
    {
        AudioClip nextClip =
            bossBattle
                ? bossMusicClip
                : escapeMusicClip;

        if (
            nextClip == null ||
            (
                isBossMusic == bossBattle &&
                musicSource.clip == nextClip &&
                musicSource.isPlaying
            )
        )
        {
            return;
        }

        isBossMusic = bossBattle;
        musicSource.Stop();
        musicSource.clip = nextClip;
        musicSource.volume =
            bossBattle
                ? BossMusicVolume
                : RunnerMusicVolume;
        musicSource.Play();
    }

    private void PlayCue(GameSoundCue cue)
    {
        if (
            cue == GameSoundCue.Warning &&
            Time.unscaledTime < nextWarningTime
        )
        {
            return;
        }

        if (cue == GameSoundCue.Warning)
        {
            nextWarningTime = Time.unscaledTime + 0.18f;
        }

        if (!clips.TryGetValue(cue, out AudioClip clip))
        {
            return;
        }

        effectsSource.PlayOneShot(clip, GetCueVolume(cue));
    }

    private void CreateSoundLibrary()
    {
        clips[GameSoundCue.Jump] = CreateSweepClip(
            "Jump",
            0.16f,
            330f,
            690f,
            0.28f,
            false
        );

        clips[GameSoundCue.ParrySuccess] = CreateChordClip(
            "ParrySuccess",
            0.28f,
            new[] { 523.25f, 659.25f, 783.99f },
            0.34f
        );

        clips[GameSoundCue.KickHit] = CreateImpactClip(
            "KickHit",
            0.2f,
            88f,
            0.62f
        );

        clips[GameSoundCue.PlayerHit] = CreateSweepClip(
            "PlayerHit",
            0.24f,
            230f,
            92f,
            0.42f,
            true
        );

        clips[GameSoundCue.Warning] = CreatePulseClip(
            "Warning",
            0.3f,
            760f,
            0.25f
        );

        clips[GameSoundCue.StageClear] = CreateChordClip(
            "StageClear",
            0.52f,
            new[] { 392f, 523.25f, 659.25f },
            0.32f
        );

        clips[GameSoundCue.GameClear] = CreateChordClip(
            "GameClear",
            0.9f,
            new[] { 523.25f, 659.25f, 783.99f, 1046.5f },
            0.3f
        );
    }

    private static AudioClip CreateEscapeMusicClip()
    {
        const float duration = 12f;
        const float beatDuration = 0.375f;
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        float[] roots =
        {
            73.42f,
            58.27f,
            65.41f,
            55f
        };
        float[] ratios =
        {
            2f,
            2.3784f,
            2.9966f,
            2.3784f
        };

        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / SampleRate;
            int beatIndex = Mathf.FloorToInt(
                time / beatDuration
            );
            int barIndex = beatIndex / 4;
            float beatTime =
                Mathf.Repeat(time, beatDuration);
            float root = roots[
                barIndex % roots.Length
            ];

            float stringPulse =
                0.72f +
                Mathf.Sin(
                    2f * Mathf.PI * 4f * time
                ) * 0.12f;

            float lowStrings =
                (
                    Mathf.Sin(
                        2f * Mathf.PI * root * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 2f * time
                    ) * 0.32f +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 3f * time
                    ) * 0.12f
                ) * 0.055f * stringPulse;

            float eighthDuration =
                beatDuration * 0.5f;
            int eighthIndex = Mathf.FloorToInt(
                time / eighthDuration
            );
            float eighthTime =
                Mathf.Repeat(time, eighthDuration);
            float arpeggioNote =
                root * ratios[
                    eighthIndex % ratios.Length
                ];
            float arpeggioEnvelope =
                Mathf.Exp(
                    -eighthTime /
                    eighthDuration *
                    5.5f
                );
            float arpeggio =
                (
                    Mathf.Sin(
                        2f * Mathf.PI *
                        arpeggioNote * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        arpeggioNote * 2f * time
                    ) * 0.18f
                ) * 0.045f * arpeggioEnvelope;

            float kickEnvelope =
                Mathf.Exp(
                    -beatTime /
                    beatDuration *
                    18f
                );

            float kick = Mathf.Sin(
                2f * Mathf.PI *
                Mathf.Lerp(
                    105f,
                    48f,
                    beatTime / beatDuration
                ) *
                beatTime
            ) * 0.12f * kickEnvelope;

            float noise = GetNoise(index);
            bool snareBeat =
                beatIndex % 4 == 1 ||
                beatIndex % 4 == 3;
            float snare = snareBeat
                ? noise *
                    Mathf.Exp(
                        -beatTime /
                        beatDuration *
                        24f
                    ) *
                    0.075f
                : 0f;

            float hiHat =
                noise *
                Mathf.Exp(
                    -eighthTime /
                    eighthDuration *
                    45f
                ) *
                0.025f;

            samples[index] = Mathf.Clamp(
                lowStrings +
                arpeggio +
                kick +
                snare +
                hiHat,
                -0.72f,
                0.72f
            );
        }

        return CreateClip(
            "UrgentOfficeEscapeLoop",
            samples
        );
    }

    private static AudioClip CreateBossMusicClip()
    {
        const float duration = 20f;
        const float beatDuration = 0.625f;
        int sampleCount =
            Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        float[] roots =
        {
            73.42f,
            58.27f,
            49f,
            55f
        };

        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / SampleRate;
            int beatIndex =
                Mathf.FloorToInt(time / beatDuration);
            int barIndex = beatIndex / 4;
            float beatTime =
                Mathf.Repeat(time, beatDuration);
            float barTime =
                Mathf.Repeat(
                    time,
                    beatDuration * 4f
                );
            float root =
                roots[barIndex % roots.Length];

            bool majorThird =
                barIndex % roots.Length == 3;
            float thirdRatio =
                majorThird ? 1.2599f : 1.1892f;

            float swell =
                0.55f +
                Mathf.Sin(
                    Mathf.PI *
                    barTime /
                    (beatDuration * 4f)
                ) *
                0.45f;

            float brass =
                (
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 2f * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 2f *
                        thirdRatio * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 3f * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 4f * time
                    ) * 0.28f
                ) * 0.048f * swell;

            float tremolo =
                0.72f +
                Mathf.Sin(
                    2f * Mathf.PI * 7f * time
                ) *
                0.2f;

            float strings =
                (
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 4f * time
                    ) +
                    Mathf.Sin(
                        2f * Mathf.PI *
                        root * 6f * time
                    ) * 0.45f
                ) * 0.035f * tremolo;

            bool taikoBeat =
                beatIndex % 4 == 0 ||
                beatIndex % 4 == 2;
            float drumEnvelope =
                Mathf.Exp(
                    -beatTime /
                    beatDuration *
                    10f
                );
            float taiko = taikoBeat
                ? (
                    Mathf.Sin(
                        2f * Mathf.PI *
                        Mathf.Lerp(
                            82f,
                            42f,
                            beatTime / beatDuration
                        ) *
                        beatTime
                    ) *
                    0.18f +
                    GetNoise(index) *
                    0.055f
                ) * drumEnvelope
                : 0f;

            float cymbal = beatIndex % 8 == 0
                ? GetNoise(index) *
                    Mathf.Exp(
                        -beatTime /
                        beatDuration *
                        2.5f
                    ) *
                    0.035f
                : 0f;

            float bass =
                Mathf.Sin(
                    2f * Mathf.PI *
                    root * 0.5f * time
                ) *
                0.075f;

            samples[index] = Mathf.Clamp(
                brass +
                strings +
                taiko +
                cymbal +
                bass,
                -0.78f,
                0.78f
            );
        }

        return CreateClip(
            "GrandOfficeBossLoop",
            samples
        );
    }

    private static float GetNoise(int sampleIndex)
    {
        return Mathf.Sin(
            sampleIndex * 12.9898f +
            Mathf.Sin(sampleIndex * 0.017f) *
            78.233f
        );
    }

    private static AudioClip CreateSweepClip(
        string clipName,
        float duration,
        float startFrequency,
        float endFrequency,
        float amplitude,
        bool squareWave
    )
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int index = 0; index < sampleCount; index++)
        {
            float progress = (float)index / sampleCount;
            float frequency = Mathf.Lerp(
                startFrequency,
                endFrequency,
                progress
            );

            phase += 2f * Mathf.PI * frequency / SampleRate;

            float wave = squareWave
                ? Mathf.Sign(Mathf.Sin(phase))
                : Mathf.Sin(phase);

            float envelope =
                Mathf.Sin(progress * Mathf.PI) *
                (1f - progress * 0.25f);

            samples[index] = wave * envelope * amplitude;
        }

        return CreateClip(clipName, samples);
    }

    private static AudioClip CreateChordClip(
        string clipName,
        float duration,
        float[] frequencies,
        float amplitude
    )
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / SampleRate;
            float progress = (float)index / sampleCount;
            float sample = 0f;

            for (int noteIndex = 0;
                noteIndex < frequencies.Length;
                noteIndex++)
            {
                float delay = noteIndex * 0.055f;

                if (time < delay)
                {
                    continue;
                }

                sample += Mathf.Sin(
                    2f * Mathf.PI *
                    frequencies[noteIndex] *
                    (time - delay)
                );
            }

            float envelope =
                Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);

            samples[index] =
                sample /
                Mathf.Max(frequencies.Length, 1) *
                envelope *
                amplitude;
        }

        return CreateClip(clipName, samples);
    }

    private static AudioClip CreateImpactClip(
        string clipName,
        float duration,
        float frequency,
        float amplitude
    )
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / SampleRate;
            float progress = (float)index / sampleCount;
            float envelope = Mathf.Exp(-progress * 8f);
            float noise = Mathf.Sin(
                index * 12.9898f +
                Mathf.Sin(index * 0.17f) * 41.3f
            );

            float body = Mathf.Sin(
                2f * Mathf.PI * frequency * time
            );

            samples[index] =
                (body * 0.72f + noise * 0.28f) *
                envelope *
                amplitude;
        }

        return CreateClip(clipName, samples);
    }

    private static AudioClip CreatePulseClip(
        string clipName,
        float duration,
        float frequency,
        float amplitude
    )
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = (float)index / SampleRate;
            float progress = (float)index / sampleCount;
            float pulse = Mathf.Repeat(time, 0.12f) < 0.065f
                ? 1f
                : 0f;

            float envelope =
                Mathf.Sin(progress * Mathf.PI);

            samples[index] =
                Mathf.Sin(2f * Mathf.PI * frequency * time) *
                pulse *
                envelope *
                amplitude;
        }

        return CreateClip(clipName, samples);
    }

    private static AudioClip CreateClip(
        string clipName,
        float[] samples
    )
    {
        AudioClip clip = AudioClip.Create(
            clipName,
            samples.Length,
            1,
            SampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }

    private static float GetCueVolume(GameSoundCue cue)
    {
        switch (cue)
        {
            case GameSoundCue.KickHit:
                return 0.92f;
            case GameSoundCue.PlayerHit:
                return 0.78f;
            case GameSoundCue.Warning:
                return 0.48f;
            case GameSoundCue.StageClear:
            case GameSoundCue.GameClear:
                return 0.82f;
            default:
                return 0.68f;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
