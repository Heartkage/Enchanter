using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using Valve.VR;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace GameWizard{
    public class GestureCollecting : MonoBehaviour
    {
        public SteamVR_TrackedController controller;
        public SteamVR_Controller tmp;
        public GameObject head;

        PlayerSkill _playerSkill;
        PlayerStatus _playerStatus;
        Photon.Pun.PhotonView _photonView;
        bool _isInDebugMode;
        GameObject currentObjectTransform;


        private bool PressedFlag = false;
        private bool canCall = false;

        

        StringBuilder sb = new StringBuilder();

        // Variable for data index, data position
        private int DataIdx = 0;


        [Header("For Training")]
        // Variable for file name
        public bool collecting;
        public string DataType;
        public string user;
        public int csvNum = 1;


        // Variable for server connection
        UdpSocket socket;
        System.Diagnostics.Process pythonServer = null;  // Auto Start Python Server Process
        String startPath;  // The Unity Application Starting Path

        private void Awake()
        {
            _photonView = GetComponent<Photon.Pun.PhotonView>();
            _playerSkill = GetComponent<PlayerSkill>();
            _playerStatus = GetComponent<PlayerStatus>();

            if (!GetComponent<Photon.Pun.PhotonView>().IsMine)
                Destroy(this);
        }


        void Pressed(object sender, ClickedEventArgs e)
        {
            currentObjectTransform = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentObjectTransform.transform.parent = head.transform;
            currentObjectTransform.transform.localPosition = Vector3.zero;
            currentObjectTransform.transform.rotation = head.transform.rotation;
            currentObjectTransform.GetComponent<MeshRenderer>().enabled = false;
            currentObjectTransform.GetComponent<SphereCollider>().enabled = false;
            //currentObjectTransform.transform.parent = null;
        }

        void Released(object sender, ClickedEventArgs e)
        {
            Destroy(currentObjectTransform);
        }

        void OnDisable()
        {
            controller.TriggerClicked -= Pressed;
            controller.TriggerUnclicked -= Released;
        }

        void Start()
        {
            
            controller.TriggerClicked += Pressed;
            controller.TriggerUnclicked += Released;

			_isInDebugMode = GameManager_Main.instance.debugFlag;  // DebugFlag
            socket = GameManager_Main.instance.socket;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive() && _photonView.IsMine)
            {
                // Not pressed yet, and detected pressed
                if (!PressedFlag && controller.triggerPressed)
                {
                    PressedFlag = true;
                    _playerSkill.IsCasting();
                    if (collecting)
                    {
                        string[] initial_List = new string[] { "Index", "X", "Y", "Z" };
                        sb.AppendLine(string.Join(",", initial_List));
                    }
                }

                // If the controller is pressed, record the location of tracker
                if (controller.triggerPressed)
                {
                    Vector3 img_coord = currentObjectTransform.transform.InverseTransformPoint(controller.transform.position.x, controller.transform.position.y, controller.transform.position.z);
                    string[] posList = new string[] { DataIdx.ToString(), img_coord.x.ToString(), img_coord.y.ToString(), img_coord.z.ToString() };
                    if (!_isInDebugMode)
                    {
                        //Debug.Log(posList);
                        //Debug.Log(socket);
                        socket.SocketSend(posList[1] + "," + posList[2]);
                    }

                    if (collecting)
                    {
                        sb.AppendLine(string.Join(",", posList));
                        DataIdx += 1;
                    }

                }
                // If the triggered is not pressed
                if (PressedFlag && !controller.triggerPressed)
                {
                    PressedFlag = false;
                    if (!_isInDebugMode)
                    {
                        socket.SocketSend("MessageSent");
                        canCall = true;
                    }


                    if (collecting)
                    {
                        DirectoryInfo di = Directory.CreateDirectory("Assets\\Gesture\\Data\\csv\\" + DataType + "\\" + user);

                        Debug.Log("csv num " + csvNum + " Done");
                        string filename = csvNum + ".csv";
                        string filePath = "Assets\\Gesture\\Data\\csv\\" + DataType + "\\" + user + "\\" + filename;
                        StreamWriter outStream = new StreamWriter(filePath);
                        outStream.WriteLine(sb);
                        outStream.Close();


                        sb = new StringBuilder();
                        DataIdx = 0;
                        csvNum += 1;
                    }
                }

                if (canCall && (socket.recvStr != ""))
                {
                    //Debug.Log(socket.recvStr);
                    _playerSkill.CastingSpell(int.Parse(socket.recvStr));
                    socket.recvStr = "";
                    canCall = false;
                }
            } 
        }

        public void ControllerVibrate()
        {
            SteamVR_Controller.Input((int)controller.controllerIndex).TriggerHapticPulse(2000);
        }
    }
}