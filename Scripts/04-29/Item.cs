using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 붕대사용
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

    // 구급상자사용
    public void UseAidKit()
    {
        NetworkIdentity localPlayerIdentity = NetworkClient.localPlayer;

        if(localPlayerIdentity != null)
        {
            PlayerStat health = localPlayerIdentity.GetComponent<PlayerStat>();

            if(health != null)
            {
                health.CmdIncreaseHealth(50);
                Debug.Log("상태이상 치료");

                Destroy(gameObject);
            }
        }
    }
}
