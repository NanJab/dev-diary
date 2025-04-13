using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // �̱���
    public InvenSlot[] slots; // ���� ���� ����Ʈ
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
                    UseItem(i);
                }
                else
                {
                    // ���ο� �����̸� ���̶���Ʈ Ȱ��ȭ
                    SelectSlot(i);
                }
            }
        }
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        InvenSlot slot = slots[index];

        Debug.Log($"����� ���� ��ȣ : {index + 1}");
    }

    private void SelectSlot(int index)
    {
        // ���� ���õ� ���� ���̶���Ʈ ��Ȱ��ȭ
        if(selectedSlotIndex >= 0 && selectedSlotIndex < slots.Length)
        {
            slots[selectedSlotIndex].SetHighlight(false);
        }

        // ���ο� ���� ���̶���Ʈ Ȱ��ȭ
        slots[index].SetHighlight(true);
        selectedSlotIndex = index;
    }

    public void RegisterSlots(InvenSlot[] slotArray)
    {
        slots = slotArray;
    }
}
