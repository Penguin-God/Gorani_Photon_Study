using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ObjectPool : MonoBehaviourPun
{
    public static ObjectPool Instance = null;

    [SerializeField] NetworkManager networkManager = null;

    [SerializeField] GameObject bullet = null;
    private Queue<MyBullet> BulletQueue = new Queue<MyBullet>();

    [SerializeField] int test;
    void Update()
    {
        test = BulletQueue.Count;
    }

    void Awake()
    {
        Instance = this;
        networkManager.OnJoinRoomEvent += () => Init(30);
    }

    void Init(int _count)
    {
        if(!PhotonNetwork.IsMasterClient) return;

        for (int i = 0; i < _count; i++)
        {
            BulletQueue.Enqueue(CreateNewBullet());
        }
    }

    // 총알은 마스터꺼. 클라이언트가 마스터한테쏘고 맞은얘가 마인이 아니면 함수 실행. 그 함수에서 마스터 클라이언트일 때만 연산 실행
    MyBullet CreateNewBullet()
    {
        // 오늘의 정답 PhotonNetwork.Instantiate는 오브젝트를 무조건 활성화시킨 상태로 생성한다.
        GameObject _bullet = PhotonNetwork.Instantiate(bullet.name, new Vector3(500, 500, 500), Quaternion.identity);
        return _bullet.GetComponent<MyBullet>();
    }

    public static MyBullet GetBullet()
    {
        if (Instance.BulletQueue.Count > 0)
        {
            MyBullet _bullet = Instance.BulletQueue.Dequeue();
            _bullet.photonView.RPC("SetActive", RpcTarget.All, true);
            return _bullet;
        }
        else
        {
            MyBullet _bullet = Instance.CreateNewBullet();
            _bullet.photonView.RPC("SetActive", RpcTarget.All, true);
            return _bullet;
        }
    }

    public static void ReturnBullet(MyBullet _bullet)
    {
        Instance.BulletQueue.Enqueue(_bullet);
    }
}
