using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameWizard
{
    enum panelType
    {
        Close = 0,
        Create = 1,
        Join = 2
    }

    [RequireComponent(typeof(Animator))]
	public class UI_ConfirmPanel : MonoBehaviour {

        [SerializeField]
        private Text text_Title;
        [SerializeField]
        private InputField inputField;
        [SerializeField]
        private Text placeHolder;
        [SerializeField]
        private Button btn_OK;
        [SerializeField]
        private Text text_OK;
        

        private Animator animation;

        void Start()
        {
            animation = GetComponent<Animator>();
            btn_OK.interactable = false;
        }

        void OnClickedCreateButton()
        {
            Photon_Lobby.instance.CreateNewRoom(inputField.text);
            ClosePanel();
        }
        void OnClickedJoinButton()
        {
            Photon_Lobby.instance.JoinRoom(inputField.text);
            ClosePanel();
        }

        public virtual void OnClickedCancelButton()
        {
            ClosePanel();
        }

        public void OpenPanel(int type)
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ClickBtnSound);
            Photon_Lobby.instance.LobbyToComfirm(true);
            SetPanel((panelType)type);
            inputField.onEndEdit.AddListener(delegate { CheckInput(); });
            animation.SetBool("Open", true);
        }
        void ClosePanel()
        {
            Sound_Manager.instance.PlayShortSound(SoundIndex.ClickBtnSound);
            SetPanel(panelType.Close);
            animation.SetBool("Open", false);
            inputField.onEndEdit.RemoveAllListeners();
            Photon_Lobby.instance.LobbyToComfirm(false);
        }

        void SetPanel(panelType type)
        {
            btn_OK.onClick.RemoveAllListeners();
            if (type == panelType.Create)
            {
                text_Title.text = "Create Room";
                placeHolder.text = "Type Room Name";
                text_OK.text = "Create";
                btn_OK.onClick.AddListener(delegate { OnClickedCreateButton(); });
            }
            else if (type == panelType.Join)
            {
                text_Title.text = "Join Room";
                placeHolder.text = "Enter Room Name";
                text_OK.text = "Join";
                btn_OK.onClick.AddListener(delegate { OnClickedJoinButton(); });
            }
            else
            {
                inputField.text = "";
                btn_OK.interactable = false;
            }
        }

        void CheckInput()
        {
            //--Must be lesser than 10
            bool check = false;
            if (inputField.text.Length > 10)
                check = false;
            else
            {
                //--Must be number of alphabet
                for (int i = 0; i < inputField.text.Length; i++)
                {
                    if ((inputField.text[i] >= '0') && (inputField.text[i] <= '9'))
                    {
                        check = true;
                    }
                    else if ((inputField.text[i] >= 'A') && (inputField.text[i] <= 'Z'))
                    {
                        check = true;
                    }
                    else if ((inputField.text[i] >= 'a') && (inputField.text[i] <= 'z'))
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
            }

            if (check)
            {
                btn_OK.interactable = true;
                Debug.Log("Result: It's a valid name");
            }
            else
            {
                btn_OK.interactable = false;
                Debug.Log("Result: It's an invalid name");
            }
                
            

        }

        
	}
}

