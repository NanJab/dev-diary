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
    public int def; // 방어력
    [SyncVar]
    public int hunger; // 배고픔
    [SyncVar]
    public int thirsty; // 목마름
    [SyncVar]
    public float threatRate; // 위험도

    [Header("체력바")]
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

        // 로컬 플레이어일 경우, 태그로 체력바를 찾는다.
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

        // ui 초기화 
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

    // 체력 변경 감지
    void OnChangeHealth(int oldHealth, int newHealth)
    {
        UpdateHpUI();

        if (newHealth <= 0)
        {
            Debug.Log("캐릭터 쓰러짐");

            if(isServer)
            {
                RpcPlayDeathEffect(); // 클라이언트에서 사망 효과 실행
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
        // 사망 애니메이션, 효과음 등을 재생 가능
        Debug.Log("사망 이펙트 재생");
    }
}
