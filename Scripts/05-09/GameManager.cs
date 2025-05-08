using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject shopUI;
    public bool shopOnOff = false;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        ShopControl();
    }

    public void ShopControl()
    {
        Vector3 offPosition = new Vector3(-10000, 0, 0);
        Vector3 onPosition = new Vector3(0, 0, 0);
        if(Input.GetKeyDown(KeyCode.E) && !shopOnOff)
        {
            shopUI.GetComponent<RectTransform>().transform.localPosition = onPosition;
            shopOnOff = true;
        }
        else if(Input.GetKeyDown(KeyCode.E) && shopOnOff)
        {
            shopUI.GetComponent<RectTransform>().transform.localPosition = offPosition;
            shopOnOff = false;
        }
    }
}
