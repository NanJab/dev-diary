using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // 싱글톤
    public InvenSlot[] slots; // 슬롯 참조 리스트
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
                    UseItem(i);
                }
                else
                {
                    // 새로운 슬롯이면 하이라이트 활성화
                    SelectSlot(i);
                }
            }
        }
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        InvenSlot slot = slots[index];

        Debug.Log($"사용한 슬롯 번호 : {index + 1}");
    }

    private void SelectSlot(int index)
    {
        // 기존 선택된 슬롯 하이라이트 비활성화
        if(selectedSlotIndex >= 0 && selectedSlotIndex < slots.Length)
        {
            slots[selectedSlotIndex].SetHighlight(false);
        }

        // 새로운 슬롯 하이라이트 활성화
        slots[index].SetHighlight(true);
        selectedSlotIndex = index;
    }

    public void RegisterSlots(InvenSlot[] slotArray)
    {
        slots = slotArray;
    }
}
