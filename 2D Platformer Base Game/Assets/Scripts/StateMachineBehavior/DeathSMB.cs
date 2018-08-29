using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateIfno, int layerIndex)
    {
        monoBehaviour.SetMoveVector(monoBehaviour.GetHurtDirection() * monoBehaviour.hurtJumpSpeed);
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Death state");
        monoBehaviour.CheckOnGround();
        monoBehaviour.AirVerticalMovement();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.SetMoveVector(Vector2.zero);
    }
}