using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    public ParticleSystem confetti;
    public Animator animator;
    public GameObject arrow;
    private bool _doOnce = true;

    [SerializeField]private Outline _outline;

    private void Awake()
    {
        Time.timeScale = 0.0f;
    }

    private void OnMouseDown()
    {
        if (_doOnce)
        {
            _doOnce = false;
            Time.timeScale = 1.0f;
            animator.SetBool("Started", true);
            confetti.Play();
            arrow.SetActive(false);
            _outline.enabled = false;
        }
    }
}
