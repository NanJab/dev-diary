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
    public int def; // ����
    [SyncVar(hook = nameof(OnHungerChanged))]
    public int hunger; // �����
    [SyncVar(hook = nameof(OnThreatChanged))]
    public float threatRate; // ���赵
    [SyncVar(hook = nameof(OnMoneyChanged))]
    public int money;

    [Header("���¹̳� ��ġ")]
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

    [Header("ü�¹�")]
    public WorldHpBar worldHpBar;
    public MyHpBar localHpBar;
    public MyStaminaBar localStaBar;

    [Header("����� ���� �ý��� ����")]
    public double hungerDecreaseInterval;  // ������� �����ϴ� �ð� ����
    public int hungerDecreaseAmount = 1;        // ������� �����ϴ� ��
    public double healthDecreaseInterval;   // ������� 0�� �� ü���� �����ϴ� �ð� ����
    public int healthDecreaseAmount = 1;

    private double nextHungerDecrease; // ���� ����� ���� �ð�
    private double nextHealthDecrease; // ���� ü�� ���� �ð�

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

        // ui �ʱ�ȭ 
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

        // ���¹̳� �Ҹ� ���� ��, ȸ�� �غ� ������ ���� ȸ�� ����
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

    // ü�� ���� ����
    void OnChangeHealth(int oldHealth, int newHealth)
    {
        UpdateHpUI();

        if (newHealth <= 0)
        {
            UnityEngine.Debug.Log("ĳ���� ������");

            if(isServer)
            {
                RpcPlayDeathEffect(); // Ŭ���̾�Ʈ���� ��� ȿ�� ����
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    // ���� ��ġ ��ȭ ���� �Լ�
    void OnDefChanged(int oldValue, int newValue)
    {       
        // ���� ��ȭ�� ���� �߰� ����
    }

    // ������ ��ġ ��ȭ ���� �Լ�
    void OnThreatChanged(float oldValue, float newValue)
    {
        // ������ UI ������Ʈ �� �߰� ����
        // ��: �������� Ư�� ��ġ�� ������ �ֺ� AI ������ ��� ���� ����
    }

    // ����� ��ġ ��ȭ ���� �Լ�
    void OnHungerChanged(int oldValue, int newValue)
    {
        // ����� ��ġ�� ���� ui������Ʈ
        // 0�� �Ǹ� ü�� �ս� üũ
        // ���� ����� ����� ���� üũ
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

    // ������ ����
    // �����ݾ��� ������ ��� ������ ����
    [Server]
    public void BuyItem(ItemType itemType, int price, int amount)
    {        
        if(money >= price)
        {
            money -= price;
            TargetGiveItem(connectionToClient, itemType, amount);
        }
    }

    // ���ӸŴ����� ��ϵ� �����۰� ������ ��ϵ� �������� ���ϴ� itemCheck�Լ��� ���ӿ�����Ʈ�� �����´�.
    // ������ ������Ʈ�� �����ؼ� ������ ��ġ�� �°� ����ϴ� �Լ� ����
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
        // ��� �ִϸ��̼�, ȿ���� ���� ��� ����
        UnityEngine.Debug.Log("��� ����Ʈ ���");
    }

    // ü�� ���� Command �Լ�
    [Command]
    public void CmdIncreaseHealth(int amount)
    {
        if (currentHealth >= maxHealth) return;

        // ���������� �����
        currentHealth += amount;
    }

    [Command]
    public void CmdUpdateHunger(int amount)
    {
        // ����� ��ġ ��ȭ
        hunger += amount;
        // �ּ�/�ִ밪 ���� ����
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
        // 0~100 ���̷� �����ϴ� ���� ��
    }

    [Command]
    public void OnMoneyChanged(int oldMoney, int newMoney)
    {
        if(isLocalPlayer && GameManager.Instance.currentMoneyText != null)
        {
            GameManager.Instance.currentMoneyText.text = $"������ : {newMoney.ToString()}��";
        }
    }

    void OnStaminaChanged(float oldSta, float newSta)
    {
        UpdateSta();
    }
}
