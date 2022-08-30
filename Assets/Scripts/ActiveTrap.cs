using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTrap : MonoBehaviour
{
    public enum TrapType 
    {
        GENERIC,
        COLLAPSE,
        SMACK,
        PUNCH
    }

    public TrapType trapType;

    void OnValidate()
    {
        this.tag = "ActiveTrap";
    }
}
