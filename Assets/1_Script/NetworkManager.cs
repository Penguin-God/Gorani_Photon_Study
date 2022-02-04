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

    // 1. 클라이언트 마스터가 주요 작업 동기화하기
    // 2. 총알 풀링하기
    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    // UI 버튼 클릭으로 실행. ContextMenu는 테스트 편의를 위해서 추가함
    [ContextMenu("접속")]
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
        PlayerSpawn();
        StartCoroutine(Co_DestroyAllBullet());
        StartCoroutine(Co_EscapeRoom());
    }

    // respawn button 클릭으로도 사용
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

    // 방안에 있을때
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

    // 방에서 나가면 실행
    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }
}
