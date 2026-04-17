using System;
using GCat_Test.Core.DataModel;
using GCFramework.Singleton;
using UnityEngine;
using UnityEngine.Audio;

namespace GCat_Test.Core.Manager
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        public AudioMixer audioMixer;
        private AudioSource _bgmAudioSource;
        private AudioSource _sfxAudioSource;

        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SOUND_EFFECT_VOLUME_KEY = "SoundEffectVolume";
        public float MusicVolume { get; private set; }
        public float SoundEffectVolume { get; private set; }

        protected override void Awake()
        {
            base.Awake();
           
            _bgmAudioSource = transform.Find("Audio/Bgm").GetComponent<AudioSource>();
            _sfxAudioSource = transform.Find("Audio/Sfx").GetComponent<AudioSource>();
        }

        private void Start()
        {
            InitState();
        }

        private void InitState()
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
            SoundEffectVolume = PlayerPrefs.GetFloat(SOUND_EFFECT_VOLUME_KEY, 0.7f);
            SetMusicVolume(MusicVolume);
            SetSoundEffectVolume(SoundEffectVolume);
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = value;   
            audioMixer.SetFloat("Music", TodBVolume(value));
        }

        public void SetSoundEffectVolume(float value)
        {
            SoundEffectVolume = value; 
            audioMixer.SetFloat("SoundEffect", TodBVolume(value));
        }

        public void PlayBgm(string bgmTag)
        {
            var clip = GlobalAudioDataModel.Instance.GetClip(bgmTag);
            if (clip == null)
                return;

            _bgmAudioSource.clip = clip;
        }

        public void PlaySfx(string sfxTag)
        {
            var clip = GlobalAudioDataModel.Instance.GetClip(sfxTag);
            if (clip == null)
                return;
            
            _sfxAudioSource.PlayOneShot(clip);
        }

        private float TodBVolume(float value)
        {
            return (value * 100f) - 80;
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
            PlayerPrefs.SetFloat(SOUND_EFFECT_VOLUME_KEY, SoundEffectVolume);
        }
    }
}