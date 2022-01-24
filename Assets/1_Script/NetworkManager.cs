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
    [SerializeField] GameObject player = null;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // ��ư Ŭ������ ����
    [ContextMenu("����")]
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    // �����ϸ� ����
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameField.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    // �濡 �����ϸ� ����
    public override void OnJoinedRoom()
    {
        disconnectPanel.SetActive(false);
        PlayerSpawn();
        StartCoroutine(Co_InTheRoom()); 
    }

    IEnumerator Co_InTheRoom()
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

    // respawn button������ ���
    public void PlayerSpawn()
    {
        PhotonNetwork.Instantiate(player.name, Vector3.zero, Quaternion.identity);
        respawnPanel.SetActive(false);
    }

    // �濡�� ������ ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }
}
