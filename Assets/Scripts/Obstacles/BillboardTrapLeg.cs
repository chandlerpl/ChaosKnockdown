using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardTrapLeg : Trap
{
    public BillbordTrap controller;
    public GameObject particleEffect;
    public Animator camAnimator;

    public override void ResetTrap() { }

    public override void TrapTriggered()
    {
        if (!controller.HasFallen)
        {
            controller.Toggle(gameObject);
            //spawn particle effect
            Instantiate(particleEffect, transform.position, Quaternion.identity);
            //camshake
            camAnimator.Play("shake");
            //hide UI prompt
        }
    }
}
