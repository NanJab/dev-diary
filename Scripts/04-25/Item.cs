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

    public void UseItem()
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
}
