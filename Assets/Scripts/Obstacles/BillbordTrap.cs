using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BillbordTrap : Trap
{
    public Collider leftCollider;
    public Collider rightCollider;
    // Whether one of the legs have been destroyed. True = leg fallen.
    public bool HasFallen { get; private set; }

    private Animator _animator;
    private GameObject firedLeg;


    private void OnEnable()
    {
        
    }

    //HighlightController
    [SerializeField] TrapHighlightController _highlightController;


    private void Start() {
        _animator = GetComponent<Animator>();
    }

    public override void ResetTrap()
    {
        leftCollider.tag = "Trap";
        rightCollider.tag = "Trap";
        _animator.Rebind();
        _animator.Update(0f);
        _animator.enabled = false;
        firedLeg.SetActive(true);
        _highlightController.HighlightEnabled(true);
        HasFallen = false;
    }
    public override void TrapTriggered() { }

    // Plays the trap animation
    public void Toggle(GameObject obj) {
        ;
        
        HasFallen = true;
        firedLeg = obj;
        if(obj.name.Equals("RightLeg")) {
            rightCollider.tag = "ActiveTrap";
            _animator.enabled = true;
            _animator.Play("RightLeg");

            _highlightController.HighlightEnabled(false);
        } else {
            leftCollider.tag = "ActiveTrap";
            _animator.enabled = true;
            _animator.Play("LeftLeg");
            _highlightController.HighlightEnabled(false);
        }
        //firedLeg.SetActive(false);
    }
}
