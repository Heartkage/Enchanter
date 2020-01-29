/*
 * Coder: Yuansyun Ye
 * Date: 2019/06/04
 * Description: 將血量朝向本機端玩家
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWizard{
public class LookAtPlayer : MonoBehaviour {

	public Transform lookatTarget;
	private string targetName = "Camera (eye)";

	// Use this for initialization
	void Start () {
        GameObject players = GameObject.Find(targetName);
        if (players != null)
        {
            lookatTarget = players.transform;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if(lookatTarget!=null)
        {
			transform.LookAt(lookatTarget);
                //transform.position = new Vector3(lookatTarget.position.x, transform.localPosition.y, lookatTarget.position.z);
        }

	}
}
}

