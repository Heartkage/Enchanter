/*
 * Coder: Yuansyun Ye
 * Date: 2019/05/15
 * Description: 透過BaseSkill施放技能的Prefab使用
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace GameWizard
{
    [RequireComponent(typeof(PhotonView))]
    public class CastingSkill : MonoBehaviourPunCallbacks, IPunObservable
    {
        public BaseSkill skill;

        #region Self Variables

        bool hasTriggeredEffect;
        int _ownerID;
        float currentAttackRatio;
        //--Variables serialize across network-//
        float _currentDamage;
        float _currentForce;

        #endregion


        private PhotonView _photonView;
        private Rigidbody rig;
        private string playerTag = "Player";
        private string defaultTag = "Untagged";

        // Use this for initialization
        void Awake()
        {
            rig = GetComponent<Rigidbody>();
            _photonView = GetComponent<PhotonView>();
            hasTriggeredEffect = false;
            if (skill != null)
            {
                _currentDamage = skill.damage;
                _currentForce = skill.force;
            }
            else
                Debug.Log("[System] Warning, skill of this prefab is not set!");
        }

        void FixedUpdate()
        {
            if (_photonView.IsMine)
            {
                if ((rig != null) && (skill != null))
                {
                    if (skill.skillType == SkillType.SkillShot)
                        rig.AddForce(transform.forward * skill.force, ForceMode.Force);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_photonView.IsMine)
            {
                return;
            }

            if (other.CompareTag(playerTag))
            {
                PhotonView otherPhotonView = other.GetComponent<PhotonView>();
                PlayerStatus status = other.GetComponent<PlayerStatus>();
                
                //Check the collideing skill what is or not myself casting.

                if (otherPhotonView.IsMine)
                {
                    if (skill.skillType == SkillType.Utility)
                    {
                        if (skill.hasEffect && !hasTriggeredEffect)
                        {
                            if(skill.stealth)
                                status.EnableInvisable(skill.buffDuration);

                            if (skill.updateHP)
                                status.Damage(-skill.HP, _ownerID); //--Negative damage <=> heal--//

                            if (skill.updatePlayerStatus)
                            {
                                status.SetPlayerStats(skill.buffDuration, skill.changeMovingSpeedRatio, skill.changeAttackRatio, skill.changeDefenseRatio);
                            }

                            hasTriggeredEffect = true;
                            if (skill.hitEffect != null)
                                PhotonNetwork.Instantiate(skill.hitEffect.name, transform.position + skill.hitEffectPositionOffset, Quaternion.identity);
                        }
                    }
                }
                else
                {
                    //Transfer the skill ownership
                    if (skill.skillType != SkillType.Utility)
                    {
                        float actualDamage = this._currentDamage * currentAttackRatio;
                        status.Damage(actualDamage, _ownerID);
                        
                        if (skill.hasEffect)
                            status.SetPlayerStats(skill.buffDuration, skill.changeMovingSpeedRatio, skill.changeAttackRatio, skill.changeDefenseRatio);

                        if (skill.hitEffect != null)
                            PhotonNetwork.Instantiate(skill.hitEffect.name, transform.position + skill.hitEffectPositionOffset, Quaternion.identity);

                        if (skill.destroyOnImpact)
                        {
                            PhotonNetwork.Destroy(gameObject);
                        } 
                    } 
                }
            }
            else if (other.CompareTag(defaultTag))
            {
                CastingSkill otherSkill = other.gameObject.GetComponent<CastingSkill>();
                //--Hits Another Skill--//
                if (otherSkill != null)
                {
                    //-- If not my skill --//
                    if (otherSkill.GetOwnerID() != this._ownerID)
                    {
                        if (skill.destroyOnImpact && otherSkill.skill.destroyOnImpact)
                        {
                            float damageDifference = this._currentDamage - otherSkill.GetCurrentDamage();
                            if (damageDifference > 0.1f)
                            {
                                SetSkillStats(damageDifference, this._currentForce * 0.6f);
                            }
                            else
                            {
                                if (skill.hitSkillEffect != null)
                                    PhotonNetwork.Instantiate(skill.hitSkillEffect.name, transform.position + skill.hitEffectPositionOffset, Quaternion.identity);
                                PhotonNetwork.Destroy(this.gameObject);
                            }
                        }

                        if ((skill.skillType == SkillType.SkillShot) && (otherSkill.skill.skillType == SkillType.Utility)) 
                        {
                            if (otherSkill.skill.hasEffect && otherSkill.skill.cancelOtherSpell)
                            {
                                if (skill.hitSkillEffect != null)
                                    PhotonNetwork.Instantiate(skill.hitSkillEffect.name, transform.position + skill.hitEffectPositionOffset, Quaternion.identity);
                                PhotonNetwork.Destroy(this.gameObject);
                            }
                        }
                    }
                }
                //-- Hits terrain--//
                else
                {
                    if (skill.destroyOnImpact)
                    {
                        if (skill.hitEffect != null)
                            PhotonNetwork.Instantiate(skill.hitEffect.name, transform.position + skill.hitEffectPositionOffset, Quaternion.identity);
                        PhotonNetwork.Destroy(this.gameObject);
                    }
                }
            }
        }

        IEnumerator RunDestroyTimer()
        {
            yield return new WaitForSeconds(skill.existingDuration);
            PhotonNetwork.Destroy(gameObject);
        }

        #region api

        public int GetOwnerID() {return this._ownerID;}
        public float GetCurrentDamage() { return this._currentDamage; }
        public float GetCurrentForce() { return this._currentForce; }

        public void SetSkill(int playerID, float attackRatio)
        {
            _photonView.RPC("RPC_SetPlayerID", RpcTarget.All, playerID, attackRatio);
            StartCoroutine(RunDestroyTimer());
        }

        void SetSkillStats(float damage, float force)
        {
            //Debug.Log("My Spell Damage: " + this._currentDamage + " > " + damage);
            //Debug.Log("My Spell Force: " + this._currentForce + " > " + force);
            this._currentDamage = damage;
            this._currentForce = force;
        }

        [PunRPC]
        private void RPC_SetPlayerID(int playerID, float attackRatio)
        {
            this._ownerID = playerID;
            this.currentAttackRatio = attackRatio;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentDamage);
                stream.SendNext(_currentForce);
            }
            else
            {
                _currentDamage = (float)stream.ReceiveNext();
                _currentForce = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}


