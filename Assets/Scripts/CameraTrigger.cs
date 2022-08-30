using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public CameraTracker cameraTracker;
    public bool isStart = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "CameraTracker" || (isStart && other.tag == "Cyclist"))
        {
            if(tag.Equals("FinishLine"))
            {
                cameraTracker.StopCamera();
            } else if(tag.Equals("SpeedTriggerEnter")) 
            {
                cameraTracker.EnteredTrap();
            } else 
            {
                if(isStart)
                {
                    cameraTracker.ExitedTrap();
                } else
                {
                    cameraTracker.StopTrack();
                }
            }

            gameObject.SetActive(false);
        }
    }
}

