using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractOnButton2D : InteractOnTrigger2D
{
    public UnityEvent OnButtonPress;

    bool canExecuteButtons;

    protected override void ExecuteOnEnter(Collider2D other)
    {
        canExecuteButtons = true;
        OnEnter.Invoke();
    }

    protected override void ExecuteOnExit(Collider2D other)
    {
        canExecuteButtons = false;
        OnExit.Invoke();
    }

    private void Update()
    {
        if (canExecuteButtons)
        {
            if (OnButtonPress.GetPersistentEventCount() > 0 && PlayerInput.Instance.Interact.Down)
                OnButtonPress.Invoke();
        }
    }
}
