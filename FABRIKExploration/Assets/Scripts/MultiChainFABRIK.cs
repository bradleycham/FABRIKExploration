using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiChainFABRIK : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform[] inputTransforms;
    Vector3 transfer;
    //Vector3 offset;

    public SubBase[] subBases;

    //Vector3 targetPos;
    Vector3 basePos;
    public Transform targetTrans;

    float targetDistFromRoot;
    int maxIterations = 10;
    public float tolerance = 0.05f;

    float diff; // the distance between the end effector and the target
    float lambda;

    // Set up data at start
    float chainLength;
    float[] lengths; // one less than the length of IP
    Vector3[] vecJoints;


    void Start()
    {
        SetUpPointsAndLengths();
    }

    void SetUpPointsAndLengths()
    {
        // Vector3 math is faster than getting transform positions all the time

        //set the length of vecJoints[amount of input transforms]
        vecJoints = new Vector3[inputTransforms.Length];

        for (int i = 0; i < inputTransforms.Length; i++)
        {
            vecJoints[i].x = inputTransforms[i].position.x;
            vecJoints[i].y = inputTransforms[i].position.y;
            vecJoints[i].z = inputTransforms[i].position.z;
        }

        //set up lengths to be one less than amount of joints
        lengths = new float[vecJoints.Length - 1];
        for (int i = 0; i < lengths.Length; i++)
        {
            lengths[i] = (vecJoints[i + 1] - vecJoints[i]).magnitude;
        }

        //Calculate Max length  
        chainLength = 0f;
        for (int i = 0; i < lengths.Length; i++)
        {
            chainLength += lengths[i];
        }
    }

    void CheckTargetDistance(Vector3 targetPos, Vector3 root)
    {
        targetDistFromRoot = (targetPos - root).magnitude; // set the distance of the root to target to the mag of a vector between the two
    }

    void FABRIK()
    {
        //targetPos = targetTrans.position;
        CheckTargetDistance(targetTrans.position, vecJoints[0]);

        if (targetDistFromRoot > chainLength + tolerance)
        {
            Vector3 rayToTarget = targetTrans.position - vecJoints[0];
            for (int j = 0; j < lengths.Length; j++)
            {
                vecJoints[j + 1] = vecJoints[j] + lengths[j] * rayToTarget.normalized;
            }
        }
        else
        {
            for (int i = 0; i < maxIterations; i++) // the 'while' loop replacing the tolerance bs
            {
                // Perform forward backward while the end efector is not within the range of target
                //set root position on each iteration
                basePos = vecJoints[0];               

                ForwardFabrik(vecJoints,lengths,targetTrans.position);
                BackwardFabrik();
                CheckDiff(targetTrans.position, vecJoints[^1]);
                
            }
        }

    }

    // iterate from the end effector backwards to the subbases/root
    void ForwardFabrik(Vector3[] joints, float[] lengths, Vector3 target)
    {       
        // STAGE 1 forward
        joints[^1] = target;
        //find the dstance between the new joint and the -1 joint
        for (int p = lengths.Length - 1; p > 0; p--)
        {
            Vector3 ray = joints[p + 1] - joints[p]; //ray from last to joint to next joint
            lambda = lengths[p] / ray.magnitude;
            //IP[p] = IP[p+1] + ((1f - lambda) * ray); // find the new position
            joints[p] = ((1f - lambda) * joints[p + 1]) + (lambda * joints[p]);

        }
    }

    Vector3 ForwardFabrikSub(Vector3[] joints, float[] lengths, Vector3 target)
    {
        // STAGE 1 forward
        joints[^1] = target;
        //find the dstance between the new joint and the -1 joint
        for (int p = lengths.Length - 1; p > 0; p--)
        {
            Vector3 ray = joints[p + 1] - joints[p]; //ray from last to joint to next joint
            lambda = lengths[p] / ray.magnitude;
            //IP[p] = IP[p+1] + ((1f - lambda) * ray); // find the new position
            joints[p] = ((1f - lambda) * joints[p + 1]) + (lambda * joints[p]);

        }
        return joints[0];
    }

    void BackwardFabrik()
    {
        // STAGE 2 Backwards
        vecJoints[0] = basePos;
        for (int k = 0; k < lengths.Length; k++)
        {
            Vector3 ray = vecJoints[k + 1] - vecJoints[k]; //ray from next joint to joint
            lambda = lengths[k] / ray.magnitude;
            //IP[k + 1] = IP[k] + ((1f - lambda) * ray);
            vecJoints[k + 1] = ((1f - lambda) * vecJoints[k]) + (lambda * vecJoints[k + 1]);
        }
    }
    void CheckDiff(Vector3 targetPos, Vector3 endEffector)
    {
        //check if the end effector is withing the tolerance of the target
        diff = (targetPos - endEffector).magnitude;
        if (diff < tolerance) // if it is then stop iterating
            return;
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
            transfer.x = vecJoints[i].x;
            transfer.y = vecJoints[i].y;
            transfer.z = vecJoints[i].z;
            inputTransforms[i].position = transfer;
        }
    }
    //this function will add the position of the root to the array of positions
    void MoveRoot()
    {
        Vector3 offset = new Vector3();
        offset.x = this.transform.position.x;
        offset.y = this.transform.position.y;
        offset.z = this.transform.position.z;
        for (int i = 0; i < inputTransforms.Length; i++)
        {
            inputTransforms[i].position = new Vector3(vecJoints[i].x, vecJoints[i].y, vecJoints[i].z) + offset;
        }
        //targetTrans.position = new Vector3(IP[^1].x, IP[^1].y, 0.0f) + offset;
    }
    void DrawDebugLines()
    {
        for (int i = 0; i < lengths.Length; i++)
        {
            //laser light show
            Color color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));
            
            // draw lines
            Debug.DrawLine((Vector3)inputTransforms[i].position, (Vector3)inputTransforms[i + 1].position, color);
        }
    }
}
