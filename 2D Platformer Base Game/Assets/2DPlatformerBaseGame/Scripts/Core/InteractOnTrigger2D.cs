using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractOnTrigger2D : MonoBehaviour {

    public LayerMask layers;
    public UnityEvent OnEnter, OnExit;
    public InventoryController.InventoryChecker[] inventoryChecks;

    protected Collider2D _collider;

    private void Reset()
    {
        layers = LayerMask.NameToLayer("Everything");
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled)
            return;

        if (layers.Contains(other.gameObject))
            ExecuteOnEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!enabled)
            return;

        if (layers.Contains(other.gameObject))
            ExecuteOnExit(other);
    }

    protected virtual void ExecuteOnEnter(Collider2D other)
    {
        OnEnter.Invoke();
        for (int i = 0; i < inventoryChecks.Length; i++)
        {
            inventoryChecks[i].CheckInventory(other.GetComponentInChildren<InventoryController>());
        }
    }

    protected virtual void ExecuteOnExit(Collider2D other)
    {
        OnExit.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);
    }
}
