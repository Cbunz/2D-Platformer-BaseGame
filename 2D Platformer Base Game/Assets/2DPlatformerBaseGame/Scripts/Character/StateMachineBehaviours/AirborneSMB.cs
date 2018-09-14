using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateFacing();
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckForRangedAttackOut();
        if (monoBehaviour.CheckForMeleeAttackInput())
        {
            monoBehaviour.MeleeAttack();
        }
        monoBehaviour.CheckAndFireGun();
    }
}
