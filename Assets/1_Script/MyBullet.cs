using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MyBullet : MonoBehaviourPun//, IPunObservable
{
    void Start() => gameObject.SetActive(false);

    void OnEnable()
    {
        StartCoroutine(Co_Return());
    }

    IEnumerator Co_Return()
    {
        yield return new WaitForSeconds(5f);
        StopCoroutine(shotCoroutine);
        ReturnPool();
    }


    Coroutine shotCoroutine = null;
    [SerializeField] float speed;

    [PunRPC]
    public void Shot(Vector3 _startPos, int _dir, int _playerId)
    {
        transform.position = _startPos;
        //currentPos = transform.position;
        currentShotPlayerId = _playerId;
        shotCoroutine = StartCoroutine(Co_Shot(_dir));
    }

    IEnumerator Co_Shot(int _dir)
    {
        while (true)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed * _dir);
            yield return null;
        }
    }

    [SerializeField] int currentShotPlayerId = -3;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") ReturnPool();

        // 대미지 연산은 방장만
        // A랑 B가 있을 때 호스트랑 클라 둘 다 맞으면 조건문 실행
        // 클라는 거르고 호스트에서 대미지 연산 후 클라한테 데이터를 전달해 동기화시킴
        if (collision.tag == "Player" && currentShotPlayerId != collision.GetComponent<Player>().id && PhotonNetwork.IsMasterClient)
        {
            collision.GetComponent<Player>().OnDamage();
            ReturnPool();
        }
    }

    void ReturnPool() => ObjectPool.ReturnBullet(this);

    [PunRPC]
    public void SetActive(bool _active)
    {
        // 한번 생성된 후 세상구경해본 애들이 스스로 껏다키기 가능
        if (_active) gameObject.SetActive(true);
        else
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
            transform.position = new Vector3(500, 500, 500);
            currentShotPlayerId = -3;
        }
    }

    //[SerializeField] Vector3 currentPos;
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(currentPos);
    //    }
    //    else
    //    {
    //        currentPos = (Vector3)stream.ReceiveNext();
    //        transform.position = currentPos;
    //    }
    //}
}
