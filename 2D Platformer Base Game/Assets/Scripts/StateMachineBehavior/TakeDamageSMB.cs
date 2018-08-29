using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.SetMoveVector(monoBehaviour.GetHurtDirection() * monoBehaviour.hurtJumpSpeed);
        monoBehaviour.StartFlickering();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Take Damage State");
        if (monoBehaviour.IsFalling())
        {
            monoBehaviour.CheckOnGround();
        }
        monoBehaviour.AirVerticalMovement();
    }
}
