using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance { get; set; }

    public ShopFunction shop;

    [Header("공통 아이템")]
    public List<ItemType> commonItems = new List<ItemType>();
    [Header("개별 아이템")]
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

    // 서버가 시작될 때 실행
    public override void OnStartServer()
    {
        base.OnStartServer();
        // 서버에서만 InitShopItems 호출
        InitShopItems();
    }

    // 클라이언트가 연결될 때 실행
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
