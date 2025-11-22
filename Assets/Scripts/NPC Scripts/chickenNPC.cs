using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ChickenNPC : PassiveNPC
{
    private Animator ChickenAnimator;

    protected override void Awake()
    {
        base.Awake();
        ChickenAnimator = GetComponent<Animator>();
    }

    protected override void WanderMovementScript()
    {
        if (!desPointSet)
        {
            ChickenAnimator.SetBool("Walking", false);
        }
        else 
        {
            ChickenAnimator.SetFloat("MoveSpeed", 1f);
            ChickenAnimator.SetBool("Walking", true);
        }
        base.WanderMovementScript();
    }

    protected override void EscapeMovementScript()
    {
        ChickenAnimator.SetBool("Walking", true);
        ChickenAnimator.SetFloat("MoveSpeed", escapeMoveModifier);
        base.EscapeMovementScript();
    }
}
