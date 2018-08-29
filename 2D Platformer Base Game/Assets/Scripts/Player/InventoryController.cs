using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryController : MonoBehaviour, IDataPersister
{
    [System.Serializable]
    public class InventoryEvent
    {
        public string key;
        public UnityEvent OnAdd, OnRemove;
    }

    [System.Serializable]
    public class InventoryChecker
    {
        public string[] inventoryItems;
        public UnityEvent OnHasItem, OnDoesNotHaveItem;

        public bool CheckInventory(InventoryController inventory)
        {
            if (inventory != null)
            {
                for (var i = 0; i < inventoryItems.Length; i++)
                {
                    if (!inventory.HasItem(inventoryItems[i]))
                    {
                        OnDoesNotHaveItem.Invoke();
                        return false;
                    }
                }
                OnHasItem.Invoke();
                return true;
            }
            return false;
        }
    }

    public InventoryEvent[] inventoryEvents;
    public event System.Action OnInventoryLoaded;

    public DataSettings dataSettings;

    HashSet<string> inventoryItems = new HashSet<string>();

    [ContextMenu("Dump")]
    void Dump()
    {
        foreach (var item in inventoryItems)
        {
            Debug.Log(item);
        }
    }

    void OnEnable()
    {
        PersistentDataManager.RegisterPersister(this);
    }

    void OnDisable()
    {
        PersistentDataManager.UnregisterPersister(this);
    }

    public void AddItem(string item)
    {
        if (!inventoryItems.Contains(item))
        {
            inventoryItems.Add(item);
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
            {
                invEvent.OnAdd.Invoke();
            }
        }
    }

    public void RemoveItem(string item)
    {
        if (inventoryItems.Contains(item))
        {
            var invEvent = GetInventoryEvent(item);
            if (invEvent != null)
            {
                invEvent.OnRemove.Invoke();
            }
            inventoryItems.Remove(item);
        }
    }

    public bool HasItem(string item)
    {
        return inventoryItems.Contains(item);
    }

    public void Clear()
    {
        inventoryItems.Clear();
    }

    InventoryEvent GetInventoryEvent(string item)
    {
        foreach (var invEvent in inventoryEvents)
        {
            if (invEvent.key == item)
            {
                return invEvent;
            }
        }
        return null;
    }

    public DataSettings GetDataSettings()
    {
        return dataSettings;
    }

    public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
    {
        dataSettings.dataTag = dataTag;
        dataSettings.persistenceType = persistenceType;
    }

    public Data SaveData()
    {
        return new Data<HashSet<string>>(inventoryItems);
    }

    public void LoadData(Data data)
    {
        Data<HashSet<string>> inventoryData = (Data<HashSet<string>>)data;
        foreach (var item in inventoryData.value)
        {
            AddItem(item);
        }
        if (OnInventoryLoaded != null) OnInventoryLoaded();
    }
}
