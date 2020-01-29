using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWizard
{
	public class DontDestroy : MonoBehaviour {

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
	}
}

