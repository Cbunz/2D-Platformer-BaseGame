using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.TeleportToColliderBottom();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Movement State");
        monoBehaviour.UpdateFacing();
        monoBehaviour.GroundHorizontalMovement(true);
        monoBehaviour.GroundVerticalMovement();
        monoBehaviour.CheckOnGround();
        if (monoBehaviour.CheckForJumpInput())
        {
            monoBehaviour.SetVerticalMovement(monoBehaviour.jumpSpeed);
        }
        else if (monoBehaviour.CheckForMeleeAttackInput())
        {
            Debug.Log("Melee Atack");
            monoBehaviour.MeleeAttack();
        }
    }
}
