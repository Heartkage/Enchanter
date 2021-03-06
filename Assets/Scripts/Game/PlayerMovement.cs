﻿/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/16
 * Description: 控制腳色移動，VR透過左手Controller, PC透過鍵盤
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Photon.Pun;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView)), RequireComponent(typeof(PlayerStatus))]
    public class PlayerMovement : MonoBehaviour
    {

        private bool _hasVREnvironment;

        public Transform playerCamera;
        public SteamVR_TrackedController leftController;
        [Range(10, 100)]
        public float keyboardRotationSpeed = 10f;
        float yRotation;

        private float touchpadThreshold = 0.25f;
        private PhotonView _photonView;
        private PlayerStatus _playerStatus;
        private Rigidbody _myRigi;
        private BoxCollider _collider;
        private Vector3 movement;

        // Use this for initialization
        void Start()
        {
            _hasVREnvironment = GameManager_Main.instance.hasVREnvironment;

            _playerStatus = GetComponent<PlayerStatus>();
            _photonView = GetComponent<PhotonView>();

            _myRigi = GetComponent<Rigidbody>();
            _collider = GetComponent<BoxCollider>();
            
            if (!_hasVREnvironment && GameManager_Main.instance.allowKeyboardInput)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            

            if (!_photonView.IsMine)
            {
                Destroy(this);
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive())
            {
                movement = Vector3.zero;
               
                if (GameManager_Main.instance.allowKeyboardInput)
                {
                    //--KeyBoard Mode--//
                    if (Input.GetKey(KeyCode.W))
                    {
                        movement = playerCamera.forward;
                    }
                    else if (Input.GetKey(KeyCode.S))
                    {
                        movement = -1 * playerCamera.forward;
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        movement += playerCamera.right;
                        //transform.Rotate(Vector3.up, Time.deltaTime * keyboardRotationSpeed);
                    }
                    else if (Input.GetKey(KeyCode.A))
                    {
                        movement += -1 * playerCamera.right;
                        //transform.Rotate(Vector3.up, Time.deltaTime * -keyboardRotationSpeed);
                    }

                    CameraRotation();
                    Movement(movement, _playerStatus.CurrentMovementSpeed());

                }

                if (_hasVREnvironment)
                {
                    if (leftController != null)
                    {
                        VRControllerState_t controllerState = leftController.controllerState;
                        ulong pad = controllerState.ulButtonTouched & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad));

                        if (pad > 0L)
                        {
                            movement = Vector3.zero;
                            bool singleDirection = false;
                            if (controllerState.rAxis0.x > touchpadThreshold)
                            {
                                movement = playerCamera.right;
                                singleDirection = !singleDirection;
                                //Debug.Log("right");
                            }
                            else if (controllerState.rAxis0.x < -touchpadThreshold)
                            {
                                movement = -1 * playerCamera.right;
                                singleDirection = !singleDirection;
                                //Debug.Log("left");
                            }

                            if (controllerState.rAxis0.y > touchpadThreshold)
                            {
                                movement += playerCamera.forward;
                                singleDirection = !singleDirection;
                                //Debug.Log("forward");
                            }
                            else if (controllerState.rAxis0.y < -touchpadThreshold)
                            {
                                movement -= playerCamera.forward;
                                singleDirection = !singleDirection;
                                //Debug.Log("down");
                            }

                            if (!singleDirection)
                                movement /= 2;

                            Movement(movement, _playerStatus.CurrentMovementSpeed());
                        }
                    }
                }

                

                // keep the collider follow the HMD
                _collider.center = new Vector3(playerCamera.localPosition.x, _collider.size.y/2, playerCamera.localPosition.z);

            }
        }


        void CameraRotation()
        {
            float mouseX = Input.GetAxis("Mouse X") * keyboardRotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * keyboardRotationSpeed * Time.deltaTime;

            yRotation -= mouseY;
            yRotation = Mathf.Clamp(yRotation, -80f, 80f);

            playerCamera.localRotation = Quaternion.Euler(yRotation, 0, 0);
            transform.Rotate(Vector3.up * mouseX);

        }

        void Movement(Vector3 direction, float speed)
        {


            Vector3 d = new Vector3(direction.x, 0, direction.z);
            //Debug.DrawRay(playerCamera.position, d * speed, Color.red);
            Vector3 f = d * speed * Time.deltaTime;

            if (_hasVREnvironment)
                _myRigi.AddForce(f, ForceMode.Impulse);
            else
            {
                _myRigi.velocity = new Vector3(d.x*speed/2f, _myRigi.velocity.y, d.z*speed/2f);
            }
            //Debug.Log("d:" + direction + ", speed" + speed);
        }
    }
}

