using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    Bat,        // 야구배트
    Shovel,     // 삽
    Sword,      // 검
    BeamSword   // 광선검
}

public class WeaponAttackMotion : NetworkBehaviour
{
    public WeaponType weaponType;
    public Transform weaponPivot;
    private BoxCollider2D bCollider;
    private WeaponPosition weaponPos;    
    public float attackSpeed;

    [SyncVar]
    public bool dmgCheck = false;
    [SyncVar]
    public bool attackCheck = false;
    [SyncVar]
    public float attackStartTime = 0;

    private Quaternion originalRotation;
    // 공격 모션 도착지점
    private Quaternion targetRotation = Quaternion.Euler(0, 0, -110);

    // 공격 관련 상수 (모든 클라이언트에서 동일하게 사용)
    private const float DAMAGE_TIMING = 0.2f;
    private const float ATTACK_DURATION = 0.5f;
    void Start()
    {
        bCollider = GetComponentInChildren<BoxCollider2D>();
        weaponPos = GetComponent<WeaponPosition>();
        originalRotation = weaponPivot.rotation;
    }

    void Update()
    {
        //if (!isOwned) return; // 본인 캐릭터가 아니면 실행 x

        if(Input.GetMouseButtonDown(0) && !attackCheck)
        {
            CmdAttack();
        }

        if(attackCheck)
        {
            HandleAttack();
        }

    }

    [Command]
    void CmdAttack()
    {
        if (attackCheck) return;

        attackCheck = true;
        dmgCheck = false;
        attackStartTime = (float)NetworkTime.time;
        weaponPos.moveCheck = true;

        // 모든 클라이언트에 공격 시작 알림
        RpcStartAttack();
    }

    [ClientRpc]
    void RpcStartAttack()
    {
        // 로컬 플레이어가 이미 처리했으므로 제외
        if (isLocalPlayer) return;

        weaponPos.moveCheck = true;
        // 회전 초기화
        weaponPivot.localRotation = originalRotation;
    }

    private void HandleAttack()
    {
        // 경과 시간 계산 (NetworkTime 사용)
        float elapsedTime = (float)NetworkTime.time - attackStartTime;

        // 공격 진행도 (0~1 범위)
        float progress = Mathf.Clamp01(elapsedTime / ATTACK_DURATION);

        weaponPivot.localRotation = Quaternion.Slerp(originalRotation, targetRotation, progress * 2);

        // 서버에서만 데미지 판정     모션 중간에 데미지 판정
        if (isServer && !dmgCheck && elapsedTime >= DAMAGE_TIMING)
        {
            WeaponAttackDamage weaponDamage = GetComponentInChildren<WeaponAttackDamage>();
            weaponDamage.CheckHit(); // 적 타격

            // 무기 타입별 추가 효과
            if (weaponType == WeaponType.Bat)
            {
                weaponDamage.KnockBack(2);
            }
            else if (weaponType == WeaponType.Shovel)
            {
                weaponDamage.Stun();
            }

            dmgCheck = true;
        }

        // 공격 종료 처리
        if (elapsedTime > ATTACK_DURATION)
        {
            // 서버에서만 상태 변경 (SyncVar로 클라이언트에 전파)
            if (isServer)
            {
                attackCheck = false;
                dmgCheck = false;
                weaponPos.moveCheck = false;

                RpcResetWeaponPosition();
            }          
        }
    }

    [ClientRpc]
    private void RpcResetWeaponPosition()
    {
        weaponPivot.localRotation = originalRotation;
        weaponPos.moveCheck = false; 
    }
}
