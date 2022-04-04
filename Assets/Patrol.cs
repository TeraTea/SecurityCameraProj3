// Patrol.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AIFoV))]
public class Patrol : MonoBehaviour {

    public Transform[] points;
    public float reactionTime = 1;      // how long can we see the player before springing into action.
    private int destPoint = 0;
    private NavMeshAgent agent;

    private AIFoV fov;


    void Start () {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<AIFoV>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        // agent.autoBraking = false;

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

    void Update () {
        if(fov.canSeePlayer == true) {
            eyesOnPlayerTimer += Time.deltaTime;
            Debug.Log("EyesOnPlayerTimer: " + eyesOnPlayerTimer);
            if(eyesOnPlayerTimer > reactionTime) {
                agent.destination = fov.player.position;
                return;     // don't look at anything else in the Update function.
            }
        }
        else {
            //reset the eyesOnPlayerTimer if we lose sight of the player.
            eyesOnPlayerTimer = 0;
        }

        

        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            StartCoroutine(WaitAtPatrolPoint());
    }

    IEnumerator WaitAtPatrolPoint() {
        // play the waiting animation
        yield return new WaitForSeconds(2);
        GotoNextPoint();
    }
}