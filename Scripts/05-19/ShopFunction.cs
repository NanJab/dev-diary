using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopFunction : MonoBehaviour
{
    public GameObject[] buyButtons; 

    public TextMeshProUGUI[] slotPrice;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PriceCheck();
    }

    public void PriceCheck()
    {
        for(int i = 0; i < buyButtons.Length; i++)
        {
            Item items = buyButtons[i].GetComponentInChildren<Item>();

            if(items != null)
            {
                slotPrice[i].text = items.itemPrice.ToString() + "원";
            }
        }
    }

    public void ShopClose()
    {
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-10000, 0, 0);
        GameManager.Instance.shopOnOff = false;
    }

    // 구매버튼을 누르면 플레이어에게 아이템 구매 정보를 등록
    // 네트워크 처리를 위해 플레이어 스크립트에서 실행
    public void BuyFunction(GameObject button)
    {
        Item item = button.GetComponentInChildren<Item>();

        var player = NetworkClient.connection.identity;
        if(player != null)
        {
            PlayerStat playerStat = player.GetComponent<PlayerStat>();
            int totalPrice = item.itemPrice * item.amount;

            if(playerStat != null && playerStat.money >= totalPrice)
            {
                playerStat.BuyItem(item.itemType, totalPrice, item.amount);
            }
        }
        
        Debug.Log(item.name);
    }
}
