using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; set; }// �̱���
    public Canvas UICanvas;
    public GameObject[] slots; // ���� ���� ����Ʈ
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
                // �̹� ���õ� �����̸� ������ ���
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
                    // ���ο� �����̸� ���̶���Ʈ Ȱ��ȭ
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
            Debug.Log("���ǻ��");
        }
        Debug.Log($"����� ���� ��ȣ : {index + 1}");
    }

    public void UseAidKit(Item slotItem, int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (slotItem != null)
        {
            slotItem.UseAidKit();
            Debug.Log("���޻��ڻ��");
        }
        Debug.Log($"����� ���� ��ȣ : {index + 1}");
    }

    private void SelectSlot(int index)
    {
        // ���� ���õ� ���� ���̶���Ʈ ��Ȱ��ȭ
        if(selectedSlotIndex >= 0 && selectedSlotIndex < slots.Length)
        {
            slots[selectedSlotIndex].GetComponent<InvenSlot>().SetHighlight(false);
        }

        // ���ο� ���� ���̶���Ʈ Ȱ��ȭ
        slots[index].GetComponent<InvenSlot>().SetHighlight(true);
        selectedSlotIndex = index;
    }

    public void RegisterSlots(GameObject[] slotArray)
    {
        slots = slotArray;
    }

    // ���� �������� ����ִ� ������ �ڽ����� �����Ѵ�.
    public void RegisterItemSlot(Item item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponentInChildren<Item>() == null)
            {
                item.transform.SetParent(slots[i].transform);
                item.transform.localPosition = Vector3.zero;
                item.count += item.amount;

                Debug.Log($"������ {item.name} ���� {i + 1}���� ��ϵ�");
                return;
            }
        }
    }
}
