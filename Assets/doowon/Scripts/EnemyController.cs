using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public Transform target; // Player's transform
    private NavMeshAgent agent;
    public Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator != null)
        {
            if(target != null)
            {
                animator.SetLookAtWeight(1f);
                animator.SetLookAtPosition(target.position);
            }
        }
    }
}