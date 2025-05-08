using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopFunction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShopClose()
    {
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-10000, 0, 0);
        GameManager.Instance.shopOnOff = false;
    }

    public void BuyFunction(GameObject button)
    {
        Debug.Log(button.name);
    }
}
