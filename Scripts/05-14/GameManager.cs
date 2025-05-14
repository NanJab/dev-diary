using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject[] items;
    public GameObject shopUI;
    public bool shopOnOff = false;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //ShopControl();
    }

    public void ShopControl()
    {
        Vector3 offPosition = new Vector3(-10000, 0, 0);
        Vector3 onPosition = new Vector3(0, 0, 0);
        if(!shopOnOff)
        {
            shopUI.GetComponent<RectTransform>().transform.localPosition = onPosition;
            shopOnOff = true;
        }
        else
        {
            shopUI.GetComponent<RectTransform>().transform.localPosition = offPosition;
            shopOnOff = false;
        }
    }

    // 구매하려는 상점 아이템과 프리팹의 아이템이 같을 때 아이템 생성
    public GameObject itemsCheck(ItemType itemType)
    {
        for(int i = 0; i < items.Length; i++)
        {
            Item itemComponent = items[i].GetComponent<Item>();
            if (itemComponent != null && itemComponent.itemType == itemType)
            {
                GameObject newItem = items[i];
                return newItem;
            }
        }
        return null;
    }
}
