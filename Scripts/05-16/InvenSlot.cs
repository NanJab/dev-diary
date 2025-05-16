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

        // 1. ���� Ÿ���̸� ����
        if (existingItem != null && droppedItem.itemType == existingItem.itemType &&
            existingItem.stackAble && existingItem.count < existingItem.maxCount)
        {
            // ���� ���밡���� ����
            int spaceLeft = existingItem.maxCount - existingItem.count;

            if (spaceLeft >= droppedItem.count)
            {
                // �ű� ������ ���� ����
                existingItem.count += droppedItem.count;
                // �ű� ������ ������Ʈ�� ����
                Destroy(droppedObj);
            }
            else
            {
                // ���� �������� ������ �ִ�ġ�� ����
                existingItem.count = existingItem.maxCount;
                // �ű� �������� ������ ���� �������� ������ �߰��� ��ŭ ����
                droppedItem.count -= spaceLeft;
            }
        }
        // 2. �������� �ְ� Ÿ���� �ٸ��� �� ����
        else if (existingItem != null)
        {
            // �ű� �������� ��ġ�� ���� �������� �̵�
            droppedObj.transform.SetParent(existingItem.transform.parent);
            droppedObj.transform.localPosition = Vector3.zero;

            // ���� �����ϴ� �������� ��ġ�� �ű� �������� ���� ��ġ�� �̵�
            existingItem.transform.SetParent(droppedItem.GetComponent<ItemDragDrop>().originalParent);
            existingItem.transform.localPosition = Vector3.zero;
        }
        // 3. ������ ��������� �� �׳� �ֱ�
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
