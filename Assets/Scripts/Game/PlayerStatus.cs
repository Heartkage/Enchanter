/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/15
 * Description: 更新玩家的血量(需繼承IPunObservable)
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Photon.Pun;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
	public class PlayerStatus : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Player UI")]
        public Image playerNameSprite;
        public GameObject scoreBoard;
        public GameObject restart;
        public GameObject backToRoom;
        public Transform scoreboardParent;
        public GameObject playerBlockPrefab;
        public ParticleSystem deathFire;

        [Header("Player Self HP UI")]
        public Slider myselfHpSlider;
        public Slider myselfHpInnerSlider;
        public Image self_InnerSliderImage;
        public Image self_fillBackImage;
        public Image self_fillFrontImage;
        Color self_InitialFillBack;
        Color self_InitialFillFront;

        [Header("Player HP UI For Other")]
        public Slider hpSlider;
        public Slider hpInnerSlider;
        public Image innerSliderImage;
        public Image fillBackImage;
        public Image fillFrontImage;
        [Range(1, 3)]
        public float delayTime = 1.5f;
        Color initialFillBack;
        Color initialFillFront;
        float innerSliderTime;


        [Header("Player Render")]
        public bool invisibleFlag;
        public GameObject InvisableUI;
        public GameObject staff;
        public SkinnedMeshRenderer skinnedRenderBody;
        public SkinnedMeshRenderer skinnedRenderHand;
        public SkinnedMeshRenderer skinnedRenderArmor;
        public Material visiableMaterial;
        public Material invisableMaterial;
        public float invisableInterval = 10f;

        #endregion 

        PhotonView _photonView;
        int _playerID;
		bool _isAlive;
		float _currentHP;
		float _currentMovementSpeed;

        float _speedRatio;
        float _attackRatio;
        float _defenseRatio;

        #region Initializing...
        void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if (_photonView.IsMine)
            {
                if (hpSlider != null)
                {
                    Destroy(hpSlider.gameObject);
                }

                if(playerNameSprite != null)
                    Destroy(playerNameSprite.gameObject);

                if (scoreBoard != null)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        //Destroy(restart);
                        Destroy(backToRoom);
                    }
                    Destroy(restart);
                    scoreBoard.SetActive(false);
                }
            }
            else
            {
                if (myselfHpSlider != null)
                {
                    Destroy(myselfHpSlider.transform.parent.gameObject);
                }

                if (scoreBoard != null)
                {
                    Destroy(scoreBoard);
                }
            }
            initialFillBack = fillBackImage.color;
            initialFillFront = fillFrontImage.color;
            self_InitialFillBack = self_fillBackImage.color;
            self_InitialFillFront = self_fillFrontImage.color;
        }

		void OnEnable()
		{
            if(PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Player OnEnable called");
                GameManager_Main.instance.GameSetup += PlayerStatsInitialize; 
            }
            GameManager_Main.instance.GameEnd += ShowResultInScoreboard;
		}

		void OnDisable()
		{
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Player OnDisable called");
                GameManager_Main.instance.GameSetup -= PlayerStatsInitialize;
            }
            GameManager_Main.instance.GameEnd -= ShowResultInScoreboard;
		}

		void PlayerStatsInitialize()
		{
			_isAlive = true;
            invisibleFlag = false;
			_currentHP = Photon_GameSetting.instance.maxHealth;
			_currentMovementSpeed = Photon_GameSetting.instance.startMovementSpeed;
            _photonView.RPC("RPC_ResetPlayerStats", RpcTarget.All);
            UpdateHPSlider();
            if (_photonView.IsMine)
            {
                Debug.Log("current HP: " + _currentHP);
                Debug.Log("current Speet: " + _currentMovementSpeed);
            }
            Debug.Log("This Prefab ID: " + this._playerID);
            _photonView.RPC("UpdateStatus", RpcTarget.Others, this._isAlive, this._currentHP, this._currentMovementSpeed, this.invisibleFlag);
        }
        #endregion

        #region Update Player Status
        void UpdateHPSlider()
        {   
            // check the hp
            if (_currentHP <= 0)
            {
                _currentHP = 0;
                _isAlive = false;
            }
            else if(_currentHP > Photon_GameSetting.instance.maxHealth)
            {
                _currentHP = Photon_GameSetting.instance.maxHealth;
            }

            if (myselfHpSlider != null)
            {
                myselfHpSlider.value = _currentHP / Photon_GameSetting.instance.maxHealth;
                SetValueToSelfImageColor(myselfHpSlider.value);
            }

            if (hpSlider != null)
            {
                hpSlider.value = _currentHP / Photon_GameSetting.instance.maxHealth;
                SetValueToImageColor(hpSlider.value);
            }

            //--This player is dead--//
            if (!this._isAlive)
            {
                skinnedRenderBody.material = invisableMaterial;
                skinnedRenderHand.material = invisableMaterial;
                skinnedRenderArmor.material = invisableMaterial;
                staff.SetActive(false);
                if (InvisableUI != null) InvisableUI.SetActive(false);
                if (hpSlider != null) hpSlider.gameObject.SetActive(false);
                if (myselfHpSlider != null) myselfHpSlider.gameObject.SetActive(false);
                if (playerNameSprite != null) playerNameSprite.gameObject.SetActive(false);
                gameObject.GetComponent<Rigidbody>().useGravity = false;
                gameObject.GetComponent<Collider>().enabled = false;
                deathFire.Play();
                Sound_Manager.instance.PlayLoopSound(SoundIndex.Ghost);
            }
            else
            {
                Sound_Manager.instance.StopLoopSound(SoundIndex.Ghost);

                if (!invisibleFlag)
                {
                    skinnedRenderBody.material = visiableMaterial;
                    skinnedRenderHand.material = visiableMaterial;
                    skinnedRenderArmor.material = visiableMaterial;
                    staff.SetActive(true);
                    if (hpSlider != null) hpSlider.gameObject.SetActive(true);
                    if (playerNameSprite != null) playerNameSprite.gameObject.SetActive(true);
                }
                if (myselfHpSlider != null) myselfHpSlider.gameObject.SetActive(true);

                gameObject.GetComponent<Collider>().enabled = true;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                deathFire.Stop();
                deathFire.Clear();
            }
        }

        IEnumerator TemporarySetPlayerStats(float time, float ms, float att, float def)
        {
            this._speedRatio *= ms;
            this._attackRatio *= att;
            this._defenseRatio *= def;

            float totalTime = 0;
            float eachTime = time / 10;
            bool valid = true;
            while (totalTime < time)
            {
                if (!this._isAlive)
                {
                    valid = false;
                    break;
                }
                yield return new WaitForSeconds(eachTime);
                totalTime += eachTime;
            }

            if (valid)
            {
                this._speedRatio /= ms;
                this._attackRatio /= att;
                this._defenseRatio /= def;
            }
        }

        IEnumerator RunInvisable(float time)
        {
            skinnedRenderBody.material = invisableMaterial;
            skinnedRenderHand.material = invisableMaterial;
            skinnedRenderArmor.material = invisableMaterial;

            staff.SetActive(false);
            if (InvisableUI != null) InvisableUI.SetActive(true);
            if (hpSlider != null) hpSlider.gameObject.SetActive(false);
            //if (myselfHpSlider != null) myselfHpSlider.gameObject.SetActive(false);
            if (playerNameSprite != null) playerNameSprite.gameObject.SetActive(false);
            invisibleFlag = true;

            yield return new WaitForSeconds(time);

            skinnedRenderBody.material = visiableMaterial;
            skinnedRenderHand.material = visiableMaterial;
            skinnedRenderArmor.material = visiableMaterial;

            staff.SetActive(true);
            if (InvisableUI != null) InvisableUI.SetActive(false);
            if (hpSlider != null) hpSlider.gameObject.SetActive(true);
            //if (myselfHpSlider != null) myselfHpSlider.gameObject.SetActive(true);
            if (playerNameSprite != null) playerNameSprite.gameObject.SetActive(true);
            invisibleFlag = false;
        }

        IEnumerator ReduceSliderCountDown()
        {
            float currentTime = Time.time;
     
            //--Wait Until no damage received--//
            float timeInterval = (delayTime / 5f);
            while (currentTime < innerSliderTime)
            {
                currentTime += timeInterval;
                yield return new WaitForSeconds(timeInterval);
            }

            //--Start reducing Inner HP--//
            if(innerSliderImage != null)
            {
                while (innerSliderImage.fillAmount > hpSlider.value)
                {
                    innerSliderImage.fillAmount -= 0.03f;
                    if (hpSlider.value > innerSliderImage.fillAmount)
                        innerSliderImage.fillAmount = hpSlider.value;

                    if (hpInnerSlider != null)
                        hpInnerSlider.value = innerSliderImage.fillAmount;

                    yield return new WaitForSeconds(0.04f);
                }
            }
            else if(self_InnerSliderImage != null)
            {
                while (self_InnerSliderImage.fillAmount > myselfHpSlider.value)
                {
                    self_InnerSliderImage.fillAmount -= 0.05f;
                    if(myselfHpSlider.value > self_InnerSliderImage.fillAmount)
                        self_InnerSliderImage.fillAmount = myselfHpSlider.value;
                    
                    if(myselfHpInnerSlider != null)
                        myselfHpInnerSlider.value = self_InnerSliderImage.fillAmount;

                    yield return new WaitForSeconds(0.05f);
                }
            }
            
        }

        IEnumerator IncreaseSliderCountDown()
        {
            if(innerSliderImage != null)
            {
                while (innerSliderImage.fillAmount < hpSlider.value)
                {
                    innerSliderImage.fillAmount += 0.05f;
                    if (hpSlider.value < innerSliderImage.fillAmount)
                        innerSliderImage.fillAmount = hpSlider.value;

                    if (hpInnerSlider != null)
                        hpInnerSlider.value = innerSliderImage.fillAmount;

                    yield return new WaitForSeconds(0.05f);
                }
            }
            else if(self_InnerSliderImage != null)
            {
                while(self_InnerSliderImage.fillAmount < myselfHpSlider.value)
                {
                    self_InnerSliderImage.fillAmount += 0.05f;
                    if(myselfHpSlider.value < self_InnerSliderImage.fillAmount)
                        self_InnerSliderImage.fillAmount = myselfHpSlider.value;

                    if(myselfHpInnerSlider != null)
                        myselfHpInnerSlider.value = self_InnerSliderImage.fillAmount;

                    yield return new WaitForSeconds(0.05f); 
                }
            }
        }

        IEnumerator VibrateOnDamageReceive()
        {
            float currentTime = 0;

            while(currentTime < 0.5f)
            {
                GetComponent<GestureCollecting>().ControllerVibrate();
                float time = Time.deltaTime;
                yield return new WaitForSeconds(time);
                currentTime += time;
            }
        }

        void ShowResultInScoreboard()
        {
            if (_photonView.IsMine)
            {
                scoreBoard.SetActive(true);
                foreach (Transform child in scoreboardParent)
                    GameObject.Destroy(child.gameObject);

                for (int i = 0; i < GameManager_Main.instance.playerAmountInGame; i++)
                {
                    GameObject playerBlock = Instantiate(playerBlockPrefab, scoreboardParent);
                    GameManager_Main.instance.CalculateScoreResult();
                    playerBlock.GetComponent<Scoreboard_PlayerScore>().SetPlayerScore(Photon_GameSetting.instance.playerColor[i], GameManager_Main.instance.GetPercent(i), GameManager_Main.instance.GetScore(i));
                }
            }
        }

        #endregion

        #region Set Functions
        public void SetPlayerID(int id)
        {
            _photonView.RPC("BroadCastPlayerID", RpcTarget.Others, id);
        }

        public void SetPlayerStats(float duration, float ms, float att, float def)
        {
            _photonView.RPC("RPC_UpdatePlayerStats", RpcTarget.All, duration, ms, att, def);
        }

        void SetValueToImageColor(float currentPercent)
        {
            float r = initialFillBack.r * 255;
            float g = initialFillBack.g * 255;
            float turningValue = g - r;
            //Debug.Log("Turning Value: " + turningValue);
            float totalValue = turningValue + g;
            float actualValue = totalValue * (1-currentPercent);
            float new_r = 0;
            float new_g = 0;

            if ((r + actualValue) > g)
            {
                new_r = (g/255);
                new_g = (g - (actualValue-turningValue))/255;
            }
            else
            {
                new_r = (r+actualValue)/255;
                new_g = (g/255);
            }
            //Debug.Log("R: " + new_r + ", G: " + new_g);
            fillBackImage.color = new Color(new_r, new_g, initialFillBack.b, initialFillBack.a);


            r = initialFillFront.r * 255;
            g = initialFillFront.g * 255;
            turningValue = g - r;
            totalValue = turningValue + g;
            actualValue = totalValue * (1 - currentPercent);

            if ((r + actualValue) > g)
            {
                new_r = (g / 255);
                new_g = (g - (actualValue - turningValue)) / 255;
            }
            else
            {
                new_r = (r + actualValue) / 255;
                new_g = (g / 255);
            }
            fillFrontImage.color = new Color(new_r, new_g, initialFillFront.b, initialFillFront.a);
        }

        void SetValueToSelfImageColor(float currentPercent)
        {
            float r = self_InitialFillBack.r * 255;
            float g = self_InitialFillBack.g * 255;
            float turningValue = g - r;
            //Debug.Log("Turning Value: " + turningValue);
            float totalValue = turningValue + g;
            float actualValue = totalValue * (1-currentPercent);
            float new_r = 0;
            float new_g = 0;

            if ((r + actualValue) > g)
            {
                new_r = (g/255);
                new_g = (g - (actualValue-turningValue))/255;
            }
            else
            {
                new_r = (r+actualValue)/255;
                new_g = (g/255);
            }
            //Debug.Log("R: " + new_r + ", G: " + new_g);
            self_fillBackImage.color = new Color(new_r, new_g, self_InitialFillBack.b, self_InitialFillBack.a);


            r = self_InitialFillFront.r * 255;
            g = self_InitialFillFront.g * 255;
            turningValue = g - r;
            totalValue = turningValue + g;
            actualValue = totalValue * (1 - currentPercent);

            if ((r + actualValue) > g)
            {
                new_r = (g / 255);
                new_g = (g - (actualValue - turningValue)) / 255;
            }
            else
            {
                new_r = (r + actualValue) / 255;
                new_g = (g / 255);
            }
            self_fillFrontImage.color = new Color(new_r, new_g, self_InitialFillFront.b, self_InitialFillFront.a);
        }

        #endregion

        #region Uility Functions
        public bool IsPlayerAlive(){return _isAlive;}
		public float PlayerCurrentHealth(){return _currentHP;}
        public float CurrentMovementSpeed() { return _currentMovementSpeed*_speedRatio; }
        public float CurrentAttackRatio() { return _attackRatio; }
        public void Damage(float d, int casterID)
        {
            d = d / this._defenseRatio;
            if (GameManager_Main.instance.startGame && this._isAlive)
            {
                if (d > 0)
                {
                    if (_currentHP < d)
                    {
                        Photon_Room.instance.AddScoreOnDamage(casterID, (int)_currentHP);
                        //--Add 300 points for kill
                        Photon_Room.instance.AddScoreOnDamage(casterID, 30);
                    }
                    else
                        Photon_Room.instance.AddScoreOnDamage(casterID, (int)d);
                }

                _currentHP -= d;
                UpdateHPSlider();
                _photonView.RPC("UpdateStatus", RpcTarget.Others, this._isAlive, this._currentHP, this._currentMovementSpeed, this.invisibleFlag);
                if(d > 0)
                {
                    _photonView.RPC("UpdateLateHpBar", RpcTarget.All);
                    if (this._isAlive)
                        Sound_Manager.instance.PlayShortSound(SoundIndex.Damage);
                    else
                    {
                        Sound_Manager.instance.PlayShortSound(SoundIndex.Death);
                        //--Player Loses 300 points on Death--//
                        //Photon_Room.instance.AddScoreOnDamage(_playerID, -30);
                    } 
                }   
                else if (d < 0)
                    _photonView.RPC("InstantUpdateBackFill", RpcTarget.All);
            }
        }

        public void EnableInvisable(float invisableTime)
        {
            Debug.Log("Invisible time~");
            invisibleFlag = true;
            StartCoroutine(RunInvisable(invisableTime));
            _photonView.RPC("UpdateStatus", RpcTarget.Others, this._isAlive, this._currentHP, this._currentMovementSpeed, this.invisibleFlag);
        }

        public void RevivePlayer()
        {
            _isAlive = true;
            invisibleFlag = false;
            _currentHP = Photon_GameSetting.instance.maxHealth;
            _currentMovementSpeed = Photon_GameSetting.instance.startMovementSpeed;
            _photonView.RPC("RPC_ResetPlayerStats", RpcTarget.All);
            _photonView.RPC("ResetInnerSlider", RpcTarget.All);
            UpdateHPSlider();
            int index = Random.Range(0, Photon_GameSetting.instance.maxPlayer);
            this.transform.position = Photon_GameSetting.instance.playerSpawningInfo[index].position;
            this.transform.rotation = Photon_GameSetting.instance.playerSpawningInfo[index].rotation;
            _photonView.RPC("UpdateStatus", RpcTarget.Others, this._isAlive, this._currentHP, this._currentMovementSpeed, this.invisibleFlag);
            Sound_Manager.instance.PlayShortSound(SoundIndex.Revive);
        }

        #endregion

        #region RPC Functions

        [PunRPC]
        private void BroadCastPlayerID(int id)
        {
            this._playerID = id;
            playerNameSprite.sprite = Photon_GameSetting.instance.playerColor[this._playerID].image;
            if(this._playerID == Photon_Room.instance.GetMyPlayerID())
            {
                playerNameSprite.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(180, 0, 0);
            }
        }

        [PunRPC]
        private void UpdateStatus(bool _alive, float _hp, float _speed, bool _invisable)
        {
            if (this._currentHP > _hp)
                if (_photonView.IsMine && GameManager_Main.instance.hasVREnvironment)
                    StartCoroutine(VibrateOnDamageReceive());

            this._isAlive = _alive;
            this._currentHP = _hp;
            this._currentMovementSpeed = _speed;
            UpdateHPSlider();
            
            if (!this.invisibleFlag && _invisable)
            {
                this.invisibleFlag = _invisable;
                StartCoroutine(RunInvisable(invisableInterval));
            }
        }

        [PunRPC]
        private void RPC_ResetPlayerStats()
        {
            this._speedRatio = 1;
            this._attackRatio = 1;
            this._defenseRatio = 1;
        }

        [PunRPC]
        private void RPC_UpdatePlayerStats(float time, float addedSpeed, float attack, float defense)
        {
            StartCoroutine(TemporarySetPlayerStats(time, addedSpeed, attack, defense));
        }

        [PunRPC]
        private void ResetInnerSlider()
        {
            innerSliderImage.fillAmount = 1;

            if (hpInnerSlider != null)
                hpInnerSlider.value = 1;

            if(_photonView.IsMine)
            {
                if(myselfHpInnerSlider != null)
                    myselfHpInnerSlider.value = 1;

                self_InnerSliderImage.fillAmount = 1;
            }
        }

        [PunRPC]
        private void UpdateLateHpBar()
        {
            innerSliderTime = Time.time + delayTime;
            StartCoroutine(ReduceSliderCountDown());
        }

        [PunRPC]
        private void InstantUpdateBackFill()
        {
            StartCoroutine(IncreaseSliderCountDown());
        }

        #endregion

    }
}