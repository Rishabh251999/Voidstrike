using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PublicChatManager : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField chatInput;

    [Header("GameObjects")]
    [SerializeField] private GameObject chatLog;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject chatUI;


    private void Awake()
    {
        chatUI = GameObject.FindWithTag("Canvas");

        chatContent = chatUI.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;
        chatInput = chatUI.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TMP_InputField>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            OnSendButtonClicked();
    }


    /// <summary>
    /// This is being called when you click on the Enter key to send the message.
    /// </summary>
    private void OnSendButtonClicked()
    {
        if (isLocalPlayer)
        {
            if (!string.IsNullOrEmpty(chatInput.text))
            {
                CmdSendChat(chatInput.text);
                chatInput.text = "";

                // Refocus on the input field
                chatInput.Select();
                chatInput.ActivateInputField();
            }
        }
    }


    /// <summary>
    /// This is we requesting the server that we want to send a message.
    /// </summary>
    [Command]
    void CmdSendChat(string message) => RpcDisplayChat($"{GetComponent<BattleMultiplayerManager>().playerName}: {message + "\n"}");


    /// <summary>
    /// We telling all the cients that we have sent a message, please receive it.
    /// </summary>
    [ClientRpc]
    void RpcDisplayChat(string message)
    {
        GameObject chat = Instantiate(chatLog);

        chat.GetComponent<TextMeshProUGUI>().text = message;
        chat.transform.SetParent(chatContent.transform, false);
    }
}