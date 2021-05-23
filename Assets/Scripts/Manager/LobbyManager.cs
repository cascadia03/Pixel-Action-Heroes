using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField NicknameInputField;
    [SerializeField] private Text       MessageText;

    private void Awake()
    {
        MessageText.color = new Color(1, 1, 1, 0);

        Screen.SetResolution(1920, 1080, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Connect();
        if (Input.GetKeyDown(KeyCode.Escape))
            Exit();
    }

    public void Connect()
    {
        if (IsNicknameOK())
            PhotonNetwork.ConnectUsingSettings();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private bool IsNicknameOK()
    {
        string nickname = NicknameInputField.text;

        if (nickname.Length < 1)
        {
            ShowMessage("닉네임을 입력해주세요");
            return false;
        }

        if (nickname.Length > 8)
        {
            ShowMessage("8자 이하로만 입력해주세요");
            return false;
        }

        char[] ch = nickname.ToCharArray();

        for (int i = 0; i < ch.Length; i++)
        {
            if (!(0x61 <= ch[i] && ch[i] <= 0x7A) && !(0x41 <= ch[i] && ch[i] <= 0x5A))
            {
                ShowMessage("영문으로만 입력해주세요");
                return false;
            }
        }

        return true;
    }

    private void ShowMessage(string message)
    {
        if (mMessage != null) StopCoroutine(mMessage);
        mMessage = StartCoroutine(Message(message));
    }

    private Coroutine mMessage;
    private IEnumerator Message(string message)
    {
        MessageText.text = message;
        MessageText.color = new Color(1, 1, 1, 1);

        while(MessageText.color.a > 0)
        {
            MessageText.color -= new Color(0, 0, 0, 0.01f);
            yield return new WaitForSecondsRealtime(0.01f);
        }

        MessageText.text = "";
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NicknameInputField.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 10 }, null);
        SceneManager.LoadScene("InGame");
    }
}
