using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : NetworkBehaviour
{
    public WeaponAttackMotion equippedWeapon;
    [SyncVar(hook = nameof(OnChangeHealth))]
    public int currentHealth;
    [SyncVar]
    public int maxHealth;
    [SyncVar]
    public float currentStamina;
    [SyncVar]
    public float maxStamina;
    [SyncVar]
    public int def; // ����
    [SyncVar]
    public int hunger; // �����
    [SyncVar]
    public int thirsty; // �񸶸�
    [SyncVar]
    public float threatRate; // ���赵

    [Header("ü�¹�")]
    public WorldHpBar worldHpBar;
    public MyHpBar localHpBar;

    // Start is called before the first frame update
    void Start()
    {
        //equippedWeapon = transform.GetComponentInChildren<WeaponAttackMotion>();

        if (isServer)
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }        

        // ���� �÷��̾��� ���, �±׷� ü�¹ٸ� ã�´�.
        if(isLocalPlayer)
        {
            if(localHpBar == null)
            {
                GameObject hpUIObj = GameObject.FindWithTag("HpUI");
                if(hpUIObj != null)
                {
                    localHpBar = hpUIObj.GetComponent<MyHpBar>();
                }
            }

            if (localHpBar != null)
            {
                localHpBar.gameObject.SetActive(true);                
            }

            if(worldHpBar != null)
            {
                worldHpBar.gameObject.SetActive(false);
            }
        }
        else
        {
            if(worldHpBar != null)
            {
                worldHpBar.gameObject.SetActive(true);
            }
        }

        // ui �ʱ�ȭ 
        UpdateHpUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if(Input.GetMouseButtonDown(0) && equippedWeapon != null)
        {
            CmdAttack();
        }
    }

    [Command]
    void CmdAttack()
    {
        equippedWeapon.ServerAttack();
    }

    // ü�� ���� ����
    void OnChangeHealth(int oldHealth, int newHealth)
    {
        UpdateHpUI();

        if (newHealth <= 0)
        {
            Debug.Log("ĳ���� ������");

            if(isServer)
            {
                RpcPlayDeathEffect(); // Ŭ���̾�Ʈ���� ��� ȿ�� ����
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    void UpdateHpUI()
    {
        float percent = (float)currentHealth / maxHealth;

        if(localHpBar != null && isLocalPlayer)
        {
            localHpBar.SetHp(percent);
        }

        if(worldHpBar != null)
        {
            worldHpBar.SetHp(percent);
        }
    }

    public void SetWeapon(WeaponAttackMotion weapon)
    {
        equippedWeapon = weapon;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
    }

    [ClientRpc]
    void RpcPlayDeathEffect()
    {
        // ��� �ִϸ��̼�, ȿ���� ���� ��� ����
        Debug.Log("��� ����Ʈ ���");
    }
}
