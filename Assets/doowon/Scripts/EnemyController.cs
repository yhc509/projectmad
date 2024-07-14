using System.Collections;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform target; // Player's transform
    public NavMeshAgent agent;
    public Animator animator;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    public float crawlThreshold = 0.5f;
    public float catchDistance = 0.15f;
    bool crawl = false;
    Coroutine attackCoroutine;

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
        CheckValidPath();
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        if(worldDeltaPosition.sqrMagnitude > 5f)
        {
            return;
        }

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

        if(Vector3.Distance(transform.position, target.transform.position) < catchDistance)
        {
            StartAttack(2f);
        }
    }

    void OnAnimatorMove()
    {
        // Update position to agent position
        if (attackCoroutine == null)
        {
            transform.position = agent.nextPosition;
        }
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

    public void SetSpeed(float speed)
    {
        if(agent != null)
        {
            agent.speed = speed;
        }
    }

    public void StartAttack(float screamTime)
    {
        if (attackCoroutine != null)
        {
            return;
        }
        attackCoroutine = StartCoroutine(AttackCoroutine(screamTime));
    }

    IEnumerator AttackCoroutine(float screamTime)
    {
        var player = target.GetComponent<PlayerController>();
        player.stunned = true;
        animator.SetBool("Attack", true);
        float t = 0;
        while ( t< screamTime )
        {
            player.LookAt(animator.GetBoneTransform(HumanBodyBones.Head).position);
            yield return null;
            t += Time.deltaTime;
        }
        player.stunned = false;
        animator.SetBool("Attack", false);
        Warp(player.transform.position + player.transform.forward * -20);
        attackCoroutine = null;
    }

    public void CheckValidPath()
    {
        var player = target.GetComponent<PlayerController>();
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Warp(player.transform.position + player.transform.forward * -30);
        }
    }

    public void Warp(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out var hit, 5.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    
        agent.SetDestination(position);
        agent.velocity = Vector3.zero;
        smoothDeltaPosition = Vector3.zero;
        crawl = false;
        animator.SetBool("Crawl", false);
    }
}