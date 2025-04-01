using Mirror;
using System.Collections;
using System.Collections.Generic;
using Telepathy;
using UnityEngine;

public class ZombieMove : NetworkBehaviour
{
    [SyncVar]
    private Vector2 knockBackDirection;
    [SyncVar]
    private Vector2 syncPosition;
    [SyncVar]
    private bool isKnockedBack = false;
    [SyncVar(hook = nameof(OnFlipXChanged))]
    private bool syncFlipX;
    [SyncVar]
    private float syncX;
    [SyncVar]
    private float syncY;
    [SyncVar(hook = nameof(OnAinmationChanged))]
    private bool syncIsMoving;
    [SyncVar]
    public bool syncStun;
    [SyncVar]
    private double syncStunEndTime;
    public double stunDuration = 3f;


    public Transform target;
    public LayerMask playerLayer;
    public float moveSpeed;
    public float initMoveSpeed;
    private float knockBackSpeed;
    private float knockBackTime;
    private SpriteRenderer render;
    private Animator anime;
    private ZombieAttack attack;
    private bool isMoving;

    void Start()
    {
        moveSpeed = initMoveSpeed;
        attack = GetComponent<ZombieAttack>();
        render = GetComponent<SpriteRenderer>();
        anime = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isServer) // ���������� �̵� ���
        {
            if (isKnockedBack)
            {
                transform.position += (Vector3)knockBackDirection * knockBackSpeed * Time.deltaTime;
                knockBackTime -= Time.deltaTime;

                if (knockBackTime <= 0)
                {
                    isKnockedBack = false;
                }
            }
            else if(syncStun)
            {
                moveSpeed = 0;
                syncIsMoving = false; // ���� ���¿��� ������ ����

                // ���� ��Ʈ��ũ �ð��� ���� �ð��� �������� üũ
                if(NetworkTime.time >= syncStunEndTime)
                {
                    moveSpeed = initMoveSpeed;
                    syncStun = false;
                }
            }
            else
            {
                Moving();
            }

            syncPosition = transform.position; // ��ġ ����ȭ
        }
        else
        {
            transform.position = syncPosition; // Ŭ���̾�Ʈ�� ���� ���� ����
        }
        
    }

    public void ApplyKnockBack(Vector2 direction, float force, float duration)
    {
        knockBackDirection = direction;
        knockBackSpeed = force;
        knockBackTime = duration;
        isKnockedBack = true;
    }

    public void ApplyStun()
    {
        if (!isServer) return;
        syncStun = true;
        syncStunEndTime = NetworkTime.time + stunDuration; // ����ð� + ���ӽð�
        Debug.Log(syncStun);
    }

    // �÷��̾� ����ٴϴ� ���� �̵�
    [Server]
    public void Moving()
    {
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(transform.position, 5f, playerLayer);

        if(hitPlayer.Length > 0)
        {
            target = hitPlayer[0].transform; // ���� ����� �÷��̾� ����

            Vector2 direction = (target.position - transform.position).normalized;

            if(syncStun)
            {
                isMoving = false;
                syncIsMoving = false;
            }
            else if(Vector2.Distance(transform.position, target.position) > 1f)
            {
                isMoving = true;
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
            else
            {
                isMoving = false;
                attack.TryAttack(target);
            }         

            bool newFlipX = direction.x < 0;
            syncFlipX = newFlipX;

            // ���� ���°� �ƴ� ���� syncIsMoving ����
            if(!syncStun)
            {
                syncIsMoving = isMoving;
            }
            CmdAnimation(direction.x, direction.y);            
        }
    }

    [ClientRpc]
    void CmdAnimation(float x, float y)
    {
        syncX = x;
        syncY = y;        
    }

    private void OnAinmationChanged(bool oldValue, bool newValue)
    {
        anime.SetBool("isMove", newValue);
    }

    private void OnFlipXChanged(bool oldValue, bool newValue)
    {
        if(render != null)
        {
            render.flipX = newValue;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
