using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Trap : MonoBehaviour
{
    [Tooltip("The number of seconds before a trap can be reused, 0 to disable reusability.")]
    public float reusableTimer = 5f;
    public CameraTracker tracker;

    private Collider _collider;
    private NavMeshObstacle _navObstacle;
    private bool _triggered = false;

    private void Start() {
        _collider = GetComponent<Collider>();
        _navObstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnMouseDown() {
        if (!_triggered)
        {
            _triggered = true;
            tracker.ExitedTrap();
            if (_navObstacle != null)
                _navObstacle.enabled = true;
            tag = "ActiveTrap";
            TrapTriggered();

            if (reusableTimer > 0)
                StartCoroutine(StartTimer());
        }
    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(reusableTimer);

        ResetTrap();
        if (_navObstacle != null)
            _navObstacle.enabled = false;
        tag = "Trap";
        _triggered = false;
    }

    public abstract void TrapTriggered();
    public abstract void ResetTrap();
}
