/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/15
 * Description: 觸發施放技能，Debug模式為可用鍵盤1-5觸發技能。
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using System.IO;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerSkill : MonoBehaviour
    {
        [Header("Skill List")]
        public Transform playerCamera;
        public ParticleSystem castingGround;
        public ParticleSystem castingTrail;
        public ParticleSystem failedSpell;
        public BaseSkill[] skills = new BaseSkill[5];
        public bool[] canCastSkill = new bool[5];
        
        bool _isCasting = false;
        bool _received = false;
        bool _debugFlag;

        private PhotonView photonView;
        private PlayerStatus _playerStatus;

        int chosenSpell = 5;
        private InGameInfoUIManager skillUIManager;

        // Use this for initialization
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            _playerStatus = GetComponent<PlayerStatus>();
            skillUIManager = GetComponent<InGameInfoUIManager>();
            if (!photonView.IsMine)
            {
                Destroy(this);
            }
            _debugFlag = GameManager_Main.instance.debugFlag;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager_Main.instance.startGame && _playerStatus.IsPlayerAlive())
            {
                if (_debugFlag)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                        CastingSkill(0, (playerCamera.position), playerCamera.forward);
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                        CastingSkill(1, (playerCamera.position), playerCamera.forward);
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                        CastingSkill(2, (playerCamera.position), playerCamera.forward);
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                        CastingSkill(3, (playerCamera.position), playerCamera.forward);
                    else if (Input.GetKeyDown(KeyCode.Alpha5))
                        CastingSkill(4, (playerCamera.position), playerCamera.forward);
                }
                else
                {
                    //--Relase and Cast a spell--//
                    if (Input.GetKeyUp(KeyCode.Space) || (_received))
                    {
                        _received = false;
                        Sound_Manager.instance.StopLoopSound(SoundIndex.Casting);
                        if (castingGround.isPlaying)
                        {
                            castingGround.Stop();
                            castingGround.Clear();
                        }
                        if(castingTrail.isPlaying)
                        {
                            castingTrail.Stop();
                            castingTrail.Clear();
                        }
                            
                        if ((chosenSpell <= 4) && (canCastSkill[chosenSpell]))
                            CastingSkill(chosenSpell, (playerCamera.position), playerCamera.forward);
                        else
                        {
                            Debug.Log("Skill Failed");
                            Sound_Manager.instance.PlayShortSound(SoundIndex.CastFail);
                            if(!failedSpell.isPlaying)
                                failedSpell.Play();
                        }
                            
                    }
                    //--Pressed spacebar or trigger--//
                    else if (Input.GetKeyDown(KeyCode.Space) || _isCasting)
                    {
                        chosenSpell = 5;
                        if (!castingGround.isPlaying)
                            castingGround.Play();
                        if(!castingTrail.isPlaying)
                        {
                            Debug.Log("Playing Particle");
                            castingTrail.Play();
                        }
                            
                        Sound_Manager.instance.PlayLoopSound(SoundIndex.Casting);
                        _isCasting = false;
                    }
                    //--Holding spacebar or trigger--//
                    else if (Input.GetKey(KeyCode.Space) /*|| Trigger is pressing*/)
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1))
                            chosenSpell = 0;
                        else if (Input.GetKeyDown(KeyCode.Alpha2))
                            chosenSpell = 1;
                        else if (Input.GetKeyDown(KeyCode.Alpha3))
                            chosenSpell = 2;
                        else if (Input.GetKeyDown(KeyCode.Alpha4))
                            chosenSpell = 3;
                        else if (Input.GetKeyDown(KeyCode.Alpha5))
                            chosenSpell = 4;
                    }
                }
                

                if(GameManager_Main.instance.debugFlag && Input.GetKeyDown(KeyCode.F))
                    GetComponent<PlayerStatus>().Damage(10, Photon_Room.instance.GetMyPlayerID());
            }
            else if (GameManager_Main.instance.endGame)
            {
                Sound_Manager.instance.StopLoopSound(SoundIndex.Casting);
                if (castingGround.isPlaying)
                {
                    castingGround.Stop();
                    castingGround.Clear();
                }
                if (castingTrail.isPlaying)
                {
                    castingTrail.Stop();
                    castingTrail.Clear();
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        //PhotonNetwork.LoadLevel(Photon_GameSetting.instance.scenesIndex.firstScene);
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        Photon_Room.instance.BackToMainMenu();
                    }
                }
            }
            
        }

        public void CastingSkill(int skillIndex, Vector3 pos, Vector3 direction)
        {
            if((skillIndex >=0) && (skillIndex <= Photon_GameSetting.instance.allKindsOfSpell.Length))
            {
                if (!_debugFlag)
                    StartCoroutine(CDCounter(skillIndex, skills[skillIndex].coolDownTime));

               
                if (skills[skillIndex].isGroundSkill)
                {
                    pos = new Vector3(pos.x, transform.position.y, pos.z);
                    direction.y = 0;
                    pos = pos + skills[skillIndex].castingOffset * direction;
                }    
                else
                {
                    pos = pos + skills[skillIndex].castingOffset * direction;
                }

                for (int i = 0; i < skills[skillIndex].castingAmount; i++)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    switch (i)
                    {
                        case 1:
                            rotation *= Quaternion.Euler(0, 30, 0);
                            break;
                        case 2:
                            rotation *= Quaternion.Euler(0, -30, 0);
                            break;
                        case 3:
                            rotation *= Quaternion.Euler(0, 60, 0);
                            break;
                        case 4:
                            rotation *= Quaternion.Euler(0, -60, 0);
                            break;
                        case 5:
                            rotation *= Quaternion.Euler(0, 90, 0);
                            break;
                        case 6:
                            rotation *= Quaternion.Euler(0, -90, 0);
                            break;
                        case 7:
                            rotation *= Quaternion.Euler(0, 120, 0);
                            break;
                        case 8:
                            rotation *= Quaternion.Euler(0, -120, 0);
                            break;
                        case 9:
                            rotation *= Quaternion.Euler(0, 150, 0);
                            break;
                        case 10:
                            rotation *= Quaternion.Euler(0, -150, 0);
                            break;
                        case 11:
                            rotation *= Quaternion.Euler(0, 180, 0);
                            break;
                    }
                  
                    GameObject s = PhotonNetwork.Instantiate(Path.Combine("Skills", skills[skillIndex].skillPrefab.name), pos, rotation);
                    CastingSkill castingSkill = s.GetComponent<CastingSkill>();
                    castingSkill.SetSkill(Photon_Room.instance.GetMyPlayerID(), _playerStatus.CurrentAttackRatio());                  
                }
            }
        }


        IEnumerator CDCounter(int skillIndex, float time)
        {
            skillUIManager.CoolDownBegin(skillIndex, time);
            canCastSkill[skillIndex] = false;
            Debug.Log("Skill " + skillIndex + " is on CD");
            float amount = time/100;
            float currentTotal = 0;
            while(currentTotal < time)
            {
                currentTotal += amount;
                //call for ui update//
                skillUIManager.CoolDowning(skillIndex, time, currentTotal);
                yield return new WaitForSeconds(amount);
            }
            
            canCastSkill[skillIndex] = true;
            skillUIManager.coolDownEnd(skillIndex);
        }

        #region Set Functions
        public void CastingSpell(int index)
        {
            chosenSpell = index;
            _received = true;
        }

        public void IsCasting()
        {
            _isCasting = true;
        }
        #endregion
    }
}

