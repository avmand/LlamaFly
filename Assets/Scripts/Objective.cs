using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private List<Transform> rings = new List<Transform>();

    public Material activeRing;
    public Material inactiveRing;
    public Material finalRing;
    private int ringfinished = 0;


    private void Start()
    {
        //set obj field of GameScene wala script
        FindObjectOfType<GameScene>().obj = this;
        //at start, assign inactive
        foreach (Transform t in transform)
        {
            rings.Add(t);
            t.GetComponent<MeshRenderer>().material = inactiveRing;
        }

        if(rings.Count==0)
        {
            Debug.Log("The start part of objective. Aka no objectives assigned on this level da");
            return;
        }

        //Activate first one
        rings[ringfinished].GetComponent<MeshRenderer>().material = activeRing;
        rings[ringfinished].GetComponent<Ring>().ActivateRing();
    }

    public void NextRing()
    {
        //some effects thing
        rings[ringfinished].GetComponent<Animator>().SetTrigger("collectionTrigger");
        ringfinished++;

        //if ring is the last one, call  a celebration
        if(ringfinished==rings.Count)
        {
            Debug.Log("NextRing1 " + ringfinished);
            Victory();
            return;
        }
        //if second last, give cute ring
        if(ringfinished==rings.Count-1)
        {
            Debug.Log("NextRing2 "+ ringfinished);
            rings[ringfinished].GetComponent<MeshRenderer>().material = finalRing;


        }
        else
        {
            Debug.Log("NextRing3 " + ringfinished);
            rings[ringfinished].GetComponent<MeshRenderer>().material = activeRing;
        }
        rings[ringfinished].GetComponent<Ring>().ActivateRing();
    }


    private void Victory()
    {
        FindObjectOfType<GameScene>().CompleteLevel();
    }

    public Transform getcurrring()
    {
        return rings[ringfinished];
    }
}
