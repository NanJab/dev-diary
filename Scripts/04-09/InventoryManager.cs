using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // �̱���
    public InvenSlot[] slots; // ���� ���� ����Ʈ

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseItem(i);
            }
        }
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        InvenSlot slot = slots[index];

        Debug.Log($"����� ���� ��ȣ : {index + 1}");
    }

    public void RegisterSlots(InvenSlot[] slotArray)
    {
        slots = slotArray;
    }
}
