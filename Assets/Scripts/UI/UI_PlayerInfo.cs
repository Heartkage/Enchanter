using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWizard
{
    public class UI_PlayerInfo : MonoBehaviour
    {
        [SerializeField]
        private Text playerType;
        [SerializeField]
        private Text playerName;
        [SerializeField]
        private Image playerImage;
        [SerializeField]
        private Sprite selfSprite;
        [SerializeField]
        private Sprite none;
        [SerializeField]
        private Color normalColor;
        [SerializeField]
        private Color readyColor;

        public void SetPlayerInfo(bool isHost, string name, bool isSelf)
        {
            playerType.text = (isHost) ? "Host" : "Client";
            playerName.text = name;
            playerImage.sprite = (isSelf) ? selfSprite : none;
        }
        
        public void SetBackgroundColor(bool isReady)
        {
            GetComponent<Image>().color = (isReady) ? readyColor : normalColor;
        }

    }
}


