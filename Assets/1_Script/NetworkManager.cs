using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] InputField nicknameField = null;
    [SerializeField] GameObject disconnectPanel = null;
    [SerializeField] GameObject respawnPanel = null;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // 버튼 클릭으로 실행
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    // 접속하면 실행
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameField.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    // 방에 접속하면 실행
    public override void OnJoinedRoom()
    {
        disconnectPanel.SetActive(false);
        StartCoroutine(Co_JoinRoom()); 
    }

    IEnumerator Co_JoinRoom()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
                yield break;
            }
            yield return null;
        }
    }

    // 방에서 나가면 실행
    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }
}
