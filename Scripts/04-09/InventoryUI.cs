using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;
    public int slotCount = 9;

    private GameObject[] slots;
    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[slotCount];

        InvenSlot[] invenSlots = new InvenSlot[slotCount];
        for(int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            slot.name = "Slot" + (i + 1);
            slots[i] = slot;
            invenSlots[i] = slot.GetComponent<InvenSlot>();
        }

        InventoryManager.Instance.RegisterSlots(invenSlots);
    }
}
