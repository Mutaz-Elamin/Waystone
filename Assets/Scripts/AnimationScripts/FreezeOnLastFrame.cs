using UnityEngine;

public class FreezeOnLastFrame : StateMachineBehaviour
{
    public string boolParameter = "IsDefending"; // Animator bool to check

    private bool frozen = false;

    // Called each frame the state is active
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.GetBool(boolParameter))
        {

            animator.speed = 1f;
            frozen = false;
            return;
        }

        if (!frozen && stateInfo.normalizedTime >= 1f)
        {
 
            animator.speed = 0f;
            frozen = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = 1f;
        frozen = false;
    }
}