using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Bandage,
    AidKit,
}

public class Item : NetworkBehaviour
{
    public ItemType itemType;
    public int itemPrice;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // �ش���
    public void UseBandage()
    {
        NetworkIdentity localPlayerIdentity = NetworkClient.localPlayer;

        if(localPlayerIdentity != null)
        {
            PlayerStat health = localPlayerIdentity.GetComponent<PlayerStat>();

            if(health != null)
            {
                health.CmdIncreaseHealth(20);

                Destroy(gameObject);
            }
        }
    } 

    // ���޻��ڻ��
    public void UseAidKit()
    {
        NetworkIdentity localPlayerIdentity = NetworkClient.localPlayer;

        if(localPlayerIdentity != null)
        {
            PlayerStat health = localPlayerIdentity.GetComponent<PlayerStat>();

            if(health != null)
            {
                health.CmdIncreaseHealth(50);
                Debug.Log("�����̻� ġ��");

                Destroy(gameObject);
            }
        }
    }
}
