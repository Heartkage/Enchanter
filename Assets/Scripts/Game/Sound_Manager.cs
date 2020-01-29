using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameWizard
{
	public class Sound_Manager : MonoBehaviour
    {

        #region Singleton and Initialize
        public static Sound_Manager instance;
        public Audio[] allSounds;

        void Awake()
        {
            if (Sound_Manager.instance == null)
            {
                instance = this;
                InitializeAllAudio(); 
            }
            else 
            {
                if (Sound_Manager.instance != this)
                    Destroy(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
        }

        void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == Photon_GameSetting.instance.scenesIndex.menu)
            {
                for(int i=0; i < allSounds.Length; i++)
                {
                    if (allSounds[i].type == SoundType.LoopBGM)
                    {
                        allSounds[i].source.Stop();
                        if (allSounds[i].name == SoundIndex.MainBGM)
                            allSounds[i].source.Play();   
                    }
                }
            }
            else if (scene.buildIndex == Photon_GameSetting.instance.scenesIndex.firstScene)
            {
                for (int i = 0; i < allSounds.Length; i++)
                {
                    if (allSounds[i].type == SoundType.LoopBGM)
                    {
                        allSounds[i].source.Stop();
                        if (allSounds[i].name == SoundIndex.BattleBGM)
                            allSounds[i].source.Play();
                    }
                }
            }
        }

        void InitializeAllAudio()
        {
            foreach (Audio s in allSounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.playOnAwake = false;
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.loop = (s.shouldLoop) ? true : false;
            }
            
        }

        public void PlayShortSound(SoundIndex index)
        {
            for (int i = 0; i < allSounds.Length; i++)
            {
                if (allSounds[i].type == SoundType.PlayOnce)
                {
                    if (allSounds[i].name == index)
                    {
                        //Debug.Log("Playing: " + allSounds[i].nameForInspector);
                        //if(!allSounds[i].source.isPlaying)
                        allSounds[i].source.Play();
                    }
                        
                }
            }
        }

        public void PlayLoopSound(SoundIndex index)
        {
            for (int i = 0; i < allSounds.Length; i++)
            {
                if (allSounds[i].name == index)
                {
                    if(!allSounds[i].source.isPlaying)
                        allSounds[i].source.Play();
                } 
            }
        }

        public void StopLoopSound(SoundIndex index)
        {
            for (int i = 0; i < allSounds.Length; i++)
            {
                if (allSounds[i].name == index)
                {
                    if(allSounds[i].source.isPlaying)
                    {
                        allSounds[i].source.Pause();
                        allSounds[i].source.Stop(); 
                    }
                        
                } 
            }
        }
    }


    [System.Serializable]
    public enum SoundIndex
    {
        MainBGM = 0,
        BattleBGM = 1,
        ConfirmBtnSound = 2,
        ClickBtnSound = 3,
        Casting = 4,
        Damage = 5,
        Death = 6,
        Ghost = 7,
        Revive = 8,
        CastFail = 9
    }
    [System.Serializable]
    public enum SoundType
    {
        LoopBGM = 0,
        PlayOnce = 1
    }

    [System.Serializable]
    public class Audio
    {
        public string nameForInspector;
        public SoundIndex name;
        public SoundType type;
        public AudioClip clip;
        [Range(0, 5)]
        public float volume = 1;
        public bool shouldLoop;
        [HideInInspector]
        public AudioSource source;
    }
}

