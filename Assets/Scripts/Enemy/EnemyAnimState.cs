using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimState : StateMachineBehaviour
{
    public EnemyState state;
    public int n;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyCharacter enemy = animator.GetComponent<EnemyCharacter>();
        enemy.OnAnimStateEnter(state);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyCharacter enemy = animator.GetComponent<EnemyCharacter>();
        enemy.OnAnimStateUpdate(state, n);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyCharacter enemy = animator.GetComponent<EnemyCharacter>();
        enemy.OnAnimStateExit(state, n);
    }
}