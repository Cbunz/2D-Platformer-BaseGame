using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour {

    public InventoryController playerInventory;
    public GameObject coinPrefab;
    public GameObject coinCountPrefab;

    private Text coinText;


    private void Start()
    {
        // yield return null;

        GameObject coinIcon = Instantiate(coinPrefab);
        coinIcon.transform.SetParent(transform);
        RectTransform coinIconRect = coinIcon.transform as RectTransform;
        coinIconRect.anchoredPosition = Vector2.zero;
        coinIconRect.sizeDelta = Vector2.zero;

        GameObject coinCount = Instantiate(coinCountPrefab);
        coinCount.transform.SetParent(transform);
        RectTransform coinTextRect = coinCount.transform as RectTransform;
        coinTextRect.anchoredPosition = Vector2.zero;
        coinTextRect.sizeDelta = Vector2.zero;

        coinText = coinCount.GetComponent<Text>();
    }

    private void Update()
    {
        if (playerInventory.NumberOfItem("coin") <= 0)
            coinText.text = "000";
        else
            coinText.text = playerInventory.NumberOfItem("coin") < 10 ? "00" + (playerInventory.NumberOfItem("coin").ToString()) : (playerInventory.NumberOfItem("coin") < 100 ? "0" + (playerInventory.NumberOfItem("coin").ToString()) : (playerInventory.NumberOfItem("coin").ToString()));
    }
}
