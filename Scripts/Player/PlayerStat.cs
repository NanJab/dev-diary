using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : NetworkBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 체력 변경 감지
    void OnChangeHealth(int oldHealth, int newHealth)
    {
        if(newHealth <= 0)
        {
            Debug.Log("캐릭터 쓰러짐");

            if(isServer)
            {
                RpcPlayDeathEffect(); // 클라이언트에서 사망 효과 실행
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    [Command]
    public void CmdTakeDamage(int damage)
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
