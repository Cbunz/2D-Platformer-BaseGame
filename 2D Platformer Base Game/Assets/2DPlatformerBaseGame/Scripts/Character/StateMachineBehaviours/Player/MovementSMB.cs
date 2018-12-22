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
        monoBehaviour.UpdateFacing();
        monoBehaviour.GroundHorizontalMovement(true);
        monoBehaviour.GroundVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckForPushing();
        monoBehaviour.CheckForRangedAttackOut();
        monoBehaviour.CheckAndFireGun();
        if (monoBehaviour.CheckForBoostInput())
        {
            monoBehaviour.SetMoveVector(-(new Vector2((monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX), monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
        }
        else if (monoBehaviour.CheckForJumpInput())
        {
            monoBehaviour.AddJumpLocation();
            monoBehaviour.SetVerticalMovement(monoBehaviour.jumpSpeed);
        }
        else if (monoBehaviour.CheckForMeleeAttackInput())
        {
            monoBehaviour.MeleeAttack();
        }
    }
}