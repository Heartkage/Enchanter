using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameWizard
{
    public class UI_VR_Mode : MonoBehaviour
    {
      
        private bool isVRMode = false;

        public Camera viewing_Camera;

        void Start()
        {
            viewing_Camera.stereoTargetEye = (isVRMode) ? StereoTargetEyeMask.Both : StereoTargetEyeMask.None;
        }

        



    }
}

