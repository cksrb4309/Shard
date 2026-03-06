using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    static SoundManager instance = null;

    public AudioSource[] backgroundAudios;
    public AudioSource effectAudio;

    [HideInInspector] public float bgmVolume = 1.0f;     // 브금 소리 크기
    [HideInInspector] public float sfxVolume = 1.0f;     // 효과음 소리 크기
    [HideInInspector] public float allVolume = 1.0f;     // 전체 소리 크기

    public Sound[] soundArray;

    public AudioClip[] monsterDieSounds;
    public AudioClip[] blockDestroySounds;
    public AudioClip[] blockAttackSounds;
    public AudioClip[] sheildAttackSounds;


    Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();

    bool isBackgroundSoundPlaying = false;
    int currentBackgroundIndex = 0;

    public static void SheildAttackSoundPlay()
    {
        instance.effectAudio.PlayOneShot(instance.sheildAttackSounds[Random.Range(0, 3)]);
    }
    public static void BlockAttackSoundPlay()
    {
        instance.effectAudio.PlayOneShot(instance.blockAttackSounds[Random.Range(0, 3)]);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            allVolume = PlayerPrefs.GetFloat("ALL_Volume", 0.5f);
            bgmVolume = PlayerPrefs.GetFloat("BGM_Volume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 0.5f);

            SetAllVolume(allVolume);
            SetBGM(bgmVolume);
            SetSFX(sfxVolume);

            for (int i = 0; i < soundArray.Length; i++)
                clipDictionary.Add(soundArray[i].clipName, soundArray[i].audioClip);

            PlayBackgroundMusic("Title");

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        PlayerPrefs.SetFloat("ALL_Volume", allVolume);
        PlayerPrefs.SetFloat("BGM_Volume", bgmVolume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBackgroundMusic(scene.name);
    }
    public void PlayBackgroundMusic(string sceneName)
    {
        string clipName = "";
        // 씬 이름에 따라 배경음악 선택
        switch (sceneName)
        {
            case "Title": clipName = "TitleBGM"; break;
            case "Game": clipName = "BattleBGM"; break;
            default:
                Debug.LogWarning("해당 씬에 맞는 배경음악이 설정되지 않았습니다.");
                return;
        }

        Play(clipName, SoundType.Background);
    }
    public static void Play(string clipName, SoundType type = SoundType.Effect)
    {
        if (type == SoundType.Effect)
        {
            instance.EffectPlay(clipName);
        }
        else
        {
            instance.BackgroundPlay(clipName);
        }
    }
    public static void SetBGMVolume(float volume)
    {
        instance.SetBGM(volume);
    }
    void SetBGM(float volume)
    {
        bgmVolume = volume;

        backgroundAudios[0].volume = bgmVolume * allVolume;
        backgroundAudios[1].volume = bgmVolume * allVolume;
    }
    public static void SetSFXVolume(float volume)
    {
        instance.SetSFX(volume);
    }
    void SetSFX(float volume)
    {
        sfxVolume = volume;

        effectAudio.volume = sfxVolume * allVolume;
    }
    public static void SetAllVolume(float volume)
    {
        instance.allVolume = volume;

        instance.effectAudio.volume = instance.sfxVolume * instance.allVolume;

        instance.backgroundAudios[0].volume = instance.bgmVolume * instance.allVolume;
        instance.backgroundAudios[1].volume = instance.bgmVolume * instance.allVolume;
    }
    void EffectPlay(string clipName)
    {
        if (clipName == string.Empty)
        {
            Debug.Log("Audio가 비어있습니다");
        }
        else if (clipDictionary.ContainsKey(clipName))
        {
            effectAudio.PlayOneShot(clipDictionary[clipName]);
        }
        else
        {
            Debug.LogWarning("clipName:" + clipName + "과(와) 일치하는 Sound가 없습니다");
        }
    }
    void BackgroundPlay(string clipName)
    {
        if (clipName == string.Empty)
        {
            Debug.Log("Audio가 비어있습니다");
        }
        else if (clipDictionary.ContainsKey(clipName))
        {
            if (isBackgroundSoundPlaying)
            {
                currentBackgroundIndex ^= 1;
                backgroundAudios[currentBackgroundIndex].clip = clipDictionary[clipName];

                StartCoroutine(BackgroundAudioFadeCoroutine());
            }
            else
            {
                isBackgroundSoundPlaying = true;

                backgroundAudios[currentBackgroundIndex].clip = clipDictionary[clipName];
                backgroundAudios[currentBackgroundIndex].Play();
            }
        }
        else
        {
            Debug.LogWarning("clipName:" + clipName + "과(와) 일치하는 Sound가 없습니다");
        }
    }
    IEnumerator BackgroundAudioFadeCoroutine()
    {
        float t = 0f;

        backgroundAudios[currentBackgroundIndex].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * 0.3f;

            backgroundAudios[currentBackgroundIndex ^ 1].volume = Mathf.Lerp(bgmVolume, 0, t);
            backgroundAudios[currentBackgroundIndex].volume = Mathf.Lerp(0, bgmVolume, t);

            yield return null;
        }
        backgroundAudios[currentBackgroundIndex ^ 1].Stop();
    }
    public static void MonsterDiePlay()
    {
        instance.effectAudio.PlayOneShot(instance.monsterDieSounds[Random.Range(0, instance.monsterDieSounds.Length)]);
    }
    public static float GetBGMVolume()
    {
        return instance.bgmVolume;
    }
    public static float GetSFXVolume()
    {
        return instance.sfxVolume;
    }
    public static float GetALLVolume()
    {
        return instance.allVolume;
    }
}

[System.Serializable]
public struct Sound
{
    public string clipName;
    public AudioClip audioClip;
}
public enum SoundType
{
    Background,
    Effect
}