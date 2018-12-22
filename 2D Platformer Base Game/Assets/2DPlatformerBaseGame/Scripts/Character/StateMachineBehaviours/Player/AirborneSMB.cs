using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.AirVerticalMovement();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.UpdateFacing();
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        if (monoBehaviour.CheckGrounded())
            monoBehaviour.AddLandLocation();
        monoBehaviour.CheckWallSlide();
        monoBehaviour.CheckForRangedAttackOut();
        if (monoBehaviour.CheckForBoostInput())
        {
            monoBehaviour.SetMoveVector(-(new Vector2(monoBehaviour.shield.boost.x * monoBehaviour.shield.boostAmountX, monoBehaviour.shield.boost.y * monoBehaviour.shield.boostAmountY)));
        }
        else if (monoBehaviour.CheckForMeleeAttackInput())
        {
            monoBehaviour.MeleeAttack();
        }
        monoBehaviour.CheckAndFireGun();
    }
}
