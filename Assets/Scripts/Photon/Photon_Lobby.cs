using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace GameWizard
{
	public class Photon_Lobby : MonoBehaviourPunCallbacks
    {

        public Button[] mainButtons;
        public int maxRoomCount = 10000;
        private int randomNum;
        private string roomName;
        private bool[] roomNumberInUsed;

        private Animator anime;

        [SerializeField]
        private UI_ConfirmPanel comfirmPanel;

        #region Singleton
        public static Photon_Lobby instance;
        void Awake()
        {
            instance = this;
            ToggleBtn(false);
            anime = GetComponent<Animator>();
        }
        #endregion


        void Start ()
		{
            SetRoomInfo();
            Debug.Log("Connecting to Photon Server...");
            ConnectToPhoton();   
		}
        void SetRoomInfo()
        {
            roomNumberInUsed = new bool[maxRoomCount];
            for (int i = 0; i < maxRoomCount; i++)
                roomNumberInUsed[i] = false;
        }

        #region ConnectToPhotonServer
        void ConnectToPhoton()
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("You have connected to Photon Master Server!");
            PhotonNetwork.AutomaticallySyncScene = true;
            ToggleBtn(true);
        }

        #endregion

        #region Creating or Joining Room
        public void JoinButtonClicked()
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ConfirmBtnSound);
            PhotonNetwork.JoinRandomRoom();
            StartCoroutine(ToggleBtnCountDown());
        }
        public void CreateButtonClicked()
        {
            comfirmPanel.OpenPanel(1);
        }
        public void JoinFriendButtonClicked()
        {
            comfirmPanel.OpenPanel(2);
        }

        public void QuitGameButtonClicked()
        {
            Debug.Log("Quitting Game~");
            Sound_Manager.instance.PlayShortSound(SoundIndex.ConfirmBtnSound);
            Application.Quit();
        }

        public void JoinRoom(string name)
        {
            PhotonNetwork.JoinRoom(name);
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            Debug.Log(returnCode + ": " + message);
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);
            Debug.Log(returnCode + ": " + message);

            Debug.Log("Creating a new room...");
            CreateNewRoom(GenerateNewName());
        }
        public void CreateNewRoom(string name)
        {
            StartCoroutine(ToggleBtnCountDown());
            RoomOptions roomOption = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)Photon_GameSetting.instance.maxPlayer, PublishUserId = true};
            roomName = name;
            if (string.Compare(name, "") == 0)
                roomName = GenerateNewName();
            
            PhotonNetwork.CreateRoom(roomName, roomOption);
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log(message);
            roomNumberInUsed[randomNum] = true;

            CreateNewRoom(GenerateNewName());
        }
        #endregion
 
        #region Animation
        public void LobbyToRoom(bool shouldOpen)
        {
            anime.SetBool("OpenRoom", shouldOpen);
        }

        public void LobbyToComfirm(bool shouldOpen)
        {
            anime.SetBool("OpenPanel", shouldOpen);
        }
        #endregion

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            roomNumberInUsed[randomNum] = true;
            Debug.Log("Room Created!");
        }

        #region API For Lobby

        public void ToggleBtn(bool shouldEnable)
        {
            for (int i = 0; i < mainButtons.Length; i++)
                mainButtons[i].interactable = shouldEnable;
        }

        IEnumerator ToggleBtnCountDown()
        {
            ToggleBtn(false);
            yield return new WaitForSeconds(1f);
            ToggleBtn(true);
        }

        string GenerateNewName()
        {
            do
            {
                randomNum = Random.Range(0, maxRoomCount - 1);
            } while (roomNumberInUsed[randomNum]);

            return string.Concat("Room #" + randomNum);
        }
        #endregion
    }
}

