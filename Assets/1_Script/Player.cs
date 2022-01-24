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

        // PV.Owner : ���� ���� ���� (PV.IsMine�� true�� Player)
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
        // �߷��� �����Ƿ� y�� ���� �� �״�� ����
        RB.velocity = new Vector2(speed * axis, RB.velocity.y);

        if (axis != 0)
        {
            animator.SetBool("IsWalk", true);
            // AllBuffered : ���� �濡 �ִ� �÷��̾�� ���� �Ŀ� ���� ������ �÷��̾ ������
            PV.RPC("FlipX", RpcTarget.AllBuffered, axis);
        }
        else animator.SetBool("IsWalk", false);
    }

    // axis�� ����ȭ�ǰ� ���� �����Ƿ� ���ڰ����� ����
    [PunRPC]
    void FlipX(float _axis) => SR.flipX = (_axis == -1);


    [SerializeField] bool isGround = false;
    void Jump()
    {
        // ������ ��ġ�� ���� �׷��� �浹�� ������
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

    // ���� ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
