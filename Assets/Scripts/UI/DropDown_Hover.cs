using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameWizard
{
	public class DropDown_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
        public GameObject playerList_UI;
		string _skillName;
		public void OnPointerEnter(PointerEventData eventData)
		{
            _skillName = this.name.Split(new char[2]{':', ' '})[1];
			Photon_GameSetting.instance.EnableVideo(int.Parse(_skillName));
            if(playerList_UI != null)
                playerList_UI.SetActive(false);
        }
		public void OnPointerExit(PointerEventData eventData)
		{
            if (playerList_UI != null)
                playerList_UI.SetActive(true);
            _skillName = this.name.Split(new char[2]{':', ' '})[1];
			Photon_GameSetting.instance.DisableVideo(int.Parse(_skillName));
		}
		public void OnPointerClick(PointerEventData pointerEventData)
		{
            if (playerList_UI != null)
                playerList_UI.SetActive(true);
			_skillName = this.name.Split(new char[2]{':', ' '})[1];
			Photon_GameSetting.instance.DisableVideo(int.Parse(_skillName));
		}
	}
}
