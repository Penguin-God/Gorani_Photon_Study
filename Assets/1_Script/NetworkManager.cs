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
    [SerializeField] Button respawnButton = null;
    // 1. 클라이언트 마스터가 주요 작업 동기화하기
    // 2. 총알 풀링하기
    // 3. 매칭 시스템 만들기

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
    public event System.Action OnJoinRoomEvent = null;
    public override void OnJoinedRoom()
    {
        disconnectPanel.SetActive(false);
        PlayerSpawn();
        StartCoroutine(Co_DestroyAllBullet());
        StartCoroutine(Co_EscapeRoom());
        if (OnJoinRoomEvent != null) OnJoinRoomEvent();
    }

    int id = 0;
    public void PlayerSpawn()
    {
        Player _newPlayer = PhotonNetwork.Instantiate(player.name, new Vector3(Random.Range(-6f, 15f), 3, 0), Quaternion.identity).GetComponent<Player>();
        _newPlayer.id = ++id;
        photonView.RPC("UpdateId", RpcTarget.OthersBuffered, id);
        respawnButton.onClick.AddListener(() => _newPlayer.Respawn());
    }

    [PunRPC]
    void UpdateId(int _id)
    {
        id = _id;
    }


    IEnumerator Co_DestroyAllBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject _bullet in GameObject.FindGameObjectsWithTag("Bullet")) _bullet.GetComponent<PhotonView>().RPC("RPC_Destory", RpcTarget.All);
    }

    // 방안에 있을때 방 나가기
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
