using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class InGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject MenuUI;
    [SerializeField] private GameObject DiedUI;

    [HideInInspector] public bool IsMenu = false;

    private void Update()
    {
        Menu();
        Dead();
    }

    private void Menu()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            MenuUI.SetActive(!MenuUI.activeSelf);
            IsMenu = !IsMenu;
        }
    }

    private void Dead()
    {
        foreach (GameObject Iter in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (Iter.GetComponent<PhotonView>().IsMine)
                if (Iter.GetComponent<Player>().HP <= 0)
                {
                    PhotonNetwork.Destroy(Iter);
                    DiedUI.SetActive(true);
                }
        }
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Lobby");
    }

    public void Return() => MenuUI.SetActive(false);

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Prefabs/Player", Vector2.zero, Quaternion.identity);
    }
}
