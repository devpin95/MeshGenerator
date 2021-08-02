using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCameraTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(2.5f, 0, 2.5f));
    }
}
