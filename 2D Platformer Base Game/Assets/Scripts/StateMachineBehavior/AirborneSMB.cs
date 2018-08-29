using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Airborne state");
        monoBehaviour.UpdateFacing();
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckOnGround();
        if (monoBehaviour.CheckForMeleeAttackInput())
        {
            Debug.Log("Melee Atack");
            monoBehaviour.MeleeAttack();
        }
    }
}
