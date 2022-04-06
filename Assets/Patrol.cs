 // Patrol.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Patrol : MonoBehaviour {

    public Transform[] points;
    public float reactionTime = 1;      // how long can we see the player before springing into action.

    public enum state {Patrolling, Chasing, Searching};
    public state currentState = state.Patrolling;
    state lastFrameState; // keeps track of state changes.

    private int destPoint = 0;
    private NavMeshAgent agent;

    private AIFoV fov;


    void Start () {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<AIFoV>();

        lastFrameState = currentState;

            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
        agent.autoBraking = false;

        GotoNextPoint();
    }


    void GotoNextPoint() {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    }

    private float eyesOnPlayerTimer = 0;

    void Patrolling() {
        if(fov.canSeePlayer == true) {
            eyesOnPlayerTimer += Time.deltaTime;
            if(eyesOnPlayerTimer > reactionTime) {
                currentState = state.Chasing;
                eyesOnPlayerTimer = 0;
                return;
            }
        } 
        else {
            //reset the eyesOnPlayerTimer if we lose sight of the player.
            eyesOnPlayerTimer = 0;
        }
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f) {
            currentState = state.Searching; // this is searching. go to the searching state.
        }
    }

    void Chasing() {
        agent.destination = fov.player.position;
        float distance = Vector3.Distance(this.transform.position, fov.player.position);
        Debug.Log("Distance " + distance);
        if(distance > fov.sightDistance) {
            //the AI will continue to its destination, then go to the next patrol point
            currentState = state.Patrolling;
        }
    }

    bool waitingAtPoint = false;

    void Searching() {
        if(!waitingAtPoint){
            StartCoroutine(WaitAtPatrolPoint());
            currentState = state.Patrolling;
        }
    }



    void Update () {
        switch(currentState) {
            case state.Patrolling: Patrolling(); break;
            case state.Chasing: Chasing(); break;
            case state.Searching: Searching(); break;
        }

        if(lastFrameState != currentState) {
            Debug.Log("State has changed.");
            Debug.Log("Agent remaining Distance : " + agent.remainingDistance);

        }
        lastFrameState = currentState;
    }

    IEnumerator WaitAtPatrolPoint() {
        waitingAtPoint = true;
        // play the waiting animation
        yield return new WaitForSeconds(0);
        GotoNextPoint();
        waitingAtPoint = false;
    }
}