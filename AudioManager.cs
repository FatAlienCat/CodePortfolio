//-----------------------------------------------------------------------------------------------------
// FileName: AudioManager.cs
// Description: Handles all Audio manipulation in game from sound effects to music.
// Author: Julian Beiboer
// Date: 07/05/2022
//-----------------------------------------------------------------------------------------------------
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HelperScripts
{
    public class AudioManager : MonoBehaviour
    {
        public bool DontDestryOnLoad = true;
        public Sound[] Sounds;
        public static AudioManager Instance;
        private string _currentTheme;
        
        
        void Awake()
        {
            if (DontDestryOnLoad)
            {

                if (Instance == null)
                {
                    Instance = this;
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
            }
            foreach (Sound s in Sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = s.mixerGroup;
            }
        }
    
        public void Play(string name)
        {
            Sound s = Array.Find(Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.theme)
            {
                Stop(_currentTheme);
                _currentTheme = s.name;
                s.source.volume = 0; // set to zero for transition
                s.source.Play();
                StartCoroutine(Fade(s.source, 0, s.volume, s.transitionTime));//fade in
            }
            else
            {
                s.source.volume = s.volume;
                s.source.Play();
            }
        }
        public void SetVolume(string name, float volume, float transistionTime)
        {
            Sound s = Array.Find(Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.theme)
            {
                float currentVolume = s.source.volume;
                s.source.volume = volume;
                StartCoroutine(Fade(s.source, currentVolume, volume, transistionTime));//fade in
            }
        
        }

        public void Stop(string name)
        {
            Sound s = Array.Find(Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            StartCoroutine(Fade(s.source, .5f, 0));//fade out
            
        }
        public void Pitch(string name, float value)
        {
            Sound s = Array.Find(Sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.pitch = value;
            
        }

        IEnumerator Fade(AudioSource audio, float startVolume, float endVolume, float fadeTimePeriod = 1f)
        {
            float timeElapsed = 0f;
            Debug.LogWarning("Sound: " + name + " Fade!");
            while (timeElapsed < fadeTimePeriod)
            {
                audio.volume = Mathf.Lerp(startVolume, endVolume, timeElapsed / fadeTimePeriod);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (audio.volume == 0)
            {
                audio.Stop();
            }
        }
    }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioMixerGroup mixerGroup;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume;
        [Range(.1f, 3f)]
        public float pitch = 1f;
        public bool loop;
        public bool theme = false;
        public float transitionTime = 1f;

        [HideInInspector]
        public AudioSource source;

    }
}
