using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lattice : MonoBehaviour
{
    public static Lattice Instance = null;
    private float[,] _lattice;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void GenerateLattice(int dim, int seed)
    {
        Random.InitState(seed);
        
        _lattice = new float[dim, dim];
        
        for (int x = 0; x < dim; ++x)
        {
            for (int y = 0; y < dim; ++y)
            {
                _lattice[x, y] = Random.Range(0.0f, 1.0f);
            }
        }
    }

    public float SampleLattice(int x, int z)
    {
        return _lattice[x, z];
    }
}
