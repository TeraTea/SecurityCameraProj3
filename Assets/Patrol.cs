  // Patrol.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic; // for lists

public class Patrol : MonoBehaviour {

    public Transform[] points;
    public float reactionTime = 1;      // how long can we see the player before springing into action.
    public float waitAtPointInterval = 2f;

    public enum state {Patrolling, Chasing, Searching};
    public state currentState = state.Patrolling;
    state lastFrameState; // keeps track of state changes.

    private int destPoint = 0;
    private NavMeshAgent agent;
    bool waitingAtPoint = false;
    private AIFoV fov;

    IEnumerator wait;


    void Start () {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<AIFoV>();

        lastFrameState = currentState;

        wait = WaitAtPatrolPoint();

            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            //agent.autoBraking = false;
            

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

        currentState = state.Patrolling;
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
        if(distance > fov.sightDistance) {
            //the AI will continue to its destination, then go to the next patrol point
            currentState = state.Patrolling;
        }
    }


    void Searching() {
        if(!waitingAtPoint){
            wait = WaitAtPatrolPoint();
            StartCoroutine(wait); // this is searching

        }

        LookForPlayer();
    }

    void LookForPlayer() {
            if(fov.canSeePlayer == true) {
            eyesOnPlayerTimer += Time.deltaTime;

            if(eyesOnPlayerTimer > reactionTime) {
                currentState = state.Chasing;
                eyesOnPlayerTimer = 0;
                StopCoroutine(wait);        // stop animating 
                wait = null;
                eyePivot.rotation = looks[0].rotation;      // sets eye to face foward.
                waitingAtPoint = false;     // resetting the coroutine.
                return;
            }
        } 
        else {
            //reset the eyesOnPlayerTimer if we lose sight of the player.
            eyesOnPlayerTimer = 0;
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

        }
        lastFrameState = currentState;
    }

    public Transform eyePivot;
    public AnimationCurve curve;
    public List<Transform> looks = new List<Transform>();

    IEnumerator WaitAtPatrolPoint() {
        waitingAtPoint = true;
        // play the waiting animation
        //yield return new WaitForSeconds(waitAtPointInterval);

        //stop moving
            // assume this already happens. if not, force it.
        //look left and wait
        float timer = 0;
        while(timer < 1) {
            eyePivot.rotation = Quaternion.Lerp(looks[0].rotation, looks[1].rotation, curve.Evaluate(timer));
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1);

        //look right and wait
        timer = 0;
        while(timer < 1) {
            eyePivot.rotation = Quaternion.Lerp(looks[1].rotation, looks[2].rotation, curve.Evaluate(timer));
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1);

        //look forward and go to next point.
        timer = 0;
        while(timer < 1) {
            eyePivot.rotation = Quaternion.Lerp(looks[2].rotation, looks[0].rotation, curve.Evaluate(timer));
            timer += Time.deltaTime;
            yield return null;
        }



        GotoNextPoint();
        waitingAtPoint = false;
    }
}