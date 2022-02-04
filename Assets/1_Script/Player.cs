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

        // PV.Owner : ���� ���� ���� (PV.IsMine�� true�� Player)
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

    // ���� ���� axis�� ����ȭ�ǰ� ���� �����Ƿ� ���ڰ����� ���Ⱚ�� ����
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
        RB.velocity = Vector2.zero; // AddForce�� ȿ���� �� �� �ް� �ϱ� ���� ������ ����
        RB.AddForce(Vector2.up * 700);
    }


    // SR.flipX �� true�� �ݴ��� �ٶ󺸴� ��
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

    // ���� ����ȭ
    // ����ȭ ��� : 
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


    // �Ÿ��� 10�� ������ ��� �����̵���Ű�� �׷��� �ʴٸ� Lerp�Լ��� ���� �ε巴�� ��ġ ����ȭ�ϴ� �Լ�
    // �� �Լ��� stream.IsWriting�� false�� ��� ������Ʈ�� ����ȭ�� �޴� �ʿ��� ����Ǳ� ������ RPC�� �ʿ䰡 ����
    void SyncedPosition()
    {
        // sqrMagnitude : ������ ũ���� ������ ����. �׳� ũ�⸦ ���ϴ� �ͺ��� ����(������ �Ÿ��� ���ϴ� �������� �������� ���ؾ� �ϴµ� �̸� �����ϹǷ�)
        if ((transform.position - currentPos).sqrMagnitude <= 100) transform.position = currentPos;
        else transform.position = Vector3.Lerp(transform.position, currentPos, Time.deltaTime * 10);
    }
}
