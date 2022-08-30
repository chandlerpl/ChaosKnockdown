using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTrap : Trap
{
    public Animator animator;
    public Animator camAnimator;
    public GameObject particleEffect;
    public Animator pusherAnimator;

    //HighlightController
    [SerializeField] TrapHighlightController _highlightController;

    public override void ResetTrap()
    {
        pusherAnimator.SetBool("Pushed", false);
        _highlightController.HighlightEnabled(true);
    }
    public override void TrapTriggered()
    {
        Instantiate(particleEffect, transform.position, Quaternion.identity);
        camAnimator.Play("shake");
        animator.enabled = true;
        pusherAnimator.SetBool("Pushed", true);
        animator.Rebind();
        animator.Update(0f);
        _highlightController.HighlightEnabled(false);
    }
}
