using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ItemUI : MonoBehaviour {

    public InventoryController playerInventory;
    public GameObject iconPrefab;
    public GameObject countPrefab;
    public string itemKey;

    private Text text;


    private void Start()
    {
        // yield return null;

        GameObject icon = Instantiate(iconPrefab);
        icon.transform.SetParent(transform);
        RectTransform iconRect = icon.transform as RectTransform;
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = Vector2.zero;

        GameObject count = Instantiate(countPrefab);
        count.transform.SetParent(transform);
        RectTransform textRect = count.transform as RectTransform;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        text = count.GetComponent<Text>();
    }

    private void Update()
    {
        if (playerInventory.NumberOfItem(itemKey) <= 0)
            text.text = "000";
        else
            text.text = playerInventory.NumberOfItem(itemKey) < 10 ? "00" + (playerInventory.NumberOfItem(itemKey).ToString()) : (playerInventory.NumberOfItem(itemKey) < 100 ? "0" + (playerInventory.NumberOfItem(itemKey).ToString()) : (playerInventory.NumberOfItem(itemKey).ToString()));
    }
}
