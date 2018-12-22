using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDispenser : MonoBehaviour
{
    InventoryController inventoryController;
    KeyToItem keyToItem;
    Launcher launcher;
    Vector2 dispenseVector;
    float dispenseAngle;
    bool dispense = false;
    SpriteRenderer spriteRenderer;
    Collider2D coll;
    Shatter shatter;
    bool canShatter = true;

    public bool shatterOn = true;
    public float shatterTime = 2f;
    public bool instantDispense = true;
    public float dispenseDeltaTime = 0f;
    public float dispenseSpeed = 10f;
    public float speedVariance = 5f;
    public float minDispenseAngle = 0.001f;
    public float maxDispenseAngle = 179.999f;
    public bool destroyAfterDispense = true;
    public Vector2 dispenseOffset = new Vector2();

    private void Awake()
    {
        inventoryController = GetComponent<InventoryController>();
        keyToItem = GetComponent<KeyToItem>();
        launcher = GetComponent<Launcher>();
        shatter = GetComponent<Shatter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (dispense)
        {
            if (shatterOn && canShatter)
            {
                shatter.shatter();
                canShatter = false;
            }
                
            StartCoroutine(Dispense());
            dispense = false;
        }
    }

    IEnumerator Dispense()
    {
        List<string> itemKeys = new List<string>();
        List<Rigidbody2D> rbList = new List<Rigidbody2D>();
        Vector2 dispenseLocation = new Vector2(transform.position.x + dispenseOffset.x, transform.position.y + dispenseOffset.y);

        foreach (KeyValuePair<string, int> item in inventoryController.inventoryItems)
        {
            for (int i = 0; i < item.Value; i++)
                itemKeys.Add(item.Key);
        }

        foreach (string key in itemKeys)
        {
            GameObject gameObject;
            gameObject = Instantiate(keyToItem.ConvertToPrefab(key), dispenseLocation, Quaternion.identity);
            rbList.Add(gameObject.GetComponent<Rigidbody2D>());
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = GetRandomDispenseDirection() * GetRandomDispenseSpeed();
            inventoryController.RemoveItem(key);
            if (!instantDispense)
                yield return new WaitForSeconds(dispenseDeltaTime);
        }

        if (shatterOn)
            yield return new WaitForSeconds(shatterTime);
        else
            yield return null;

        if (destroyAfterDispense)
            Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (launcher.Launched)
        {
            DispenseOnOff();
        }
    }

    public void DispenseOnOff()
    {
        dispense = !dispense;
    }

    float GetRandomDispenseSpeed()
    {
        float speedVariation = Random.Range(-speedVariance, speedVariance);
        return dispenseSpeed + speedVariation;
    }

    Vector2 GetRandomDispenseDirection()
    {
        dispenseAngle = Random.Range(minDispenseAngle, maxDispenseAngle);
        return new Vector2(Mathf.Cos(dispenseAngle * Mathf.Deg2Rad), Mathf.Sin(dispenseAngle * Mathf.Deg2Rad));
    }
}
