using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    }

    float axis = 0;
    float speed = 4;
    void Update()
    {
        if (!PV.IsMine) return;

        Move();
        Jump();
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

    // axis는 동기화되고 있지 않으므로 인자값으로 받음
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
        RB.velocity = Vector2.zero;
        RB.AddForce(Vector2.up * 700);
    }

    // 변수 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
