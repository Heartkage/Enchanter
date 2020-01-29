using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sigtrap.VrTunnellingPro;

namespace GameWizard{
	public class TunnellingSwitch : MonoBehaviour {
		Tunnelling tunnelling;
		// Use this for initialization
		void Awake () {
			tunnelling = GetComponent<Tunnelling>();
            
			if (!GameManager_Main.instance.hasVREnvironment)
			{
                Destroy(tunnelling);
			}
			else
			{
				tunnelling.enabled = true;
			}
		}	
	}
}
