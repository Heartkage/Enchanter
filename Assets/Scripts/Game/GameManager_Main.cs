using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameWizard
{
    [RequireComponent(typeof(Photon_GameSetting))]
	public class GameManager_Main : MonoBehaviour {

		#region Singleton
		public static GameManager_Main instance;
		void Awake()
		{
            if (GameManager_Main.instance == null)
                GameManager_Main.instance = this;
            else
            {
                if (GameManager_Main.instance != this)
                    Destroy(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);
            

            int totalPlayerNum = GetComponent<Photon_GameSetting>().maxPlayer;
            scoreBoard = new int[totalPlayerNum];
            resultPercent = new float[totalPlayerNum];
        }
		#endregion

        [Header("Debug Switch")]
        public bool debugFlag = false;
        public bool hasVREnvironment = true;
        public bool allowKeyboardInput = true;
        public bool isBuiltGame = false;

        [Header("Game Parameters----Read Only!")]
        public bool startGame;
        public bool endGame;
        public int playerAmountInGame;

		public delegate void GameGeneralEvent();
		public event GameGeneralEvent GameSetup;
        public event GameGeneralEvent GameEnd;

        int[] scoreBoard;
        float[] resultPercent;


        // Variable for server connection
        public UdpSocket socket;
        System.Diagnostics.Process pythonServer = new System.Diagnostics.Process();  // Auto Start Python Server Process
        String startPath;  // The Unity Application Starting Path

        // Start UDP Socket
        void Start() {
            if (hasVREnvironment)
            {   
                // // Start Python Server Automatically
                // startPath = Application.dataPath;
                // String path = startPath + "/Gesture/Scripts/StartServer.bat";  //Resource.load
                // pythonServer.StartInfo.FileName = path;  // Initial pythonServer Path
                
                // // Set the window off
                // pythonServer.StartInfo.UseShellExecute = true;
                // pythonServer.StartInfo.CreateNoWindow = true;
                // pythonServer.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                // pythonServer.Start();  // Start Python Socket      
                    
                pythonServer.StartInfo.FileName = "CMD.exe";
                pythonServer.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                if (isBuiltGame)
                    pythonServer.StartInfo.Arguments = "/C python PythonSocket/Server.py";
                else
                    pythonServer.StartInfo.Arguments = "/C python Assets/Gesture/Scripts/Server.py";
                pythonServer.Start();

                // Start Unity Server
                Debug.Log("Socket Initial");
                socket = new UdpSocket();
                socket.InitSocket();
            }

        }

        // End UDP Socket
        void OnDestroy() {
            if (hasVREnvironment)
            {
                if(socket != null)
                {
                    Debug.Log("Socket Closed");
                    socket.SocketSend("CloseSocket");
                    socket.SocketQuit();
                }
            }
        }

        //--Only Master Client will call this--//
		public void StartingGame(int numOfPlayers)
		{
            endGame = false;
            
            if (GameSetup != null)
            {
                GameSetup();
            }
		}

        public void GameStarted()
        {
            int totalPlayerNum = GetComponent<Photon_GameSetting>().maxPlayer;
            for (int i = 0; i < totalPlayerNum; i++)
                scoreBoard[i] = 0;
            startGame = true;
        }

        public void EndGame()
        {
            startGame = false;
            endGame = true;
            if (GameEnd != null)
                GameEnd();
        }

        public void UpdateScore(int playerID, int score)
        {
            scoreBoard[playerID] += score;
            if (scoreBoard[playerID] < 0)
                scoreBoard[playerID] = 0;
        }

        public void CalculateScoreResult()
        {
            int currentHighest = 0;
            for (int i = 0; i < playerAmountInGame; i++)
            {
                if (scoreBoard[i] > currentHighest)
                    currentHighest = scoreBoard[i];
            }
            for (int i = 0; i < playerAmountInGame; i++)
            {
                if (currentHighest == 0)
                    resultPercent[i] = 0;
                else
                    resultPercent[i] = (float)((float)scoreBoard[i] / (float)currentHighest);
            }
        }

        public float GetPercent(int playerID)
        {
            return resultPercent[playerID];
        }

        public int GetScore(int playerID)
        {
            return scoreBoard[playerID];
        }


	}

}
