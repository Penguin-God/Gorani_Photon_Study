using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MyBullet : MonoBehaviourPun
{
    void Start() => gameObject.SetActive(false);

    IEnumerator Co_Destory()
    {
        if (!photonView.IsMine) yield break;

        yield return new WaitForSeconds(3.5f);
        StopCoroutine(shotCoroutine);
        ReturnPool();
    }


    Coroutine shotCoroutine = null;
    [SerializeField] float speed;

    [PunRPC]
    public void Shot(Vector3 _startPos, int _dir) => shotCoroutine = StartCoroutine(Co_Shot(_startPos, _dir));

    IEnumerator Co_Shot(Vector3 _startPos, int _dir)
    {
        transform.position = _startPos;
        StartCoroutine("Co_Destory");
        while (true)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed * _dir);
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") ReturnPool();

        // 대미지 연산은 방장만
        if (collision.tag == "Player" && !collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<Player>().OnDamage(); // 마스터 클라로 바꾸기
            ReturnPool();
        }
    }

    void ReturnPool()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
        transform.position = new Vector3(500, 500, 500);
        if(PhotonNetwork.IsMasterClient) ObjectPool.ReturnBullet(this);
    }

    [PunRPC]
    public void SetActive(bool _active)
    {
        // 한번 생성된 후 세상구경해본 애들이 스스로 껏다키기 가능
        // 총알 위치 동기화하기
        gameObject.SetActive(_active);
    }
}
