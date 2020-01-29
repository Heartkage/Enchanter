using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWizard
{
	public class Photon_GameSetting : MonoBehaviour
    {
        [Header("System Setting")]
        public ScenesIndex scenesIndex;
        public Sprite[] drawing_Images;
        public BaseSkill[] allKindsOfSpell;

        [Header("Skill Videos")]
        public GameObject quad;
        public GameObject audioPlayer;
        public GameObject[] skillVideos;

        [Header("Game Setting")]
        [Range(10, 600)]
        public int oneGameTimeInSecond = 10;
        public float maxHealth;
        public float startMovementSpeed;
        public int maxPlayer;
        public Transform[] playerSpawningInfo;
        public PlayerColor[] playerColor;

        #region Singleton
        public static Photon_GameSetting instance;
        void Awake()
        {
            if (Photon_GameSetting.instance == null)
            {
                Photon_GameSetting.instance = this;
            }
        }
        #endregion

        #region GetFunction
        public void EnableVideo(int index)
        {
            audioPlayer.SetActive(true);
            quad.SetActive(true);
            skillVideos[index].SetActive(true);
        }

        public void DisableVideo(int index)
        {
            quad.SetActive(false);
            audioPlayer.SetActive(false);
            skillVideos[index].SetActive(false);
        }

        #endregion
	}


    [System.Serializable]
    public class ScenesIndex
    {
        public int menu;
        public int firstScene;
    }

    [System.Serializable]
    public class PlayerColor
    {
        public Color color;
        public Sprite image;
    }
}

