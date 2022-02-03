using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FABRIK2D1 : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2[] IP;
    public float[] lengths; // one less than the length of IP
    public Vector2 targetPos;
    public Vector2 basePos;
    public Transform targetTrans;

    public float tolerance = 0.05f;

    public float targetDistFromRoot; 
    public float chainLength; // Set in start
    int maxIterations = 4;
    public float diff; // the distance between the end effector and the target
    public float lambda;
    void Start()
    {
        CalcLengths();
        CalcChainLength();
    }
    void CalcLengths()
    {
        lengths = new float[IP.Length - 1];
        for(int i = 0; i < lengths.Length; i++)
        {
            lengths[i] = (IP[i + 1] - IP[i]).magnitude;
        }
    }
    void CalcChainLength()
    {
        //Calculate Max length  
        chainLength = 0f;
        for (int i = 0; i < lengths.Length; i++)
        {
            chainLength += lengths[i];
        }
    }
    void CheckTargetDistance()
    {
        targetDistFromRoot = (targetPos - IP[0]).magnitude; // set the distance of the root to target to the mag of a vector between the two
    }
    void FABRIK()
    {
        targetPos = (Vector2)targetTrans.position;
        CheckTargetDistance();

        if (targetDistFromRoot > chainLength)
        {
            Vector2 rayToTarget = targetPos - IP[0];
            for (int j = 0; j < lengths.Length; j++)
            {
                IP[j+1] = IP[j] + lengths[j] * rayToTarget.normalized;
            }
        }
        else
        {
            for (int i = 0; i < maxIterations; i++) // the 'while' loop replacing the tolerance bs
            {
                // Perform forward backward while the end efector is not within the range of target

                //set root position
                basePos = IP[0];
                //check if the end effector is within the tolerance of the target
                diff = (targetPos - IP[^1]).magnitude;
                    //STAGE 1 forward
                IP[^1] = targetPos;
                //find the dstance between the new joint and the -1 joint
                for (int p = lengths.Length-1; p > 0; p--)
                {
                    Vector2 ray = IP[p + 1] - IP[p]; //ray from last to joint to next joint
                    lambda = lengths[p] / ray.magnitude;
                    //IP[p] = IP[p+1] + ((1f - lambda) * ray); // find the new position
                    IP[p] = ((1f - lambda) * IP[p + 1]) + (lambda * IP[p]);

                }
                //stage 2 Backwards
                IP[0] = basePos;
                for(int k = 0; k < lengths.Length;  k++)
                {
                    Vector2 ray = IP[k + 1] - IP[k]; //ray from next joint to joint
                    lambda = lengths[k] / ray.magnitude;
                    //IP[k + 1] = IP[k] + ((1f - lambda) * ray);
                    IP[k + 1] = ((1f - lambda) * IP[k]) + (lambda * IP[k + 1]);
                }
                diff = (targetPos - IP[^1]).magnitude;
                if (diff < tolerance)
                    return;
            }
        }
            
    }
    // Update is called once per frame
    void Update()
    {
        FABRIK();
        DrawDebugLines();
    }
    void DrawDebugLines()
    {
        for (int i = 0; i < lengths.Length; i++)
        {
            Color color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));
            Debug.DrawLine((Vector3)IP[i], (Vector3)IP[i + 1], color);
        }
        //Debug.DrawLine((Vector3)IP[^2], (Vector3)IP[^1], Color.red);
    }
}
