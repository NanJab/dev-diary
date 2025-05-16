using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InvenSlot : MonoBehaviour, IDropHandler
{
    public GameObject highlightImage;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        Item droppedItem = droppedObj.GetComponent<Item>();
        Item existingItem = GetComponentInChildren<Item>();

        // 1. 같은 타입이면 스택
        if (existingItem != null && droppedItem.itemType == existingItem.itemType &&
            existingItem.stackAble && existingItem.count < existingItem.maxCount)
        {
            // 현재 수용가능한 공간
            int spaceLeft = existingItem.maxCount - existingItem.count;

            if (spaceLeft >= droppedItem.count)
            {
                // 옮긴 아이템 개수 적용
                existingItem.count += droppedItem.count;
                // 옮긴 아이템 오브젝트는 삭제
                Destroy(droppedObj);
            }
            else
            {
                // 현재 아이템의 개수를 최대치로 설정
                existingItem.count = existingItem.maxCount;
                // 옮긴 아이템의 개수를 현재 아이템의 개수에 추가한 만큼 삭제
                droppedItem.count -= spaceLeft;
            }
        }
        // 2. 아이템이 있고 타입이 다르면 → 스왑
        else if (existingItem != null)
        {
            // 옮긴 아이템의 위치를 현재 슬롯으로 이동
            droppedObj.transform.SetParent(existingItem.transform.parent);
            droppedObj.transform.localPosition = Vector3.zero;

            // 원래 존재하던 아이템의 위치를 옮긴 아이템의 원래 위치로 이동
            existingItem.transform.SetParent(droppedItem.GetComponent<ItemDragDrop>().originalParent);
            existingItem.transform.localPosition = Vector3.zero;
        }
        // 3. 슬롯이 비어있으면 → 그냥 넣기
        else
        {
            droppedObj.transform.SetParent(transform);
            droppedObj.transform.localPosition = Vector3.zero;
        }
    }

    public void SetHighlight(bool isOn)
    {
        if(highlightImage != null)
        {
            highlightImage.SetActive(isOn);
        }
    }
}
