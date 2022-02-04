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

    // 1. Ŭ���̾�Ʈ �����Ͱ� �ֿ� �۾� ����ȭ�ϱ�
    // 2. �Ѿ� Ǯ���ϱ�
    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // UI ��ư Ŭ������ ����. ContextMenu�� �׽�Ʈ ���Ǹ� ���ؼ� �߰���
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
        StartCoroutine(Co_DestroyAllBullet());
        StartCoroutine(Co_EscapeRoom());
    }

    // respawn button Ŭ�����ε� ���
    public void PlayerSpawn()
    {
        PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-6f, 15f), 3, 0), Quaternion.identity);
        respawnPanel.SetActive(false);
    }

    IEnumerator Co_DestroyAllBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject _bullet in GameObject.FindGameObjectsWithTag("Bullet")) _bullet.GetComponent<PhotonView>().RPC("RPC_Destory", RpcTarget.All);
    }

    // ��ȿ� ������
    IEnumerator Co_EscapeRoom()
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

    // �濡�� ������ ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }
}
