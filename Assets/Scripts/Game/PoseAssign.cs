/*
 * Coder: Yuansyun Ye
 * Date: 2019/06/04
 * Description: 從SteamVR抓取Pose, 再用Photon傳送出去，讓VRIK可以同步姿勢。
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace GameWizard{
public class PoseAssign : MonoBehaviour {

	public Transform target;
	private PhotonView photonView;

	// Use this for initialization
	void Start () {
		photonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {
		if(photonView.IsMine && target != null){
			transform.position = target.position;
			transform.rotation = target.rotation;
		}
	}
}
}