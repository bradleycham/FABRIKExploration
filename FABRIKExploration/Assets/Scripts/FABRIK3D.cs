using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FABRIK3D : MonoBehaviour
{
    // Start is called before the first frame update
    public bool useTransformsToGenerate;
    public Transform[] inputTransforms;
    Vector3 transfer;
    Vector3 offset;

    public SubBase[] subBases;

    public Vector3[] IP;
    float[] lengths; // one less than the length of IP
    Vector3 targetPos;
    Vector3 basePos;
    public Transform targetTrans;

    public float tolerance = 0.05f;

    float targetDistFromRoot;
    float chainLength; // Set in start
    int maxIterations = 10;
    float diff; // the distance between the end effector and the target
    float lambda;
    void Start()
    {
        UseInputTransforms();
        CalcLengths();
        CalcChainLength();
    }

    void UseInputTransforms()
    {
        if (useTransformsToGenerate)
        {
            IP = new Vector3[inputTransforms.Length];

            for (int i = 0; i < inputTransforms.Length; i++)
            {
                IP[i].x = inputTransforms[i].position.x;
                IP[i].y = inputTransforms[i].position.y;
            }
        }
    }
    void CalcLengths()
    {
        lengths = new float[IP.Length - 1];
        for (int i = 0; i < lengths.Length; i++)
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
        targetPos = (Vector3)targetTrans.position;
        CheckTargetDistance();

        if (targetDistFromRoot > chainLength + tolerance)
        {
            Vector3 rayToTarget = targetPos - IP[0];
            for (int j = 0; j < lengths.Length; j++)
            {
                IP[j + 1] = IP[j] + lengths[j] * rayToTarget.normalized;
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

                bool isASubBase = false;
                SubBase sub;
                //find the dstance between the new joint and the -1 joint
                for (int p = lengths.Length - 1; p > 0; p--)
                {
                    for (int si = 0; si < subBases.Length; si++)
                    {
                        if (subBases[si].index == p)
                        {
                            isASubBase = true;
                            sub = subBases[si];
                            SubFabrik(sub);
                            break;
                        }
                            
                    }
                    if(isASubBase)
                    {
                        
                        
                        //SubFabrik
                    }
                    else
                    {
                        Vector3 ray = IP[p + 1] - IP[p]; //ray from last to joint to next joint
                        lambda = lengths[p] / ray.magnitude;
                        //IP[p] = IP[p+1] + ((1f - lambda) * ray); // find the new position
                        IP[p] = ((1f - lambda) * IP[p + 1]) + (lambda * IP[p]);
                    }
                    

                }
                //stage 2 Backwards
                IP[0] = basePos;
                for (int k = 0; k < lengths.Length; k++)
                {
                    Vector3 ray = IP[k + 1] - IP[k]; //ray from next joint to joint
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

    void SubFabrik(SubBase sub, bool backward) //returns the location of the subbase take in all of these and find the centroid
    {
        Vector3[] outputSubs = new Vector3[sub.subChains.Length]; // the amount of different locations that the sub base was moved to
        
        Transform[] subChain;
        Transform subTarget;
        Vector3 subBasePos;
        float[] subLengths;
        for (int sc = 0; sc < sub.subChains.Length; sc ++)
        {
            // for each subchain, perform Ik
            subChain = sub.subChains[sc].SBTrans;
            subTarget = sub.subChains[sc].target;
            subLengths = sub.subChains[sc].SBLenghts;
            //iterate through the subchains and perform ik on them, then find the centroid of the subBase
            for (int i = 0; i < maxIterations; i++) // the 'while' loop replacing the tolerance bs
            {
                // Perform forward backward while the end efector is not within the range of target

                //set root position
                subBasePos = subChain[0].position;
                //check if the end effector is within the tolerance of the target
                diff = (sub.subChains[sc].target.position - subChain[^1].position).magnitude;
                //STAGE 1 forward
                subChain[^1].position = subTarget.position;
                //find the dstance between the new joint and the -1 joint
                for (int p = subLengths.Length - 1; p > 0; p--)
                {
                    Vector3 ray = subChain[p + 1].position - subChain[p].position; //ray from last to joint to next joint
                    lambda = subLengths[p] / ray.magnitude;
                    subChain[p].position = ((1f - lambda) * subChain[p + 1].position) + (lambda * subChain[p].position);

                }
                //stage 2 Backwards
                subChain[0].position = subBasePos;
                for (int k = 0; k < subLengths.Length; k++)
                {
                    Vector3 ray = subChain[k + 1].position - subChain[k].position; //ray from next joint to joint
                    lambda = subLengths[k] / ray.magnitude;
                    IP[k + 1] = ((1f - lambda) * subChain[k].position) + (lambda * subChain[k + 1].position);
                }
                diff = (subTarget.position - subChain[^1].position).magnitude;
                if (diff < tolerance)
                {
                    outputSubs[sc] = sub.subChains[sc].SBTrans[0].position;
                    break;
                }
                
            }
            outputSubs[sc] = sub.subChains[sc].SBTrans[0].position;
        }

        // calc centroid
        Vector3 centroid = new Vector3();
        for(int j = 0; j < outputSubs.Length; j++)
        {
            centroid += outputSubs[j];
        }
        centroid /= outputSubs.Length;
        sub.transform.position = centroid;

        

    }
    // Update is called once per frame
    void Update()
    {
        FABRIK();
        Transfer();
        //MoveRoot();
        DrawDebugLines();

    }

    void Transfer()
    {
        for (int i = 0; i < inputTransforms.Length; i++)
        {
            transfer.x = IP[i].x;
            transfer.y = IP[i].y;
            transfer.z = IP[i].z;
            inputTransforms[i].position = transfer;
        }
    }
    //this function will add the position of the root to the array of positions
    void MoveRoot()
    {
        Vector3 offset = new Vector3();
        offset.x = inputTransforms[0].position.x;
        offset.y = inputTransforms[0].position.y;
        offset.z = inputTransforms[0].position.z;
        for (int i = 0; i < inputTransforms.Length; i++)
        {
            inputTransforms[i].position = new Vector3(IP[i].x, IP[i].y, IP[i].z) + offset;
        }
        //targetTrans.position = new Vector3(IP[^1].x, IP[^1].y, 0.0f) + offset; ;
    }
    void DrawDebugLines()
    {
        for (int i = 0; i < lengths.Length; i++)
        {
            Color color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));
            if (!useTransformsToGenerate)
                Debug.DrawLine((Vector3)IP[i], (Vector3)IP[i + 1], color);
            else
                Debug.DrawLine((Vector3)inputTransforms[i].position, (Vector3)inputTransforms[i + 1].position, color);
        }
    }
}
