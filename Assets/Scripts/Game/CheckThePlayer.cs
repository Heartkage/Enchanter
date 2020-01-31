/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/15
 * Description: 檢查該玩家是不是自己，如果不是則把此玩家的Steam VR刪除
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using RootMotion.FinalIK;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
    public class CheckThePlayer : MonoBehaviour
    {

        public GameObject PlayerCamera;
        public bool thisPlayerIsLocal = true;

        private PhotonView photonView;

        // Use this for initialization
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            if (!photonView.IsMine)
            {
                thisPlayerIsLocal = false;
                if(PlayerCamera != null) Destroy(PlayerCamera);
            }
        }

    }
}

