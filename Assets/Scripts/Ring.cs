using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private bool ringAwake = false;
    private Objective objectiveScript;

    private void Start()
    {
        objectiveScript = FindObjectOfType<Objective>();
    }


    public void ActivateRing()
    {
        ringAwake = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //If ring is active, tell objective script that is has been passed through
        if(ringAwake)
        {
            objectiveScript.NextRing();
            Destroy(gameObject, 6.0f);
        }
    }
}
