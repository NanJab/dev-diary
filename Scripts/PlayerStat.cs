using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : NetworkBehaviour
{
    private Move move;
    public WeaponAttackMotion equippedWeapon;
    [SyncVar(hook = nameof(OnChangeHealth))]
    public int currentHealth;
    [SyncVar]
    public int maxHealth;
    [SyncVar(hook = nameof(OnStaminaChanged))]
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

    [Header("스태미나 수치")]
    [SyncVar]
    public float staDecreaseAmount;
    [SyncVar]
    public float staRecoveryAmount;
    [SyncVar]
    public float rollStamina;

    public bool isRolling = false;

    public float regenStaTimer = 0f;
    private float regenStaColldown = 0.2f;
    private bool isRecovering = false;

    [Header("체력바")]
    public WorldHpBar worldHpBar;
    public MyHpBar localHpBar;
    public MyStaminaBar localStaBar;

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

            if(localStaBar == null)
            {
                GameObject staUIObj = GameObject.FindWithTag("StaUI");
                if(staUIObj != null)
                {
                    localStaBar = staUIObj.GetComponent<MyStaminaBar>();
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

            move = GetComponent<Move>();

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

    void Update()
    {
        if (!isLocalPlayer) return;

        if(Input.GetMouseButtonDown(0) && equippedWeapon != null)
        {
            CmdAttack();
        }

        if(move.isRunning || isRolling)
        {
            isRecovering = false;
            regenStaTimer = 0f;
            CmdUseSta(staDecreaseAmount, 0f, rollStamina);
        }

        // 스태미나 소모 멈춘 후, 회복 준비 상태일 때만 회복 시작
        if(!isRecovering)
        {
            regenStaTimer += Time.deltaTime;

            if(regenStaTimer >= regenStaColldown)
            {
                isRecovering = true;
                regenStaTimer = 0f;
            }
        }

        if(isRecovering)
        {
            CmdUseSta(0f, staRecoveryAmount, 0f);

            if(currentStamina >= maxStamina)
            {
                isRecovering = false;
            }
        }
    }

    [Command]
    void CmdUseSta(float decreaseAmount, float recoveryAmount, float rollStamina)
    {
        StaUse(decreaseAmount, recoveryAmount, rollStamina);
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

    void UpdateSta()
    {
        float percent = currentStamina / maxStamina;

        if(localStaBar != null && isLocalPlayer)
        {
            localStaBar.setSta(percent);
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
    public void StaUse(float decrease, float recovery, float rollStamina)
    {
        if (move == null) return;

        if (decrease > 0f && move.isRunning && currentStamina > 0)
        {
            currentStamina -= decrease;
            currentStamina = Mathf.Max(currentStamina, 0f);

            if (currentStamina <= 0f)
            {
                move.SetStaminaBlocked(true);
            }
        }
        else if (recovery > 0f && !move.isRunning && currentStamina < maxStamina)
        {
            currentStamina += recovery;
            currentStamina = Mathf.Min(currentStamina, maxStamina);            
        }

        if(isRolling)
        {
            currentStamina -= rollStamina;
            currentStamina = Mathf.Max(currentStamina, 0f);
            isRolling = false;
        }

        if (currentStamina >= 10f)
        {
            move.SetStaminaBlocked(false);
        }
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

    void OnStaminaChanged(float oldSta, float newSta)
    {
        UpdateSta();
    }
}
