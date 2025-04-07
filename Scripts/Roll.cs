using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Roll : NetworkBehaviour
{
    public float rollSpeed = 5f;   // 구르기 속도
    public float rollDuration = 0.5f;  // 구르기 지속 시간
    public float rollCooldown = 1.0f;  // 구르기 쿨타임

    [SyncVar] private bool isRolling = false;
    [SyncVar]private Vector2 rollDirection;
    private Move move;
    private BoxCollider2D box;
    private PlayerStat stat;
    public Animator animator;

    [SyncVar]private double rollEndTime = 0f;  // 구르기 종료 시간
    [SyncVar]private double nextRollTime = 0f; // 다음 구르기 가능 시간

    void Start()
    {
        stat = GetComponent<PlayerStat>();
        box = GetComponent<BoxCollider2D>();
        move = GetComponent<Move>();
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;

        // 구르기가 종료되었는지 체크
        if (isRolling && NetworkTime.time >= rollEndTime)
        {
            CmdEndRoll();
        }

        // 구르기 입력 처리
        if (Input.GetKeyDown(KeyCode.Space) && !stat.isRolling)
        {
            TryRoll();
        }
    }

    private void TryRoll()
    {
        if (isRolling || NetworkTime.time < nextRollTime) return;

        if (stat.currentStamina < stat.rollStamina) return;

        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if(direction == Vector2.zero)
        {
            direction = Vector2.right * transform.localScale.x;
        }

        CmdStartRoll(direction);
    }

    [Command]
    private void CmdStartRoll(Vector2 direction)
    {
        if (isRolling || NetworkTime.time < nextRollTime) return;

        // 서버에서도 스태미나 확인
        if (stat.currentStamina < stat.rollStamina) return;

        // 최종 구르기 조건 통과 후만 상태 설정
        stat.isRolling = true;        

        isRolling = true;
        rollDirection = direction;
        rollEndTime = NetworkTime.time + rollDuration;
        nextRollTime = NetworkTime.time + rollCooldown; // 다음 구르기 가능 시간 설정

        RpcStartRoll();
    }

    [ClientRpc]
    private void RpcStartRoll()
    {
        animator.SetBool("isRolling", true);
        move.enabled = false;
        box.enabled = false;
    }

    [Command]
    private void CmdEndRoll()
    {
        if (!isRolling) return;
        isRolling = false;
        RpcEndRoll();
    }

    [ClientRpc]
    void RpcEndRoll()
    {
        animator.SetBool("isRolling", false);
        move.enabled = true;
        box.enabled = true;
    }
    void FixedUpdate()
    {
        if (isRolling)
        {
            transform.position += (Vector3)rollDirection * rollSpeed * Time.fixedDeltaTime;
        }
    }
}
