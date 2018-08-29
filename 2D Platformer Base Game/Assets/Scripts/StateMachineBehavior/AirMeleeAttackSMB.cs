using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMeleeAttackSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.EnableMeleeAttack();
        if (monoBehaviour.dashWhileAirborne)
        {
            monoBehaviour.SetHorizontalMovement(monoBehaviour.meleeAttackDashSpeed * monoBehaviour.GetFacing());
        }
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Air Melee State");
        monoBehaviour.UpdateJump();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.AirVerticalMovement();
        monoBehaviour.CheckOnGround();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.DisableMeleeAttack();
    }
}
