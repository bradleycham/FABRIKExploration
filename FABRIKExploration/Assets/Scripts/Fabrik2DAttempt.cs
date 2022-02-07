using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabrik2DAttempt : MonoBehaviour
{
    // input
    public Vector2[] inputPositions; // "P" holds all joints AS WELL AS the target location
    public Transform target;

    // output
    Vector2[] finalPositions; // holds the positions of all joints WINTHOUT the final position

    // Variables
    //Vector2 targetPos; //this is the final joint in the inputPositions
    float distToTarget;
    float maxLength; // max outstreched length of the chain
    float tolerance = 0.05f; // 5% tolerance of error
    public float[] distances; // distances between all joints WITHOUT the target location

    float[] r;
    float[] lambda;
    Vector2 b;
    float dif;

    void CalculateDistances() // populate the distance array with the proper lengths of the segments
    {
        distances = new float[inputPositions.Length - 2];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = (inputPositions[i + 1] - inputPositions[i]).magnitude; // the distance between the current joint and the next joint not including the target
        }
    }
    void SetTarget()
    {
        Vector2[] tempArray = inputPositions; // make a temp array to hold all the inputPosition data
        inputPositions = new Vector2[tempArray.Length + 1]; // add 1 length to inputPositions to hold the target
        for (int i = 0; i < tempArray.Length; i++)
        {
            inputPositions[i] = tempArray[i];
        }

        UpdateTarget();
    }
    void UpdateTarget()
    {
        Vector2 tempPos;
        tempPos.x = target.position.x;
        tempPos.y = target.position.y;
        inputPositions[^1] = tempPos;
    }
    float TargetDistance() // takes the distance from the root to the target and records it
    {
        return (inputPositions[^1] - inputPositions[0]).magnitude; // the length of a vector from root to target
    }

    // Only needs to be set once
    float MaxChainLength()
    {
        float lengthTemp = 0f;
        for (int i = 0; i < distances.Length; i++)
        {
            lengthTemp += distances[i];          
        }
        //Debug.Log("Max Chain Length: " + lengthTemp);
        return lengthTemp;
    }
    
    // FABRIK
    void FABRIK()
    {
        if (distToTarget > maxLength)
        {
            //return; // THIS NEEDS TO BE REMOVED
            // find new joint positions
            for (int i = 0; i < distances.Length; i++) // AAAAAAH
            {
                r[i] = (inputPositions[^1] - inputPositions[i]).magnitude;
                lambda[i] = distances[i] / r[i];
                //find the new joint positions
                inputPositions[i + 1] = ((1f - lambda[i]) * inputPositions[i]) + (lambda[i] * inputPositions[^1]);
            }
        }
        else
        {
            //the target is reachable
            b = inputPositions[0];
            dif = (inputPositions[^2] - inputPositions[^1]).magnitude;
            while (dif > (tolerance * maxLength))
            {
                //forawardreaching
                inputPositions[^2] = inputPositions[^1]; //set the end effector as the target
                for (int i = 0; i < inputPositions.Length - 2; i++) // AAAAAH
                {
                    r[i] = (inputPositions[i + 1] - inputPositions[i]).magnitude;
                    lambda[i] = distances[i] / r[i]; 
                    Debug.Log(lambda[i]); //DEBUG
                    //find new positions
                    inputPositions[i] = ((1f - lambda[i]) * inputPositions[i+1]) + (lambda[i] * inputPositions[i]);
                }

                inputPositions[0] = b;
                for (int k = 0; k < inputPositions.Length - 2; k++)
                {
                    r[k] = (inputPositions[k + 1] - inputPositions[k]).magnitude;
                    lambda[k] = distances[k] / r[k];
                    inputPositions[k + 1] = ((1f - lambda[k]) * inputPositions[k+1]) + (lambda[k] * inputPositions[k]);

                }
                dif = (inputPositions[^2] - inputPositions[^1]).magnitude;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //belongs in start
        SetTarget();
        CalculateDistances(); //populates distances

        FloatInit();
        //move these
        distToTarget = TargetDistance();
        maxLength = MaxChainLength();

              
    }

    // Update is called once per frame
    void Update()
    {

        UpdateTarget();

        FABRIK();

        DrawDebugLines();   
    }

    void FloatInit()
    {
        r = new float[distances.Length];
        lambda = new float[distances.Length];
    }
    void DrawDebugLines()
    {
        for (int i = 0; i < distances.Length; i++)
        {
            Debug.DrawLine((Vector3)inputPositions[i], (Vector3)inputPositions[i + 1], Color.red);
        }
        Debug.DrawLine((Vector3)inputPositions[^2], (Vector3)inputPositions[^1], Color.red);
    }
}
