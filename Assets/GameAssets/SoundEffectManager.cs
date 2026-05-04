using UnityEngine;
using System.Collections.Generic;

public class SoundEffectManager : MonoBehaviour
{
    [System.Serializable]
    public struct SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 2f)] public float volume;
    }

    public static SoundEffectManager Instance { get; private set; }

    public List<SoundEffect> soundEffectList = new List<SoundEffect>();
    Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

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

    public void AddAudioSource(string name)
    {
        if (!audioSources.ContainsKey(name))
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            audioSources[name] = newSource;
        }
    }

    bool IsSoundEffectExists(string name)
    {
        return soundEffectList.Exists(s => s.name == name);
    }

    SoundEffect GetSoundEffect(string name)
    {
        return soundEffectList.Find(s => s.name == name);
    }

    public void AddSoundEffect(string name, AudioClip clip)
    {
        if (!IsSoundEffectExists(name))
        {
            soundEffectList.Add(new SoundEffect { name = name, clip = clip, volume = 1.0f });
        }
        else
        {
            Debug.LogWarning($"Sound effect '{name}' already exists.");
        }
    }

    bool IsAudioSourceExists(string name)
    {
        return audioSources.ContainsKey(name);
    }

    AudioSource GetAudioSource(string name)
    {
        if (IsAudioSourceExists(name))
        {
            return audioSources[name];
        }
        else
        {
            Debug.LogWarning($"Audio source '{name}' not found.");
            return null;
        }
    }

    public void PlaySoundEffect(string name, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!IsAudioSourceExists(name))
        {
            AddAudioSource(name);
        }

        if (IsSoundEffectExists(name) && IsAudioSourceExists(name))
        {
            audioSources[name].pitch = pitch;
            audioSources[name].PlayOneShot(GetSoundEffect(name).clip, GetSoundEffect(name).volume * volume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{name}' or audio source '{name}' not found.");
        }
    }

    public void PlaySoundEffectRandomPitch(string name, float volume = 1.0f, float pitch = 1.0f, float deltaPitch = 0.2f)
    {
        PlaySoundEffect(name, volume, pitch + Random.Range(-deltaPitch, deltaPitch));
    }
}
