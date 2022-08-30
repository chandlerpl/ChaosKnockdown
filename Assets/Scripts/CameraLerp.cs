using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerp : MonoBehaviour
{
    public Transform[] posTransforms;
    private int posInt = 1;

    void Start()
    {
        //StartCoroutine(TimedEvents());
    }

    public void ChangePosition()
    {
        StartCoroutine(LerpPosition(posTransforms[posInt], 0.5f));
        posInt++;
    }

    IEnumerator LerpPosition(Transform transformTarget, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, transformTarget.position, time / duration);
            transform.rotation = Quaternion.Lerp(startRotation, transformTarget.rotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = transformTarget.position;
        transform.rotation = transformTarget.rotation;
    }

    IEnumerator TimedEvents()
    {
        StartCoroutine(LerpPosition(posTransforms[0], 4));
        yield return new WaitForSeconds(11.0f);
        StartCoroutine(LerpPosition(posTransforms[1], 3));
        yield return new WaitForSeconds(12.0f);
        StartCoroutine(LerpPosition(posTransforms[2], 3));
    }
}
