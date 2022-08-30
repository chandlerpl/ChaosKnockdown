using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovingObstacle : Trap
{
    public float speed = 1;
    public Vector3 _endPos;
    public MeshRenderer meshRenderer;
    public Material untriggerableMat;

    private bool reverse = true;
    private float _time = 0;
    private Vector3 _startPos;
    private bool isFired = false;

    private void Start() {
        _startPos = transform.position;
    }
    public override void ResetTrap() { }

    public override void TrapTriggered()
    {
        meshRenderer.material = untriggerableMat;

        if (!isFired)
        {
            isFired = true;
            StartCoroutine(Move());
        }
    }
    IEnumerator Move() {
        while(true) {
            float updateTime = 0.02f / speed;

            // Reverses the direction that the object is moving once it reaches its destination
            if(_time > 1)
                reverse = false;
            else if(_time < 0)
                reverse = true;

            if(reverse) {
                _time += updateTime;
            } else {
                _time -= updateTime;
            }
            transform.position = Vector3.Lerp(_startPos, _endPos, _time); // Updates the position using linear interpolation.

            yield return new WaitForSeconds(0.02f);
        }
    }
}
