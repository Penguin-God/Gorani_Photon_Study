using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MyBullet : MonoBehaviourPunCallbacks
{
    void Start() => StartCoroutine("Co_Destory");

    IEnumerator Co_Destory()
    {
        if (!photonView.IsMine) yield break;

        yield return new WaitForSeconds(3.5f);
        StopCoroutine(shotCoroutine);
        PhotonNetwork.Destroy(gameObject);
    }


    Coroutine shotCoroutine = null;
    [SerializeField] float speed;
    public void Shot(int _dir) => shotCoroutine = StartCoroutine(Co_Shot(_dir));

    IEnumerator Co_Shot(int _dir)
    {
        while (true)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed * _dir);
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") DestroyMy();

        // 총알은 상대방이 쏜 거고 내가 맞았을 때. 즉,  상대방이 쏜 총알이지만 계산은 동기화는 맞은 쪽에서 함. 이유는 쏜 쪽보다 맞은 쪽이 살짝 느리기 때문
        if (collision.tag == "Player" && !photonView.IsMine && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<Player>().OnDamage();
            DestroyMy();
        }
    }

    void DestroyMy()
    {
        StopAllCoroutines();
        photonView.RPC("RPC_Destory", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_Destory()
    {
        Destroy(gameObject);
    }
}
