using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CanvasGroup canvasGroup;
    public Canvas uiCanvas;

    Vector2 resetPosition;
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        // 상호작용을 차단하지 않도록 설정
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(uiCanvas.transform);
        resetPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        transform.position = mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if(eventData.pointerEnter && eventData.pointerEnter.GetComponent<InvenSlot>())
        {
            transform.SetParent(eventData.pointerEnter.transform);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            transform.position = resetPosition;
        }
    }

}
