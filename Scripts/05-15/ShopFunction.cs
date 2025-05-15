using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopFunction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShopClose()
    {
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-10000, 0, 0);
        GameManager.Instance.shopOnOff = false;
    }

    // ���Ź�ư�� ������ �÷��̾�� ������ ���� ������ ���
    // ��Ʈ��ũ ó���� ���� �÷��̾� ��ũ��Ʈ���� ����
    public void BuyFunction(GameObject button)
    {
        Item item = button.GetComponentInChildren<Item>();

        //Item it = GameManager.Instance.itemsCheck(item.itemType).GetComponent<Item>();

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
