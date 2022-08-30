using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class CameraTracker : MonoBehaviour
{
    public Vector3 cameraOffset;
    public Vector3 cameraOffsetAtTrap;
    public Vector3 rotationOffset;
    public Vector3 rotationOffsetAtTrap;
    public float offsetChangeDuration = 2.0f;

    public CameraSpline cameraSpline;
    public float speed = 1.0f;
    public float speedAtTrap = 0.6f;
    public float accelerationTime = 2.0f;

    private GameObject _currTracker;
    private int _cyclistPoint = 0;
    private bool _atTrap = false;
    Quaternion _rotationOffset;
    Quaternion _rotationOffsetAtTrap;
    Rigidbody _rigidbody;
    
    void Start()
    {
        StartCoroutine(CheckCyclists());

        _rotationOffset = Quaternion.Euler(rotationOffset);
        _rotationOffsetAtTrap = Quaternion.Euler(rotationOffsetAtTrap);

        cameraSpline.Speed = 0f;
        _rigidbody = GetComponent<Rigidbody>();
        cameraSpline.CameraOffset = cameraOffset;
        cameraSpline.RotationOffset = _rotationOffset;
        StartCoroutine(cameraSpline.MoveAlongSpline(_rigidbody, transform.GetChild(0)));
    }

    void Update() {
        if(GameManager.instance.IsFinished)
            return;

        if(_currTracker == null || _currTracker.tag.Equals("CyclistDowned") || _currTracker.GetComponent<CyclistMovement>() == null || _currTracker.GetComponent<CyclistMovement>().completed) {
            if(!UpdateTracking())
                return;
        }
        
        if(cameraSpline.Speed == 0f) {
            return;
        }

        if(_atTrap)
            return;
        if(_cyclistPoint < cameraSpline.GetPosition()) {
            StartCoroutine(UpdateSpeed(speedAtTrap));
        } else if(_cyclistPoint > cameraSpline.GetPosition() + 1) {
            StartCoroutine(UpdateSpeed(speed * 1.5f));
        } else {
            StartCoroutine(UpdateSpeed(speed));
        }
    }

    private IEnumerator CheckCyclists() {
        while(!GameManager.instance.IsFinished) {
            yield return new WaitForSeconds(0.5f);
            if(!UpdateTracking())
                break;
        }
    }

    private bool UpdateTracking() {
        GameObject newCyclist = GameManager.instance.GetClosestCyclist();

        if(_currTracker == null) {
            _currTracker = newCyclist;
        
            if(_currTracker == null) {
                StopCamera();
                return false;
            }

            //return true;
        }

        if(newCyclist != _currTracker)  {
            _currTracker = newCyclist;

            if(_currTracker == null) {
                StopCamera();
                return false;
            }
        }
        _cyclistPoint = cameraSpline.GetNearestPoint(_currTracker.transform.position);

        return true;
    }

    private IEnumerator UpdateSpeed(float newSpeed)
    {
        float time = 0f;
        float currSpeed = cameraSpline.Speed;

        while (time < 1)
        {
            time += 0.05f / accelerationTime;

            cameraSpline.Speed = Mathf.Lerp(currSpeed, newSpeed, time);

            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator UpdateOffset(Vector3 offset, Quaternion rotationOffset)
    {
        float time = 0f;
        Vector3 off = cameraSpline.CameraOffset;
        Quaternion rot = cameraSpline.RotationOffset;
        while (time < 1)
        {
            cameraSpline.CameraOffset = Vector3.Lerp(off, offset, time);
            cameraSpline.RotationOffset = Quaternion.Lerp(rot, rotationOffset, time);

            time += Time.fixedDeltaTime / offsetChangeDuration;
            yield return new WaitForFixedUpdate();
        }
    }

    public void EnteredTrap() {
        _atTrap = true;
        StartCoroutine(UpdateSpeed(speedAtTrap));
        StartCoroutine(UpdateOffset(cameraOffsetAtTrap, _rotationOffsetAtTrap));
    }

    public void StopTrack()
    {
        _atTrap = true;
        StartCoroutine(UpdateSpeed(0));
    }

    public void ExitedTrap() {
        _atTrap = false;
        StartCoroutine(UpdateSpeed(speed));
        StartCoroutine(UpdateOffset(cameraOffset, _rotationOffset));
    }

    public void StopCamera() {
        GameManager.instance.IsFinished = true;
    }
}
