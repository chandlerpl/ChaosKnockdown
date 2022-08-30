using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CyclistMovement : MonoBehaviour
{
    public Material downedMaterial;
    public float despawnTime = 5;
    public GameObject emoji;
    public Animator animator;
    public Animator animatorBike;
    public Sprite[] emojiSprites;
    public CameraSpline cameraSpline;
    public SkinnedMeshRenderer[] meshRenderers;
    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private Collider _collider;
    public Rigidbody[] ragdollRigidbodies;
    public Vector3 impactDirection;

    private bool _useGravity;
    private float _startingSpeed;

    [Header("Speed Modifier")]
    [SerializeField] [Range(0,1)] private float _maxSpeedMod = 0.1f;
    [SerializeField] [Range(0,1)] private float _accelerationMod = 0.05f;
    [SerializeField] [Range(0,1)] private float _angularSpeedMod = 0.1f;

    [Header("Cyclist State")]
    public bool completed;
    public bool downed;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _agent.SetDestination(GameManager.instance.finishLine.position);

        // Speed modification
        _agent.speed *= Random.Range(1f - _maxSpeedMod, 1f + _maxSpeedMod); // Max speed
        _startingSpeed = _agent.speed;
        _agent.acceleration *= Random.Range(1f - _accelerationMod, 1f + _accelerationMod); // Acceleration
        _agent.angularSpeed *= Random.Range(1f - _angularSpeedMod, 1f + _angularSpeedMod);
        _agent.updateRotation = false;
        _rigidbody.freezeRotation = true;
        
        foreach(Rigidbody body in ragdollRigidbodies) {
            body.GetComponent<Collider>().enabled = false;
        }
    }

    void Update()
    {
        // Can later decrease the checking frequency
        if (_agent)
        {
            // Multiplier = current speed * 0.25f
            float speedMultiplier = _agent.velocity.magnitude * 0.25f;
            animator.SetFloat("SpeedMultiplier", speedMultiplier);
            animatorBike.SetFloat("SpeedMultiplier", speedMultiplier);

            GameObject closestCyclist = GameManager.instance.GetClosestCyclist();
            if(closestCyclist != null) {
                if(closestCyclist == gameObject) {
                    _agent.speed = _startingSpeed * 0.75f;
                } else {
                    float distance = Vector3.Distance(closestCyclist.transform.position, cameraSpline.GetNextPoint());
                    if(distance > 20) {
                        _agent.speed = _startingSpeed * 1.5f;
                    } else if (distance < 10) {
                        _agent.speed = _startingSpeed * 0.75f;
                    } else {
                        _agent.speed = _startingSpeed;
                    }
                }
            }

            Vector3 headingDirection = _agent.velocity.normalized;
            transform.localRotation *= Quaternion.FromToRotation(transform.forward, headingDirection);
            RaycastHit slopeHit;
            //Perform raycast from the object's position downwards
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 4, LayerMask.GetMask("Road")))
            {
                //Get slope angle from the raycast hit normal then calcuate new pos of the object
                Quaternion newRot = Quaternion.FromToRotation(transform.up, slopeHit.normal) * transform.rotation;

                //Apply the rotation 
                transform.rotation = newRot;

            }
        }
    }

    public float GetRemainingDistance()
    {
        float distance = 0;
        if(_agent == null || _agent.pathStatus != NavMeshPathStatus.PathComplete) {
            return distance;
        }

        Vector3[] corners = _agent.path.corners;

        if (corners.Length > 2)
        {
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 previous = new Vector2(corners[i - 1].x, corners[i - 1].z);
                Vector2 current = new Vector2(corners[i].x, corners[i].z);

                distance += Vector2.Distance(previous, current);
            }
        }
        else 
        {
            distance = _agent.remainingDistance;
        }
    
        return distance;
    }

    private void OnTriggerEnter(Collider other) {
        if (!downed)
        {
            string tag = other.tag;

            if(tag.Equals("ActiveTrap")) {
                this.downed = true;

                // Get the trap type and determine what kind of impact it produces
                ActiveTrap trap = other.GetComponent<ActiveTrap>();
                if (trap) impactDirection = GetTrapImpactDirection(trap, other.transform.position); else impactDirection = -this.transform.forward;

                StartCoroutine(DespawnTimer());
                GameManager.instance.AddDamageInfo("Trap Hit");
                return;
            }
            else if (tag.Equals("CyclistDowned")){
                this.downed = true;
                impactDirection = (other.transform.position - this.transform.position).normalized; // Cyclist should be pushed forward due to the momentum
                GameManager.instance.IncreaseChainReactionCount();
                StartCoroutine(DespawnTimer());
                GameManager.instance.AddDamageInfo("Cyclist Collision");
                return;
            }

            if(tag.Equals("FinishLine")) {
                _agent.ResetPath();
                _agent.isStopped = true;
                _rigidbody.isKinematic = true;
                this.completed = true; // This state indicates that the camera should move to the next closest cyclist who has not yet compelted match - otherwise the camera will just focus on the first one when there are still few cyclists on the way
                this.gameObject.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (!downed)
        {
            string tag = other.transform.tag;

            if(tag.Equals("ActiveTrap") || tag.Equals("CyclistDowned")) 
            {
                Vector3 velocity = _agent.velocity;

                // We may need to take the cyclist's current speed into consideration when it collides with a downed cyclist, because when the first billboard collapses, 60 ~ 99% of the cyclists are downed when they just start accelerating
                // The current cyclist should not get affected if he is riding slowly (riding fast => down)
                if(tag.Equals("CyclistDowned")) {
                    if(velocity.magnitude < _agent.speed * 0.75f ) {
                        // If current speed is below 75% of the max speed then nothing happens
                        return;
                    } else {
                        float rand = Random.Range(0, 1f);
                        if(rand < 0.1 + (0.9 - 0.1) * (velocity.magnitude / _agent.speed)) {
                            this.downed = true;
                            impactDirection = this.transform.forward;  // Cyclist should be pushed forward due to the momentum
                            GameManager.instance.IncreaseChainReactionCount();
                            StartCoroutine(DespawnTimer());
                            GameManager.instance.AddDamageInfo("Cyclist Collision");
                        }
                    }
                } else {
                    // Trap 
                    this.downed = true;

                    // Get the trap type and determine what kind of impact it produces
                    ActiveTrap trap = other.transform.GetComponent<ActiveTrap>();
                    if (trap) impactDirection = GetTrapImpactDirection(trap, other.GetContact(0).point); else impactDirection = -this.transform.forward;

                    StartCoroutine(DespawnTimer());
                    GameManager.instance.AddDamageInfo("Trap Hit");
                }
            }
        }
    }

    private IEnumerator DespawnTimer() {
        gameObject.tag = "CyclistDowned";
        transform.parent = GameManager.instance.downedCyclists;
        //animator.SetBool("Crashed", true);
        animatorBike.SetBool("Crashed", true);

        float forceMultiplier = (_agent.velocity.magnitude / _agent.speed) + 1; // Full speed results in 200% force

        _agent.ResetPath();
        _agent.isStopped = true;

        foreach(Rigidbody body in ragdollRigidbodies) {
            body.velocity = Vector3.zero;
            body.GetComponent<Collider>().enabled = true;
        }

        gameObject.GetComponent<MeshRenderer>().material = downedMaterial;
        //changes all model pieces to downedMaterial
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = downedMaterial;
        }

        StartCoroutine(Emote());

        yield return null;

        animator.enabled = false;

        #region RIGIDBODY_ADDFORCE_TEST
        // The agent needs to be turned
        if (_useGravity) 
        { 
            _agent.enabled = false;
            forceMultiplier = 100f;
        }
        // First parameter is the direction * force, second one defines that it is a one-time action
        //ragdollRigidbodies[0].AddForce((impactDirection).normalized * Random.Range(1f, 2.5f), ForceMode.Impulse);
        ragdollRigidbodies[0].AddForce(impactDirection.normalized * (forceMultiplier + Random.Range(-0.25f, 0.25f)), ForceMode.Impulse);
        #endregion

        float shrinkTimer = despawnTime / 100;
        float currTime = 0;

        yield return new WaitForSeconds(0.5f);
        while (currTime < despawnTime)
        {
            yield return new WaitForSeconds(shrinkTimer);
            currTime += shrinkTimer;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, currTime / despawnTime);
        }

        this.gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    private IEnumerator Emote() {

        if (Random.Range(0, 3) == 0)
        {
            GameObject Emoji = Instantiate(emoji, new Vector3(transform.position.x, transform.position.y + Random.Range(1.0f, 1.5f), transform.position.z), Quaternion.identity);
            //change to random emoji
            Emoji.GetComponentInChildren<SpriteRenderer>().sprite = emojiSprites[Random.Range(0, emojiSprites.Length)];

            yield return new WaitForSeconds(0.7f);
            Destroy(Emoji);
        }
    }

    private Vector3 GetTrapImpactDirection(ActiveTrap trap, Vector3 contactPoint){
        switch (trap.trapType){
            case ActiveTrap.TrapType.GENERIC:
                return -this.transform.forward; // Push backward

            case ActiveTrap.TrapType.COLLAPSE:
                return -this.transform.up; // Push downward

            case ActiveTrap.TrapType.PUNCH:
                return (this.transform.position - contactPoint).normalized; // Push to opposite direction of the contact point

            case ActiveTrap.TrapType.SMACK:
                _useGravity = true;
                Vector3 horizontalDir = (this.transform.position - contactPoint).normalized; 
                Vector3 verticalDir = this.transform.up;
                return (horizontalDir + verticalDir).normalized; // Push to opposite direction of the contact point + launch cyclist

            default:
                Debug.LogError("Cannot determine the impact direction.");
                return Vector3.zero;
        }
    }

     void OnDrawGizmos() {
        #if UNITY_EDITOR
        // For showing real-time speed on GUI
        if (_agent) UnityEditor.Handles.Label(this.transform.position + Vector3.up, _agent.velocity.magnitude.ToString("F2"));
        #endif
     }
}
