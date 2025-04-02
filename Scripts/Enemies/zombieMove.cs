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
        if(isServer) // 서버에서만 이동 계산
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
                syncIsMoving = false; // 스턴 상태에서 움직임 멈춤

                // 현재 네트워크 시간이 종료 시간을 지났는지 체크
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

            syncPosition = transform.position; // 위치 동기화
        }
        else
        {
            transform.position = syncPosition; // 클라이언트는 서버 값을 따름
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
        syncStunEndTime = NetworkTime.time + stunDuration; // 현재시간 + 지속시간
        Debug.Log(syncStun);
    }

    // 플레이어 따라다니는 좀비 이동
    [Server]
    public void Moving()
    {
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(transform.position, 5f, playerLayer);

        if(hitPlayer.Length > 0)
        {
            target = hitPlayer[0].transform; // 가장 가까운 플레이어 선택

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

            // 스턴 상태가 아닐 때만 syncIsMoving 갱신
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
