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
            slotItem.Use();
            Debug.Log("붕대사용");
        }
        Debug.Log($"사용한 슬롯 번호 : {index + 1}");
    }

    public void UseAidKit(Item slotItem, int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (slotItem != null)
        {
            slotItem.Use();
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
        for (int i = 0; i < slots.Length; i++)
        {
            Item existing = slots[i].GetComponentInChildren<Item>();

            // 같은 아이템 타입이고, 최대 수량 미만이면 누적
            if (existing != null &&
                existing.itemType == item.itemType &&
                existing.count < existing.maxCount)
            {
                // 남은 개수 계산
                int spaceLeft = existing.maxCount - existing.count;

                // 공간이 충분한 경우 전부 넣고, 넘겨받은 건 삭제
                if (spaceLeft >= item.amount)
                {
                    existing.count += item.amount; // 기존에 누적
                    Destroy(item.gameObject);      // 새 아이템은 필요 없으므로 삭제
                    return;
                }
                else
                {
                    // 일부만 누적하고 나머지는 그대로 두기
                    existing.count += spaceLeft;    // 가능한 만큼만 누적
                    item.amount -= spaceLeft;       // 남은 양은 item에 유지
                    item.count = item.amount;       // 아이템 오브젝트에도 실제 개수 반영
                }
            }
        }

        // 위에서 누적이 안 되었거나 일부만 누적된 경우, 빈 슬롯을 찾아 등록
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponentInChildren<Item>() == null)
            {
                item.transform.SetParent(slots[i].transform);   // 빈 슬롯에 등록
                item.transform.localPosition = Vector3.zero;    // 위치 초기화
                item.count = item.amount;                       // 현재 개수 반영
                return;
            }
        }

        // 모든 슬롯이 가득 찼을 경우
        Debug.LogWarning("인벤토리가 가득 찼습니다.");
        Destroy(item.gameObject); // 처리를 못하면 제거 (또는 월드에 떨구기)
    }
}
