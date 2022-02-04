using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    private Animator animator;
    private Rigidbody2D RB;
    private SpriteRenderer SR;
    private PhotonView PV;

    [SerializeField] Text nicknameText;
    [SerializeField] Image hpImage;

    [SerializeField] Vector3 currentPos;

    void Awake()
    {
        animator = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
        PV = GetComponent<PhotonView>();
        currentPos = transform.position;

        // PV.Owner : 포톤 뷰의 주인 (PV.IsMine이 true은 Player)
        nicknameText.text = (PV.IsMine) ? PhotonNetwork.NickName : PV.Owner.NickName;
        nicknameText.color = (PV.IsMine) ? Color.green : Color.red;

        if (PV.IsMine)
        {
            CinemachineVirtualCamera _cinemachine = FindObjectOfType<CinemachineVirtualCamera>();
            _cinemachine.Follow = transform;
            _cinemachine.LookAt = transform;
        }
    }

    float axis = 0;
    float speed = 4;
    void Update()
    {
        if (PV.IsMine)
        {
            Move();
            Jump();
            Shot();
        }
        else SyncedPosition();
    }

    void Move()
    {
        axis = Input.GetAxisRaw("Horizontal");
        // 중력이 있으므로 y는 현재 값 그대로 적용
        RB.velocity = new Vector2(speed * axis, RB.velocity.y);

        if (axis != 0)
        {
            animator.SetBool("IsWalk", true);
            // AllBuffered : 현재 방에 있는 플레이어는 물론 후에 새로 들어오는 플레이어도 실행함
            PV.RPC("FlipX", RpcTarget.AllBuffered, axis);
        }
        else animator.SetBool("IsWalk", false);
    }

    // 전역 변수 axis는 동기화되고 있지 않으므로 인자값으로 방향값을 받음
    [PunRPC]
    void FlipX(float _axis) => SR.flipX = (_axis == -1);


    [SerializeField] bool isGround = false;
    void Jump()
    {
        // 지정한 위치에 원을 그려서 충돌을 감지함
        isGround = Physics2D.OverlapCircle((Vector2)transform.position - (Vector2.up * 0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
        animator.SetBool("IsJump", !isGround);
        if(Input.GetKeyDown(KeyCode.Space) && isGround) PV.RPC("JumpRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void JumpRPC()
    {
        RB.velocity = Vector2.zero; // AddForce의 효과를 더 잘 받게 하기 위해 직전에 멈춤
        RB.AddForce(Vector2.up * 700);
    }


    // SR.flipX 가 true면 반대쪽 바라보는 중
    Vector3 ShotDir => transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0); 
    void Shot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject _bullet = PhotonNetwork.Instantiate("Bullet", ShotDir, Quaternion.identity);
            _bullet.GetComponent<MyBullet>().Shot(SR.flipX ? -1 : 1);
            animator.SetTrigger("Shot");
        }
    }

    public void OnDamage()
    {
        hpImage.fillAmount -= 0.1f;
        if (hpImage.fillAmount <= 0) Die();
    }

    void Die()
    {
        GameObject.Find("Canvas").transform.Find("Respawn Panel").gameObject.SetActive(true);
        PhotonNetwork.Destroy(gameObject);
    }

    // 변수 동기화
    // 동기화 목록 : 
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hpImage.fillAmount);
        }
        else
        {
            currentPos = (Vector3)stream.ReceiveNext();
            hpImage.fillAmount = (float)stream.ReceiveNext();
        }
    }


    // 거리가 10이 넘으면 즉시 순간이동시키고 그렇지 않다면 Lerp함수를 통해 부드럽게 위치 동기화하는 함수
    // 이 함수는 stream.IsWriting이 false인 상대 오브젝트의 동기화를 받는 쪽에서 실행되기 때문에 RPC는 필요가 없음
    void SyncedPosition()
    {
        // sqrMagnitude : 백터의 크기의 제곱을 구함. 그냥 크기를 구하는 것보다 빠름(백터의 거리를 구하는 과정에서 제곱근을 구해야 하는데 이를 생략하므로)
        if ((transform.position - currentPos).sqrMagnitude <= 100) transform.position = currentPos;
        else transform.position = Vector3.Lerp(transform.position, currentPos, Time.deltaTime * 10);
    }
}
