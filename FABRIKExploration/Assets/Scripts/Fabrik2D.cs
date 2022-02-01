using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabrik2D : MonoBehaviour
{
    // input
    public Vector2[] inputPositions; // "P" holds all joints AS WELL AS the target location
    public float[] distances; // distances between all joints WITHOUT the target location

    // output
    Vector2[] finalPositions; // holds the positions of all joints WINTHOUT the final position

    // Variables
    //Vector2 targetPos; //this is the final joint in the inputPositions
    float distToTarget;
    float maxLength; // max outstreched length of the chain
    float tolerance = 0.05; // 5% tolerance of error

    //TEST VARIABLES
    public Transform targetpos;

    void CalculateDistances() // populate the distance array with the proper lengths of the segments
    {
        distances = new float[inputPositions.Length - 1];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = (inputPositions[i + 1] - inputPositions[i]).magnitude; // the distance between the current joint and the next joint not including the target
        }
    }

    void TargetDistance() // takes the distance from the root to the target and records it
    {
        distToTarget = (inputPositions[inputPositions.Length - 1] - inputPositions[0]).magnitude; // the length of a vector from root to target
    }

    void MaxChainLength()
    {
        maxLength = 0f;
        for (int i = 0; i < distances.Length; i++)
        {
            maxLength += distances[i];          
        }
        Debug.Log("Max Chain Length: " + maxLength);
    }
    void CheckReach() //this takes the distToTarget and checks if it is within the reack of the FABRIK Chain
    {    

        //if(distToTarget > maxLength)
    }
    // Start is called before the first frame update
    void Start()
    {
        CalculateDistances();
        TargetDistance();
        MaxChainLength();
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < distances.Length; i++)
        {
            Debug.DrawLine(inputPositions[i], inputPositions[i+1], Color.red);
        }
    }
}
