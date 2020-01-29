/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/16
 * Description: 刪除打擊特效
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
    public class HitEffect : MonoBehaviour
    {

        public float destroyTime;

        private PhotonView _photonView;

        // Use this for initialization
        void Start()
        {
            _photonView = GetComponent<PhotonView>();
            if (_photonView.IsMine)
            {
                StartCoroutine(RunDestroyTimer());
            }
        }

        IEnumerator RunDestroyTimer()
        {
            yield return new WaitForSeconds(destroyTime);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
