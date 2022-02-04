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

        // �Ѿ��� ������ �� �Ű� ���� �¾��� ��. ��,  ������ �� �Ѿ������� ����� ����ȭ�� ���� �ʿ��� ��. ������ �� �ʺ��� ���� ���� ��¦ ������ ����
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
