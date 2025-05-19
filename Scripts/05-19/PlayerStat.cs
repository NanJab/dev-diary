using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
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
    [SyncVar(hook = nameof(OnDefChanged))]
    public int def; // 방어력
    [SyncVar(hook = nameof(OnHungerChanged))]
    public int hunger; // 배고픔
    [SyncVar(hook = nameof(OnThreatChanged))]
    public float threatRate; // 위험도
    [SyncVar(hook = nameof(OnMoneyChanged))]
    public int money;

    [Header("스태미나 수치")]
    [SyncVar]
    public float staDecreaseAmount;
    [SyncVar]
    public float staRecoveryAmount;
    [SyncVar]
    public float rollStamina;

    public bool isRolling = false;

    public float regenStaTimer = 0f;
    private float regenStaColldown = 0.5f;
    private bool isRecovering = false;

    [Header("체력바")]
    public WorldHpBar worldHpBar;
    public MyHpBar localHpBar;
    public MyStaminaBar localStaBar;

    [Header("배고픔 생존 시스템 설정")]
    public double hungerDecreaseInterval;  // 배고픔이 감소하는 시간 간격
    public int hungerDecreaseAmount = 1;        // 배고픔이 감소하는 양
    public double healthDecreaseInterval;   // 배고픔이 0일 때 체력이 감소하는 시간 간격
    public int healthDecreaseAmount = 1;

    private double nextHungerDecrease; // 다음 배고픔 감소 시간
    private double nextHealthDecrease; // 다음 체력 감소 시간

    // Start is called before the first frame update
    void Start()
    {        
        //equippedWeapon = transform.GetComponentInChildren<WeaponAttackMotion>();
        if (isServer)
        {
            hungerDecreaseInterval = 0.5f;
            healthDecreaseInterval = 1f;
            hunger = 100;
            def = 10;
            threatRate = 0;
            currentHealth = maxHealth;
            currentStamina = maxStamina;

            nextHungerDecrease = NetworkTime.time + hungerDecreaseInterval;
            nextHealthDecrease = NetworkTime.time + healthDecreaseInterval;
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

    private void FixedUpdate()
    {
        if(isServer)
        {
            if(NetworkTime.time > nextHungerDecrease)
            {
                hunger = Mathf.Max(0, hunger - hungerDecreaseAmount);
                nextHungerDecrease = NetworkTime.time + hungerDecreaseInterval;
            }

            if(hunger <= 0 && NetworkTime.time > nextHealthDecrease)
            {
                TakeDamage(healthDecreaseAmount);
                nextHealthDecrease = NetworkTime.time + healthDecreaseInterval;
            }
        }
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
            CmdUseSta(staDecreaseAmount, staRecoveryAmount, rollStamina);

            if(currentStamina >= maxStamina)
            {
                isRecovering = false;
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        GameManager.Instance.localPlayer = gameObject;
        OnMoneyChanged(money, money);
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
            UnityEngine.Debug.Log("캐릭터 쓰러짐");

            if(isServer)
            {
                RpcPlayDeathEffect(); // 클라이언트에서 사망 효과 실행
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    // 방어력 수치 변화 감지 함수
    void OnDefChanged(int oldValue, int newValue)
    {       
        // 방어력 변화에 따른 추가 로직
    }

    // 위협도 수치 변화 감지 함수
    void OnThreatChanged(float oldValue, float newValue)
    {
        // 위협도 UI 업데이트 및 추가 로직
        // 예: 위협도가 특정 수치를 넘으면 주변 AI 적들의 경계 상태 변경
    }

    // 배고픔 수치 변화 감지 함수
    void OnHungerChanged(int oldValue, int newValue)
    {
        // 배고픔 수치에 따라 ui업데이트
        // 0이 되면 체력 손실 체크
        // 음식 섭취시 배고픔 증가 체크
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

    // 아이템 구매
    // 소지금액이 존재할 경우 아이템 구매
    [Server]
    public void BuyItem(ItemType itemType, int price, int amount)
    {        
        if(money >= price)
        {
            money -= price;
            TargetGiveItem(connectionToClient, itemType, amount);
        }
    }

    // 게임매니저에 등록된 아이템과 상점에 등록된 아이템을 비교하는 itemCheck함수로 게임오브젝트를 가져온다.
    // 가져온 오브젝트를 생성해서 슬롯의 위치에 맞게 등록하는 함수 실행
    [TargetRpc]
    public void TargetGiveItem(NetworkConnection target, ItemType itemType, int amount)
    {
        GameObject prefab = GameManager.Instance.itemsCheck(itemType);
        GameObject copy = Instantiate(prefab, InventoryManager.Instance.UICanvas.transform);
        Item copyItem = copy.GetComponent<Item>();
        copyItem.amount = amount;

        InventoryManager.Instance.RegisterItemSlot(copyItem);        
    }

    [ClientRpc]
    void RpcPlayDeathEffect()
    {
        // 사망 애니메이션, 효과음 등을 재생 가능
        UnityEngine.Debug.Log("사망 이펙트 재생");
    }

    // 체력 증가 Command 함수
    [Command]
    public void CmdIncreaseHealth(int amount)
    {
        if (currentHealth >= maxHealth) return;

        // 서버에서만 실행됨
        currentHealth += amount;
    }

    [Command]
    public void CmdUpdateHunger(int amount)
    {
        // 배고픔 수치 변화
        hunger += amount;
        // 최소/최대값 제한 로직
    }

    [Command]
    public void CmdUpdateDef(int amount)
    {
        def += amount;
    }

    [Command]
    public void CmdUpdateThreatRate(float amount)
    {
        threatRate += amount;
        // 0~100 사이로 제한하는 로직 등
    }

    [Command]
    public void OnMoneyChanged(int oldMoney, int newMoney)
    {
        if(isLocalPlayer && GameManager.Instance.currentMoneyText != null)
        {
            GameManager.Instance.currentMoneyText.text = $"소지금 : {newMoney.ToString()}원";
        }
    }

    void OnStaminaChanged(float oldSta, float newSta)
    {
        UpdateSta();
    }
}
