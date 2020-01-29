using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

namespace GameWizard
{
	public class InGameInfoUIManager : MonoBehaviour {
		
        [Header("Parameter")]
		public SteamVR_TrackedController leftController;
		public Canvas UICanvas;
		public Image playerIDImage;
		public GameObject[] skillNameObjects;
		public Vector2 outlineEffectDistance;
		public CoolDownRenderer[] skillCoolDowns;

		private PhotonView _photonView;
		private PlayerColor _playerColor;
        private PlayerStatus _playerStatus;
		private PlayerSkill _playerSkill;

        void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if (!_photonView.IsMine)
            {
				UICanvas.gameObject.SetActive(false);
				Destroy(this);
            }
        }

		void Start () {
			_playerColor = Photon_GameSetting.instance.playerColor[Photon_Room.instance.GetMyPlayerID()];
            _playerStatus = GetComponent<PlayerStatus>();
			_playerSkill = GetComponent<PlayerSkill>();
			playerIDImage.sprite = _playerColor.image;

			if(GameManager_Main.instance.debugFlag)
				UICanvas.transform.localPosition = UICanvas.transform.localPosition + new Vector3(0,1f,0);
			
			SetSkillName();
		}
		void Update () {
			if(!GameManager_Main.instance.debugFlag)
            {
                if(GameManager_Main.instance.hasVREnvironment && (leftController != null))
                    UICanvas.gameObject.SetActive(leftController.triggerPressed && GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive());
                else
                    UICanvas.gameObject.SetActive(Input.GetKey(KeyCode.Q) && GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive());
            }	
			else
				UICanvas.gameObject.SetActive(Input.GetKey(KeyCode.Q) && GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive());
		}


	 	void SetSkillName()
		{
			int len = _playerSkill.skills.Length;
			for(int i=0;i<len;++i)
			{
				skillNameObjects[i].GetComponent<CurvedText>().text = _playerSkill.skills[i].skillName;
				Outline _outline = skillNameObjects[i].GetComponents<Outline>()[1];
				_outline.effectColor = _playerSkill.skills[i].color;
				_outline.effectDistance = outlineEffectDistance;
			}
		}

		public void CoolDownBegin(int skillIndex, float skillCD)
		{
			skillCoolDowns[skillIndex].CoolDownBegin(skillCD);
		}
		public void coolDownEnd(int skillIndex)
		{
			skillCoolDowns[skillIndex].CoolDownEnd();
		}
		public void CoolDowning(int skillIndex, float CDTime, float currentTime)
		{
			if(skillIndex<0 || skillIndex>= skillCoolDowns.Length) return;
			if(skillCoolDowns[skillIndex] == null)
			{
				Debug.LogError("Skill "+skillIndex+" UI component missing");
				return;
			}
			
			skillCoolDowns[skillIndex].CoolDownRender(CDTime, currentTime);
		}
	}
}