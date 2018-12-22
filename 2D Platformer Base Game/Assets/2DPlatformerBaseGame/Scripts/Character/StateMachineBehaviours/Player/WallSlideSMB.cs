using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSlideSMB : SceneLinkedSMB<PlayerCharacter>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.UpdateFacing();
        monoBehaviour.AirHorizontalMovement();
        monoBehaviour.CheckGrounded();
        monoBehaviour.CheckWallSlide();
        if (!monoBehaviour.IsFalling())
            monoBehaviour.WallSlideUpVerticalMovement();
        else
            monoBehaviour.WallSlideDownVerticalMovement();
        if (monoBehaviour.CheckForJumpInput() && monoBehaviour.TouchingWall())
        {
            Vector2 jumpVector;
            if (monoBehaviour.GetFacing() == -1)
            {
                jumpVector = new Vector2(monoBehaviour.wallSlideJumpX, monoBehaviour.wallSlideJumpY);
                monoBehaviour.ForceFacing(false);
            }
            else
            {
                jumpVector = new Vector2(-monoBehaviour.wallSlideJumpX, monoBehaviour.wallSlideJumpY);
                monoBehaviour.ForceFacing(true);
            }
            monoBehaviour.SetMoveVector(jumpVector);
            monoBehaviour.ForceNotWallSlide();
        }
    }

    public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monoBehaviour.ForceNotWallSlide();
    }
}