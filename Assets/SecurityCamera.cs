using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public Transform player;
    public float fieldOfView = 45;
    Transform emitter;

    public Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        emitter = transform.GetChild(0);
        // rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        Vector3 rayDirection = (player.position + Vector3.up) - emitter.position;

        float angle = Vector3.Angle(rayDirection, emitter.forward);

        if(angle < fieldOfView)
        {
            if (Physics.Raycast(emitter.position, rayDirection, out hit, 30f))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.DrawRay(emitter.position, rayDirection, Color.green);
                    rend.material.color = Color.green;
                }
                else
                {
                    Debug.DrawRay(emitter.position, rayDirection, Color.red);
                    rend.material.color = Color.red;
                }
            }
            else
            {
                rend.material.color = Color.black;
            }
        }
        else if(angle < fieldOfView + 25)
        {
            rend.material.color = Color.yellow;
            Debug.DrawRay(emitter.position, rayDirection, Color.yellow);
        }
        else
        {
            rend.material.color = Color.black;
        }
        
            
    
    }
}
