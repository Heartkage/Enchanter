using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWizard
{
	public class CoolDownRenderer : MonoBehaviour {
		[Header("Reference")]
		public Image coolDownMask;
		public Image blockImage;
		public Text countDownText;

		[Header("For Testing")]
		public bool startTest;
		public bool endTest;
		public bool  renderTest;
		public float skillCDTime = 5f;
		public float currentTimer = 5f;

		void Start()
		{
			coolDownMask.gameObject.SetActive(false);
			blockImage.gameObject.SetActive(false);
			countDownText.gameObject.SetActive(false);
		}

		void Update()
		{
			if(startTest) {CoolDownBegin(skillCDTime);startTest=false;}
			if(endTest) {CoolDownEnd();endTest=false;}
			if(renderTest) {CoolDownRender(skillCDTime, currentTimer);}
		}

		public void CoolDownBegin(float skillCD)
		{
			blockImage.gameObject.SetActive(true);
			countDownText.text = skillCD.ToString("#0.0");
			countDownText.gameObject.SetActive(true);
			coolDownMask.fillAmount = 1;
			coolDownMask.gameObject.SetActive(true);
		}
		public void CoolDownEnd()
		{
			blockImage.gameObject.SetActive(false);
			countDownText.gameObject.SetActive(false);
			coolDownMask.gameObject.SetActive(false);
		}
		public void CoolDownRender(float CDTime, float currentTime)
		{
			//TODO : throw Exception?
			//for fixing input error
			currentTime = Mathf.Min(currentTime, CDTime);
			currentTime = Mathf.Max(currentTime, 0);

			float leftTime = CDTime - currentTime;
			coolDownMask.fillAmount = leftTime/CDTime;
			countDownText.text = leftTime.ToString("#0.0");
		}

	}
}