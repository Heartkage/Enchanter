using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
	public class Photon_Room : MonoBehaviourPunCallbacks, IInRoomCallbacks
    {
        #region Singleton
        public static Photon_Room instance;
        void Awake()
        {
            if (Photon_Room.instance == null)
                Photon_Room.instance = this;
            else
            {
                Destroy(Photon_Room.instance.gameObject);
                if (Photon_Room.instance != this)
                {
                    Destroy(Photon_Room.instance.gameObject);
                    Photon_Room.instance = this;
                }
            }
            DontDestroyOnLoad(this.gameObject);

            panel_Room.SetActive(false);
            
            newUI_Panel.SetActive(true);
            
        }
        #endregion

        #region For Parallel Debug
        [HideInInspector]
        public bool autoReady = false;
        public GameObject newUI_Panel;
        #endregion

        #region UI Variables
        [SerializeField]
        private string playerPrefabName;
        [SerializeField]
        private GameObject panel_Room;
        [SerializeField]
        private Text room_Name;
        [SerializeField]
        private Button start_Button;
        [SerializeField]
        private Button ready_Button;
        [SerializeField]
        private Button leave_Button;

        public Transform playerList_Parent;
        public GameObject player_UI_Prefab;
        List<GameObject> playerUI_List;

        [System.Serializable]
        public struct SpellUI
        {
            public Image drawing_Image;
            public Dropdown dropDown;
            public Text damage;
            public Text duration;
            public Text CD;
        }
        public SpellUI[] spell_UI;

        List<string> allSpellNames;
        int[] spellSelection;

        #endregion

        #region Room Panel Variables
        private PhotonView photonView;
        Player[] playersInRoom;
        //-- 1~4 --//
        int myPlayerID;
        string myUserID;
        bool imReady = false;
        int readyPlayerCount;
        #endregion

        #region Game Related Variables
        [HideInInspector]
        public int currentSceneIndex;
        bool isGameLoaded;
        int numOfPlayersLoaded;
        int numOfPlayersSpawned;


        #endregion

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
        }

        void SetInitialReferences()
        {
            photonView = GetComponent<PhotonView>();
            allSpellNames = new List<string>();
            spellSelection = new int[spell_UI.Length];
            foreach (BaseSkill s in Photon_GameSetting.instance.allKindsOfSpell)
                allSpellNames.Add(s.skillName);

            for (int i = 0; i < Photon_GameSetting.instance.drawing_Images.Length; i++)      
                spell_UI[i].drawing_Image.sprite = Photon_GameSetting.instance.drawing_Images[i];

            playerUI_List = new List<GameObject>();
            for (int i = 0; i < Photon_GameSetting.instance.maxPlayer; i++)
            {
                GameObject go = GameObject.Instantiate(player_UI_Prefab, playerList_Parent);
                go.SetActive(false);
                playerUI_List.Add(go);
            }
            start_Button.interactable = true;

            imReady = false;
            UpdateReadyUI();
        }

        void Start()
        {
            SetInitialReferences();
        }

        #region For Buttons
        public void StartGame()
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ConfirmBtnSound);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            start_Button.interactable = false;
            PhotonNetwork.LoadLevel(Photon_GameSetting.instance.scenesIndex.firstScene);
        }

        public void ClickedReady()
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ClickBtnSound);
            imReady = !imReady;
            photonView.RPC("BroadcastReadyUI", RpcTarget.All, imReady, myPlayerID-1);
            UpdateReadyUI();
        }

        public void LeaveRoom()
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ConfirmBtnSound);
            PhotonNetwork.LeaveRoom();
            imReady = false;
            UpdateReadyUI();
            panel_Room.SetActive(false);
            isGameLoaded = false;
            Photon_Lobby.instance.LobbyToRoom(false);
        }

        #endregion

        #region In Room Related Functions

        [PunRPC]
        private void BroadcastReadyUI(bool isReady, int playerIndex)
        {
            if (isReady)
                readyPlayerCount++;
            else
                readyPlayerCount--;
            
            playerUI_List[playerIndex].GetComponent<UI_PlayerInfo>().SetBackgroundColor(isReady);

            if (PhotonNetwork.IsMasterClient)
            {
                if ((readyPlayerCount + 1) == playersInRoom.Length)
                    start_Button.interactable = true;
                else
                    start_Button.interactable = false;
            }
        }

        void Room_ResetInitialValue()
        {
            imReady = false;
            isGameLoaded = false;
            numOfPlayersLoaded = 0;
            numOfPlayersSpawned = 0;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            SetSpell_UI_Event();
            Debug.Log("You joined a room!");
            Photon_Lobby.instance.LobbyToRoom(true);

            playersInRoom = PhotonNetwork.PlayerList;
            myUserID = playersInRoom[playersInRoom.Length - 1].UserId;
            PlayerID_Check();
            PhotonNetwork.NickName = string.Concat("Player ", myPlayerID.ToString());
            Room_ResetInitialValue();


            RoomUpdate();
            SetupRoomUI();
            //if (autoReady && !PhotonNetwork.IsMasterClient && !imReady)
            //ClickedReady();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            Debug.Log("A new player has entered the room.");
            playersInRoom = PhotonNetwork.PlayerList;
            RoomUpdate();
            SetupRoomUI();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log(otherPlayer.NickName + " left the room.");
            base.OnPlayerLeftRoom(otherPlayer);

            playersInRoom = PhotonNetwork.PlayerList;
            PlayerID_Check();
            PhotonNetwork.NickName = string.Concat("Player ", myPlayerID.ToString());

            if (!isGameLoaded)
            {
                RoomUpdate();
                SetupRoomUI();
            }
        }


        void PlayerID_Check()
        {
            for (int i = 0; i < playersInRoom.Length; i++)
            {
                if (playersInRoom[i].UserId == myUserID)
                    myPlayerID = i + 1;
            }
        }

        //--Close Room if room is current full--//
        void RoomUpdate()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (playersInRoom.Length == Photon_GameSetting.instance.maxPlayer)
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                else
                    PhotonNetwork.CurrentRoom.IsOpen = true;
            }
        }

        //--Set up when player enter or leave room--//
        void SetupRoomUI()
        {
            if (playersInRoom.Length > 0)
            {
                panel_Room.SetActive(true);
                room_Name.text = PhotonNetwork.CurrentRoom.Name;

                //--Set Player Name--//
                for (int i = 0; i < Photon_GameSetting.instance.maxPlayer; i++)
                {
                    if (i < playersInRoom.Length)
                    {
                        playerUI_List[i].SetActive(true);
                        UI_PlayerInfo info = playerUI_List[i].GetComponent<UI_PlayerInfo>();
                        info.SetPlayerInfo((i == 0) ? true : false, string.Concat("Player ", (i+1).ToString()), ((i + 1) == myPlayerID) ? true : false);
                        info.SetBackgroundColor(false);
                    }
                    else
                        playerUI_List[i].SetActive(false);
                }


                //--Set Button--//
                if (PhotonNetwork.IsMasterClient)
                {
                    ready_Button.gameObject.SetActive(false);
                    start_Button.gameObject.SetActive(true);
                    if (playersInRoom.Length == 1)
                        start_Button.interactable = true;
                    else
                        start_Button.interactable = false;
                }   
                else
                {
                    start_Button.gameObject.SetActive(false);
                    ready_Button.gameObject.SetActive(true);
                }

                //--Set Ready Button--//
                imReady = false;
                readyPlayerCount = 0;
                UpdateReadyUI();
            }                
        }

        void SetSpell_UI_Event()
        {
            int counter = 0;
            foreach (SpellUI UI_list in spell_UI)
            {
                //---Clear---//
                UI_list.dropDown.ClearOptions();
                UI_list.dropDown.onValueChanged.RemoveAllListeners();
                //---Refresh---//
                UI_list.dropDown.AddOptions(allSpellNames);
                UI_list.dropDown.value = counter;
                UI_list.dropDown.onValueChanged.AddListener(delegate { UpdateOtherSpell_UI(UI_list.dropDown); });
                UI_list.damage.text = Photon_GameSetting.instance.allKindsOfSpell[counter].damage.ToString();
                UI_list.duration.text = string.Concat(Photon_GameSetting.instance.allKindsOfSpell[counter].existingDuration.ToString(), "s");
                UI_list.CD.text = string.Concat(Photon_GameSetting.instance.allKindsOfSpell[counter].coolDownTime.ToString(), "s");
                spellSelection[counter] = counter;
                
                counter++;
            }
        }

        void UpdateOtherSpell_UI(Dropdown next)
        {
            int index = 0;
            int oldValue = 0;

            Sound_Manager.instance.PlayShortSound(SoundIndex.ClickBtnSound);

            for (int i = 0; i < spell_UI.Length; i++)
            {
                if (spell_UI[i].dropDown == next)
                {
                    index = i;
                    oldValue = spellSelection[i];
                }
            }          

            for (int i = 0; i < spell_UI.Length; i++)
            {
                if (index == i)
                    continue;

                if (spellSelection[i] == next.value)
                {
                    spell_UI[i].dropDown.value = oldValue;
                    spellSelection[i] = oldValue;
                }   
            }
            spellSelection[index] = next.value;

            spell_UI[index].damage.text = Photon_GameSetting.instance.allKindsOfSpell[next.value].damage.ToString();
            spell_UI[index].duration.text = string.Concat(Photon_GameSetting.instance.allKindsOfSpell[next.value].existingDuration.ToString(), "s");
            spell_UI[index].CD.text = string.Concat(Photon_GameSetting.instance.allKindsOfSpell[next.value].coolDownTime.ToString(), "s");
        }

        void UpdateReadyUI()
        {
            ready_Button.gameObject.GetComponent<Image>().color = (imReady) ? Color.green : Color.white;
            foreach (SpellUI s in spell_UI)
            {
                s.dropDown.interactable = (imReady) ? false : true;
            }
        }

        #endregion

        #region Load Scene Related Functions
        void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            currentSceneIndex = scene.buildIndex;
            numOfPlayersLoaded = 0;
            numOfPlayersSpawned = 0;

            if (currentSceneIndex == Photon_GameSetting.instance.scenesIndex.firstScene)
            {
                photonView.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
                isGameLoaded = true;
            }
            else
            {
                Room_ResetInitialValue();
            }
        }

        [PunRPC]
        private void RPC_LoadedGameScene()
        {
            numOfPlayersLoaded++;
            Debug.Log("[MasterClient] Current number of players loaded to scene:" + numOfPlayersLoaded);
            if (numOfPlayersLoaded == PhotonNetwork.PlayerList.Length)
            {
                Debug.Log("[MasterClient] All players are loaded, spawning players");
                photonView.RPC("RPC_SpawnPlayer", RpcTarget.All, numOfPlayersLoaded);
            }
        }
            
        [PunRPC]
        private void RPC_SpawnPlayer(int totalPlayerAmount)
        {
            Vector3 position = Photon_GameSetting.instance.playerSpawningInfo[myPlayerID-1].position;
            Quaternion q = Photon_GameSetting.instance.playerSpawningInfo[myPlayerID-1].rotation;
            GameObject p = PhotonNetwork.Instantiate(Path.Combine("InGamePrefabs", playerPrefabName), position, q, 0);

            p.GetComponent<PlayerStatus>().SetPlayerID(myPlayerID - 1);
            PlayerSkill playerSkill = p.GetComponent<PlayerSkill>();
            
            for(int i=0;i<spell_UI.Length;i++)
            {
                playerSkill.skills[i] = Photon_GameSetting.instance.allKindsOfSpell[spellSelection[i]];
                playerSkill.canCastSkill[i] = true;
            }

            Debug.Log("[Player" + myPlayerID + "] spawned");
            GameManager_Main.instance.playerAmountInGame = totalPlayerAmount;

            photonView.RPC("RPC_ReadyToStart", RpcTarget.MasterClient);
        }

        [PunRPC]
        private void RPC_ReadyToStart()
        {
            numOfPlayersSpawned++;
            if (numOfPlayersSpawned == PhotonNetwork.PlayerList.Length)
            {
                Debug.Log("[MasterClient] All players are ready, starting the game");
                GameManager_Main.instance.StartingGame(numOfPlayersSpawned);
            }
        }

        public void BackToMainMenu()
        {
            photonView.RPC("LeaveRoomAndLobby", RpcTarget.All);
        }

        [PunRPC]
        private void LeaveRoomAndLobby()
        {
            Debug.Log("Leaving room and lobby...");
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LeaveLobby();
            StartCoroutine(WaitForLeaveLobby());
        }

        IEnumerator WaitForLeaveLobby()
        {
            while (PhotonNetwork.IsConnectedAndReady)
            {
                yield return new WaitForSeconds(0.5f);
            }
            SceneManager.LoadScene(Photon_GameSetting.instance.scenesIndex.menu);
            Destroy(this.gameObject);
            
        }

        #endregion


        #region In Game Related Function Call
        // Returns ID of 0 to maximum player-1
        public int GetMyPlayerID()
        {
            return myPlayerID-1;
        }

        public void AddScoreOnDamage(int id, int point)
        {
            photonView.RPC("UpdateScoreBoard", RpcTarget.All, id, point);
        }

        [PunRPC]
        private void UpdateScoreBoard(int id, int point)
        {
            GameManager_Main.instance.UpdateScore(id, point * 10);
        }

        #endregion
    }

}

