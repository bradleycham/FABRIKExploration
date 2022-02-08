using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubChain : MonoBehaviour
{
    public Transform[] joints;
    public Transform target;

    float[] lengths;
    public float chainLength;
    public float distFromRoot;

    float lambda;
    public float tolerance = 0.05f;
    float diff;
    //int maxIterations = 10;
    Vector3 ray;
    void CalculateLengths() // this runs once at the beginning
    {
        //set up lengths to be one less than amount of joints
        lengths = new float[joints.Length - 1];
        for (int i = 0; i < lengths.Length; i++)
        {
            lengths[i] = (joints[i + 1].position - joints[i].position).magnitude;
        }

        //Calculate Max length  
        chainLength = 0f;
        for (int i = 0; i < lengths.Length; i++)
        {
            chainLength += lengths[i];
        }       
    }
    private void Start()
    {
        CalculateLengths();
    }


    // FABRIK ALGORITHM ===========================================

    
    public void CheckTargetDistance()
    {
        distFromRoot = (target.position - joints[0].position).magnitude; // set the distance of the root to target to the mag of a vector between the two
    }
    
    //ontly run this when you have the centroid thing working

    public void OutsideReach()
    {
        //CheckTargetDistance();
       // if (distFromRoot > chainLength + tolerance)
        //{
            Vector3 rayToTarget = target.position - joints[0].position;
            for (int j = 0; j < lengths.Length; j++)
            {
                joints[j + 1].position = joints[j].position + (lengths[j] * rayToTarget.normalized);
            }
        //}
    }
    
    public Vector3 Forward()
    {
        joints[^1].position = target.position;

        for(int i = lengths.Length-1; i > 0;  i--)
        {
            ray = joints[i + 1].position - joints[i].position;
            lambda = lengths[i] / ray.magnitude;

            joints[i].position = ((1f - lambda) * joints[i+1].position) + (lambda * joints[i].position);
        }
        return joints[0].position;
    }

    public void Backward(Vector3 basePos)
    {
        // STAGE 2 Backwards
        joints[0].position = basePos;
        for (int k = 0; k < lengths.Length; k++)
        {
            ray = joints[k + 1].position - joints[k].position; //ray from next joint to joint
            lambda = lengths[k] / ray.magnitude;
            //IP[k + 1] = IP[k] + ((1f - lambda) * ray);
            joints[k + 1].position = ((1f - lambda) * joints[k].position) + (lambda * joints[k + 1].position);
        }
    }

    public void CheckDiff()
    {
        //check if the end effector is withing the tolerance of the target
        diff = (target.position - joints[^1].position).magnitude;
        if (diff < tolerance) // if it is then stop iterating
            return;
    }
}
