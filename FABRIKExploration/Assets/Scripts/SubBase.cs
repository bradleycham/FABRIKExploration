using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBase : MonoBehaviour
{
    public SubChain[] chains;
    Vector3[] forwardResults;
    Vector3 centroid;
    int maxIterations = 10;

    private void Start()
    {
        forwardResults = new Vector3[chains.Length];
    }
    private void Update()
    {
        
        for(int iter = 0; iter < maxIterations; iter++)
        {
            centroid = new Vector3();
            for (int sc = 0; sc < chains.Length; sc++)
            {
                chains[sc].CheckTargetDistance();
                if(chains[sc].distFromRoot > chains[sc].chainLength + chains[sc].tolerance)
                {
                    chains[sc].OutsideReach();
                    forwardResults[sc] = chains[sc].joints[0].position;
                }
                else
                    forwardResults[sc] = chains[sc].Forward();
            }
            //find the centroid
            for(int results = 0; results < forwardResults.Length; results++)
            {
                centroid += forwardResults[results];
            }
            centroid /= forwardResults.Length;
            Debug.Log(centroid);
            for(int b = 0; b < chains.Length; b++)
            {
                chains[b].Backward(centroid);
            }
        }        
    }
}
