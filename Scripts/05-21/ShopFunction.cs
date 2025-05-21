using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopFunction : MonoBehaviour
{
    public GameObject[] buyButtons;
    public GameObject[] shopSlots;

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
                slotPrice[i].text = items.itemPrice.ToString() + "��";
            }
        }
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

    public void ShopItemsToSlots(List<ItemType> item)
    {
        for(int i = 0; i < item.Count; i++)
        {
            GameObject prefab = GameManager.Instance.itemsCheck(item[i]);

            if (prefab == null) continue;

            GameObject itemUI = Instantiate(prefab, shopSlots[i].transform);
            itemUI.transform.localPosition = new Vector3(0, 115.6f, 0);
        }
       
    }
}
