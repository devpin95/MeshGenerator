using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform target;
    public bool lookOpposite = false;
    
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
    }
}
