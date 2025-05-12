using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour
{
    public LayerMask playerMask;
    public GameObject buttonUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShopEnter();
    }

    public void ShopEnter()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 2f, playerMask);

        bool playerCheck = players.Length > 0;

        if(playerCheck)
        {
            buttonUI.SetActive(true);
            if(Input.GetKeyDown(KeyCode.E))
            {
                GameManager.Instance.ShopControl();
            }
        }
        else
        {
            buttonUI.SetActive(false);
        }
    }
}
