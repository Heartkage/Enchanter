using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWizard
{
	public class Scoreboard_PlayerScore : MonoBehaviour
    {
        #region UI
        [Header("Element to set for UI")]
        [SerializeField]
        private Image playerName;
        [SerializeField]
        private Image innerBar;
        [SerializeField]
        private Text scoreText;
        #endregion

        public void SetPlayerScore(PlayerColor playerColor, float fillPercent, int score)
        {
            playerName.sprite = playerColor.image;
            innerBar.color = playerColor.color;
            innerBar.fillAmount = fillPercent;
            scoreText.text = score.ToString();
        }

    }

    
}

