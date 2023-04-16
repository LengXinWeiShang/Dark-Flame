using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimState : StateMachineBehaviour
{
    public PlayerState state;
    public int n;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter player = animator.GetComponent<PlayerCharacter>();
        player.OnAnimStateEnter(state);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter player = animator.GetComponent<PlayerCharacter>();
        player.OnAnimStateUpdate(state, n);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCharacter player = animator.GetComponent<PlayerCharacter>();
        player.OnAnimStateExit(state, n);
    }
}
