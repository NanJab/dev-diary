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
    public int def; // ����
    [SyncVar]
    public int hunger; // �����
    [SyncVar]
    public int thirsty; // �񸶸�
    [SyncVar]
    public float threatRate; // ���赵

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

    // ü�� ���� ����
    void OnChangeHealth(int oldHealth, int newHealth)
    {
        if(newHealth <= 0)
        {
            Debug.Log("ĳ���� ������");

            if(isServer)
            {
                RpcPlayDeathEffect(); // Ŭ���̾�Ʈ���� ��� ȿ�� ����
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
        // ��� �ִϸ��̼�, ȿ���� ���� ��� ����
        Debug.Log("��� ����Ʈ ���");
    }
}
