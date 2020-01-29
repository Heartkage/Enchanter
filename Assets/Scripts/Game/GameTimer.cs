using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Photon.Pun;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
	public class GameTimer : MonoBehaviour 
    {
        public Text timerText;

        Color redColor;
        Color greenColor;

        bool startedGameCounter;
        bool startedRevivedCounter;
        int _leftOverTime;

        PhotonView _photonView;
        PlayerStatus _playerStatus;
        void Awake()
        {
            redColor = new Color(1, 26f / 255f, 26f / 255f, 1);
            greenColor = new Color(81f / 255f, 246f / 255f, 81f / 255f, 1);
            _photonView = GetComponent<PhotonView>();
            _playerStatus = GetComponent<PlayerStatus>();
        }

        void OnEnable()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Player OnEnable called");
                GameManager_Main.instance.GameSetup += StartGameCounter;
            }
        }

        void OnDisable()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Player OnDisable called");
                GameManager_Main.instance.GameSetup -= StartGameCounter;
            }
        }

        void Update()
        {
            if (GameManager_Main.instance.startGame)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (!startedGameCounter)
                    {
                        //Debug.Log("Counter Called");
                        StartCoroutine(GameTimerCountDown());
                        startedGameCounter = true;
                    }
                }

                if (!_playerStatus.IsPlayerAlive() && _photonView.IsMine)
                {
                    if (!startedRevivedCounter)
                    {
                        startedRevivedCounter = true;
                        StartCoroutine(ReviveCountDown());
                    }
                }
            }
        }

        void StartGameCounter()
        {
            startedGameCounter = false;
            startedRevivedCounter = false;
            StartCoroutine(StartTimerCountDown());
        }

        string SetUpTimerFormat(int time)
        {
            int minute = time / 60;
            int second = time % 60;
            string text = "";
            if (minute < 10)
                if(second < 10)
                    text = string.Concat(text, "0", minute.ToString(), ":0", second.ToString());
                else
                    text = string.Concat(text, "0", minute.ToString(), ":", second.ToString());
            else
                if (second < 10)
                    text = string.Concat(text, minute.ToString(), ":0", second.ToString());
                else
                    text = string.Concat(text, minute.ToString(), ":", second.ToString());
            return text;
        }

        IEnumerator ReviveCountDown()
        {
            int counter = 3;
            SetTimerText("You are dead", 1);
            yield return new WaitForSeconds(1.5f);
            while (counter > 0)
            {
                SetTimerText(string.Concat("Reviving in ", counter.ToString(), " second"), 1);
                counter--;
                yield return new WaitForSeconds(1f);
            }
            SetTimerText("Start~", 1);
            _playerStatus.RevivePlayer();
            yield return new WaitForSeconds(0.2f);
            startedRevivedCounter = false;
        }

        IEnumerator StartTimerCountDown()
        {
            int counter = 3;
            _photonView.RPC("UpdateTimerText", RpcTarget.All, "[Game] Ready", 1);
            yield return new WaitForSeconds(2f);
            while (counter > 0)
            {
                _photonView.RPC("UpdateTimerText", RpcTarget.All, string.Concat("Start in ", counter.ToString()), 0);
                yield return new WaitForSeconds(1f);
                counter--;
            }
            _photonView.RPC("UpdateTimerText", RpcTarget.All, "[Game] Start!", 1);
            yield return new WaitForSeconds(0.5f);
            _photonView.RPC("UpdateGameManager", RpcTarget.All, true);
            _leftOverTime = Photon_GameSetting.instance.oneGameTimeInSecond;
        }

        IEnumerator GameTimerCountDown()
        {
            while (_leftOverTime > 0)
            {
                if (_leftOverTime < 10)
                    _photonView.RPC("UpdateTimerText", RpcTarget.All, SetUpTimerFormat(_leftOverTime), 0);
                else
                    _photonView.RPC("UpdateTimerText", RpcTarget.All, SetUpTimerFormat(_leftOverTime), 1);

                yield return new WaitForSeconds(1f);
                _leftOverTime--;
            }
            _photonView.RPC("UpdateTimerText", RpcTarget.All, "Game End", 1);
            _photonView.RPC("UpdateGameManager", RpcTarget.All, false);
        }



        void SetTimerText(string text, int colorType)
        {
            if(colorType == 0)
                timerText.color = redColor;
            else
                timerText.color = greenColor;
            timerText.text = text;
        }

        #region Pun Functions

        [PunRPC]
        private void UpdateGameManager(bool isStarted)
        {
            if (isStarted)
                GameManager_Main.instance.GameStarted();
            else
                GameManager_Main.instance.EndGame();
            
        }

        // 0 = redColor, 1 = greenColor //
        [PunRPC]
        private void UpdateTimerText(string text, int colorType)
        {
            if (_photonView.IsMine && _playerStatus.IsPlayerAlive())
                SetTimerText(text, colorType);
        }
        #endregion



    }
}

