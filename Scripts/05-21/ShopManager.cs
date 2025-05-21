using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance { get; set; }

    public ShopFunction shop;

    [Header("���� ������")]
    public List<ItemType> commonItems = new List<ItemType>();
    [Header("���� ������")]
    public List<GameObject> randomItems = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ������ ���۵� �� ����
    public override void OnStartServer()
    {
        base.OnStartServer();
        // ���������� InitShopItems ȣ��
        InitShopItems();
    }

    // Ŭ���̾�Ʈ�� ����� �� ����
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("ShopManager: OnStartServer called");
        InitShopItems();
    }

    [Server]
    public void InitShopItems()
    {
        Debug.Log("InitShopItems called");

        RpcCommonItems(commonItems);
    }

    [ClientRpc]
    public void RpcCommonItems(List<ItemType> items)
    {
        Debug.Log("RpcCommonItems called");
        shop.ShopItemsToSlots(items);
    }
}
