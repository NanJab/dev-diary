using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    Bat,        // �߱���Ʈ
    Shovel,     // ��
    Sword,      // ��
    BeamSword   // ������
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
    // ���� ��� ��������
    private Quaternion targetRotation = Quaternion.Euler(0, 0, -110);

    // ���� ���� ��� (��� Ŭ���̾�Ʈ���� �����ϰ� ���)
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
        //if (!isOwned) return; // ���� ĳ���Ͱ� �ƴϸ� ���� x

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

        // ��� Ŭ���̾�Ʈ�� ���� ���� �˸�
        RpcStartAttack();
    }

    [ClientRpc]
    void RpcStartAttack()
    {
        // ���� �÷��̾ �̹� ó�������Ƿ� ����
        if (isLocalPlayer) return;

        weaponPos.moveCheck = true;
        // ȸ�� �ʱ�ȭ
        weaponPivot.localRotation = originalRotation;
    }

    private void HandleAttack()
    {
        // ��� �ð� ��� (NetworkTime ���)
        float elapsedTime = (float)NetworkTime.time - attackStartTime;

        // ���� ���൵ (0~1 ����)
        float progress = Mathf.Clamp01(elapsedTime / ATTACK_DURATION);

        weaponPivot.localRotation = Quaternion.Slerp(originalRotation, targetRotation, progress * 2);

        // ���������� ������ ����     ��� �߰��� ������ ����
        if (isServer && !dmgCheck && elapsedTime >= DAMAGE_TIMING)
        {
            WeaponAttackDamage weaponDamage = GetComponentInChildren<WeaponAttackDamage>();
            weaponDamage.CheckHit(); // �� Ÿ��

            // ���� Ÿ�Ժ� �߰� ȿ��
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

        // ���� ���� ó��
        if (elapsedTime > ATTACK_DURATION)
        {
            // ���������� ���� ���� (SyncVar�� Ŭ���̾�Ʈ�� ����)
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
