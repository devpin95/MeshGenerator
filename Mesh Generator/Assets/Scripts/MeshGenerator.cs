using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private List<GameObject> _visualVerts = new List<GameObject>();

    public int xSize = 2;
    public int zSize = 2;

    // Start is called before the first frame update
    void Start()
    {
       
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        CreateShape();
        UpdateMesh();
    }
    
    private void CreateShape()
    {
        CreateVerts();
        CreateTris();
    }

    private void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        
        _mesh.RecalculateNormals();
    }

    public void OnDrawGizmos()
    {
        if (_vertices == null) return;

        int i = 0;
        foreach (var vert in _vertices)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vert, .01f);
            Handles.Label(vert, i.ToString());

            ++i;
        }
    }

    private void CreateVerts()
    {
        List<Vector3> vertlist = new List<Vector3>();
        
        // create vertices
        for (int z = 0; z <= zSize; ++z)
        {
            for (int x = 0; x <= xSize; ++x)
            {
                vertlist.Add(new Vector3(x, 0, z));
            }
        }

        _vertices = vertlist.ToArray();
    }

    private void CreateTris()
    {
        List<int> trilist = new List<int>();
        
        for( int z = 0; z < zSize; ++z )
        {
            int y = z * (xSize + 1); // offset
            for (int x = 0; x < xSize; ++x)
            {
                int bl = x + y;
                int tl = x + xSize + y + 1;
                int tr = x + xSize + y + 2;
                int br = x + y + 1;
                
                // left tri
                trilist.Add(bl);
                trilist.Add(tl);
                trilist.Add(br);
                
                // right tri
                trilist.Add(br);
                trilist.Add(tl);
                trilist.Add(tr);
                
                // Debug.Log("Creating triangles at <" + bl + ", " + tl + ", " + br + "> and <" + br + ", " + tl + ", " + tr + ">");
            }
        }
        
        _triangles = trilist.ToArray();
    }

    public void ShowVisualVertices(bool show)
    {
        if (show)
        {
            foreach (var vert in _vertices)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                obj.transform.position = vert;
                _visualVerts.Add(obj);
            }
        }
        else
        {
            foreach (var vert in _visualVerts)
            {
                Destroy(vert);
            }
        }
    }
}
