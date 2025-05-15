using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; set; }// 싱글톤
    public Canvas UICanvas;
    public GameObject[] slots; // 슬롯 참조 리스트
    private int selectedSlotIndex = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // 이미 선택된 슬롯이면 아이템 사용
                if(selectedSlotIndex == i)
                {
                    Item slotItem = slots[selectedSlotIndex].GetComponentInChildren<Item>();
                    if (slotItem != null && slotItem.itemType == ItemType.Bandage)
                    {
                        UseBandage(slotItem, i);
                    }
                    else if(slotItem != null && slotItem.itemType == ItemType.AidKit)
                    {
                        UseAidKit(slotItem, i);
                    }
                }
                else
                {
                    // 새로운 슬롯이면 하이라이트 활성화
                    SelectSlot(i);
                }
            }
        }
    }

    public void UseBandage(Item slotItem, int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (slotItem != null)
        {
            slotItem.UseBandage();
            Debug.Log("포션사용");
        }
        Debug.Log($"사용한 슬롯 번호 : {index + 1}");
    }

    public void UseAidKit(Item slotItem, int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (slotItem != null)
        {
            slotItem.UseAidKit();
            Debug.Log("구급상자사용");
        }
        Debug.Log($"사용한 슬롯 번호 : {index + 1}");
    }

    private void SelectSlot(int index)
    {
        // 기존 선택된 슬롯 하이라이트 비활성화
        if(selectedSlotIndex >= 0 && selectedSlotIndex < slots.Length)
        {
            slots[selectedSlotIndex].GetComponent<InvenSlot>().SetHighlight(false);
        }

        // 새로운 슬롯 하이라이트 활성화
        slots[index].GetComponent<InvenSlot>().SetHighlight(true);
        selectedSlotIndex = index;
    }

    public void RegisterSlots(GameObject[] slotArray)
    {
        slots = slotArray;
    }

    // 들어온 아이템을 비어있는 슬롯의 자식으로 설정한다.
    public void RegisterItemSlot(Item item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponentInChildren<Item>() == null)
            {
                item.transform.SetParent(slots[i].transform);
                item.transform.localPosition = Vector3.zero;
                item.count += item.amount;

                Debug.Log($"아이템 {item.name} 슬롯 {i + 1}번에 등록됨");
                return;
            }
        }
    }
}
