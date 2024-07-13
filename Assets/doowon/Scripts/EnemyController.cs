using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public Transform target; // Player's transform
    private NavMeshAgent agent;
    public Animator animator;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    public float crawlThreshold = 0.5f;
    bool crawl = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updatePosition = false;
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }

        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // Update animation parameters
        float speed = velocity.magnitude;
        animator.SetFloat("Speed", speed);
        if(speed> crawlThreshold)
        {
            crawl = true;
        }
        if(crawl && speed < 0.5f)
        {
            crawl = false;
        }
        animator.SetBool("Crawl", crawl);
    }

    void OnAnimatorMove()
    {
        // Update position to agent position
        transform.position = agent.nextPosition;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator != null)
        {
            if(target != null)
            {
                animator.SetLookAtWeight(1f);
                //animator.SetLookAtPosition(agent.steeringTarget + transform.forward);
                animator.SetLookAtPosition(target.transform.position);
            }
        }
    }
}