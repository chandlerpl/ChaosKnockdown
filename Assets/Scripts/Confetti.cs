using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confetti : MonoBehaviour
{
    public ParticleSystem systemOne, systemTwo;

    public IEnumerator PlayFullEffect(float duration)
    {   
        systemOne.Play();

        systemTwo.Play();

        yield return new WaitForSeconds(duration);

        systemOne.Stop();
        
        systemTwo.Stop();
    }
}
