using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InvenSlot : MonoBehaviour, IDropHandler
{
    public GameObject highlightImage;

    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중인 오브젝트
        GameObject draggedObject = eventData.pointerDrag;

        if(draggedObject != null)
        {
            ItemDragDrop item = draggedObject.transform.GetComponent<ItemDragDrop>();

            if(item != null)
            {
                draggedObject.transform.position = transform.position;
            }
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
