using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public GameObject target;
    private MeshGenerator _targetMG;
    private Vector3 _targetOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        _targetMG = target.GetComponent<MeshGenerator>();
        _targetOffset = new Vector3(_targetMG.xSize / 2f, 0, _targetMG.zSize / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target.transform.position + _targetOffset);
    }
}
