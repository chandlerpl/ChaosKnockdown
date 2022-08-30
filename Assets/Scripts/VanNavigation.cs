using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VanNavigation : MonoBehaviour
{

    private NavMeshAgent _agentVan;
    // Start is called before the first frame update
    void Start()
    {
        _agentVan= GetComponent<NavMeshAgent>();
        //_agentVan.SetDestination(GameManager.instance.finishLine.position);
    }

    // Update is called once per frame
    void Update()
    {
        _agentVan.SetDestination(GameManager.instance.finishLine.position);
    }
}
