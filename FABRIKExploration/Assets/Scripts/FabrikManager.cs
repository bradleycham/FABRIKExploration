using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabrikManager : MonoBehaviour
{
    // Start is called before the first frame update
    public SubBase[] subBases;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //run the subase agirithms in reverse
        //Ie: starting at the end subbase, run the forward k
        //then moving up run the next up
        //until you get to the root

        // then run forward from the root to the end effectors

    }
}
