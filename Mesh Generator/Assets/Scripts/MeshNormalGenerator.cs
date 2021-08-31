using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MeshNormalGenerator : MonoBehaviour
{
    public class Triangle
    {
        public List<Vector3> vertices = new List<Vector3>();
        public Vector3 normal = Vector3.up;
        public Vector3 centroid = Vector3.zero;
    }
    
    public CEvent_Vector3 updateCameraNotification;
    public GameObject vertexPrefab;
    public GameObject normalPrefab;
    private const int Dim = 10;
    private const float RemapMin = -2f;
    private const float RemapMax = 2f;
    private float _sampleMin = 0f;
    private float _sampleMax = 1f;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private MeshFilter _meshFilter;
    
    
    private List<GameObject> _vertobjs = new List<GameObject>();
    private List<GameObject> _normobjs = new List<GameObject>();
    private List<Triangle> _triobjs = new List<Triangle>();

    // Start is called before the first frame update
    void Start()
    {
        _mesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = _mesh;
        
        CreateVerts();
        CreateTris();
        UpdateMesh();
        
        updateCameraNotification.Raise(new Vector3((Dim - 1)/2f, 0, (Dim - 1)/2f));
        
        DrawTriangleEdges();
        DrawTriangleNormals();
        DrawVerticeNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void CreateVerts()
    {
        List<Vector3> vertlist = new List<Vector3>();
        
        for (int z = 0; z < Dim; ++z)
        {
            for (int x = 0; x < Dim; ++x)
            {
                Vector3 newvert = new Vector3(x, 0, z);

                float y = 0;

                y = SamplePerlinNoise(x, z);
                
                newvert.y = y;
                vertlist.Add(newvert);
            }
        }

        // Debug.Log("Mesh has " + vertlist.Count + " vertices" );
        _vertices = vertlist.ToArray();
    }

    private void CreateTris()
    {
        if (_triobjs.Count > 0) _triobjs.Clear();
        
        // checkpointNotification.Raise("Generating " + (_data.dimension + 2) * (_data.dimension + 2) + " polygons...");
        List<int> trilist = new List<int>();

        for( int z = 0; z < Dim - 1; ++z )
        {
            int offset = z * Dim; // offset
            for (int x = 0; x < Dim - 1; ++x)
            {
                int bl = x + offset;
                int tl = x + Dim + offset;
                int tr = x + Dim + offset + 1;
                int br = x + offset + 1;
                
                // left tri
                trilist.Add(bl);
                trilist.Add(tl);
                trilist.Add(br);
                Triangle left = new Triangle();
                left.vertices.Add(_vertices[bl]);
                left.vertices.Add(_vertices[tl]);
                left.vertices.Add(_vertices[br]);
                left.centroid = Putils.CalculateTriangleCentroid(left.vertices[0], left.vertices[1], left.vertices[2]);
                left.normal = Putils.CalculateTriangleNormal(left.vertices[0], left.vertices[1], left.vertices[2]);
                
                // right tri
                trilist.Add(br);
                trilist.Add(tl);
                trilist.Add(tr);
                Triangle right = new Triangle();
                right.vertices.Add(_vertices[br]);
                right.vertices.Add(_vertices[tl]);
                right.vertices.Add(_vertices[tr]);
                right.centroid =
                    Putils.CalculateTriangleCentroid(right.vertices[0], right.vertices[1], right.vertices[2]);
                right.normal = Putils.CalculateTriangleNormal(right.vertices[0], right.vertices[1], right.vertices[2]);
                
                _triobjs.Add(left);
                _triobjs.Add(right);
            }
        }
        
        // Debug.Log("Mesh has " + trilist.Count / 3 + " triangles");
        _triangles = trilist.ToArray();
    }
    
    public float SamplePerlinNoise(int x, int z)
    {
        float xSamplePoint = Putils.Remap(x, 0, Dim, _sampleMin, _sampleMax);
        float zSamplePoint = Putils.Remap(z, 0, Dim, _sampleMin, _sampleMax);
        
        // float zSamplePoint = _data.perlin.sampleMax + zMapProportion;

        float sample;
        
        float perlin = Mathf.PerlinNoise(xSamplePoint, zSamplePoint);
        sample = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1
        
        float rmsample = Putils.Remap(sample, 0, 1, RemapMin, RemapMax);

        return rmsample;
    }
    
    private void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;

        _mesh.RecalculateNormals();
    }

    private void DrawTriangleEdges()
    {
        if (_vertobjs.Count > 0)
        {
            EmptyVisualVertList();
        }
        
        var grid = Putils.FlatArrayToTwoDArray(_vertices, Dim, Dim);
        for (int y = 0; y < Dim; ++y)
        {
            for (int x = 0; x < Dim; ++x)
            {
                var vert = Instantiate(vertexPrefab, grid[x, y], vertexPrefab.transform.rotation);

                LineRenderer lr = vert.GetComponent<LineRenderer>();
                if (x + 1 < Dim && y + 1 < Dim)
                {
                    lr.SetPosition(0, grid[x, y + 1]);
                    lr.SetPosition(1, grid[x, y]);
                    lr.SetPosition(2, grid[x + 1, y]);
                    lr.SetPosition(3, grid[x, y + 1]);
                }
                else if ( x + 1 >= Dim && y + 1 < Dim )
                {
                    lr.positionCount = 2;
                    lr.SetPosition(0, grid[x, y]);
                    lr.SetPosition(1, grid[x, y + 1]);
                }
                else if (y + 1 >= Dim && x + 1 < Dim)
                {
                    lr.positionCount = 2;
                    lr.SetPosition(0, grid[x, y]);
                    lr.SetPosition(1, grid[x + 1, y]);
                }
                else
                {
                    lr.positionCount = 0;
                }
                
                _vertobjs.Add(vert);
            }
        }
    }

    private void DrawTriangleNormals()
    {
        if (_normobjs.Count > 0) EmptyVisualNormList();
        
        int griddim = (Dim - 1) * 2;
        var grid = Putils.FlatArrayToTwoDArray(_triobjs.ToArray(), griddim, Dim - 1);

        for (int y = 0; y < Dim - 1; ++y)
        {
            for (int x = 0; x < griddim; ++x)
            {
                grid[x, y].centroid = Putils.CalculateTriangleCentroid(grid[x, y].vertices[0], grid[x, y].vertices[1],
                    grid[x, y].vertices[2]);
                grid[x, y].normal = Putils.CalculateTriangleNormal(grid[x, y].vertices[0], grid[x, y].vertices[1],
                    grid[x, y].vertices[2]);
                
                var norm = Instantiate(normalPrefab, grid[x, y].centroid, normalPrefab.transform.rotation);
                LineRenderer lr = norm.GetComponent<LineRenderer>();
                lr.SetPosition(0, grid[x, y].centroid);
                lr.SetPosition(1, grid[x, y].centroid + grid[x, y].normal * 0.25f);
                
                _normobjs.Add(norm);
            }
        }
    }

    public void ResampleMesh()
    {
        Debug.Log("Resampling Mesh...");
        
        _sampleMin = UnityEngine.Random.Range(-250f, 250f);
        _sampleMax = _sampleMin + UnityEngine.Random.Range(0.5f, 1f);
        
        CreateVerts();
        CreateTris();

        DrawTriangleNormals();
        DrawTriangleEdges();
        DrawVerticeNormals();

        var mfi = _meshFilter.mesh;
        mfi.Clear();
        mfi.vertices = _vertices;
        mfi.triangles = _triangles;
        mfi.RecalculateNormals();
    }

    private void EmptyVisualVertList()
    {
        foreach (var vert in _vertobjs)
        {
            Destroy(vert);
        }
        _vertobjs.Clear();
    }
    
    private void EmptyVisualNormList()
    {
        foreach (var norm in _normobjs)
        {
            Destroy(norm);
        }
        _normobjs.Clear();
    }

    private void DrawVerticeNormals()
    {
        var grid = Putils.FlatArrayToTwoDArray(_vertices, Dim, Dim);
        var vertgrid = Putils.FlatArrayToTwoDArray(_vertobjs.ToArray(), Dim, Dim);

        for (int y = 0; y < Dim; ++y)
        {
            for (int x = 0; x < Dim; ++x)
            {
                Vector3 center = Vector3.zero, 
                    up = Vector3.zero, 
                    right = Vector3.zero, 
                    downright = Vector3.zero, 
                    down= Vector3.zero, 
                    left= Vector3.zero, 
                    upleft = Vector3.zero;
                
                center = grid[x, y];
                
                // look up
                if (y + 1 < Dim)
                {
                    up = grid[x, y + 1];
                    
                    // look up and to the left
                    if (x - 1 >= 0) upleft = grid[x - 1, y + 1];
                }

                // look to the right
                if (x + 1 < Dim)
                {
                    right = grid[x + 1, y];
                    // look to the right and down
                    if (y - 1 >= 0)
                    {
                        downright = grid[x + 1, y - 1];
                        down = grid[x, y - 1];
                    }
                }
                
                // look up
                if (y - 1 >= 0)  down = grid[x, y - 1];
                
                // look down
                if (x - 1 >= 0) left = grid[x - 1, y];

                Vector3 vertnorm = Vector3.zero;

                // make sure that none of the points are zero
                // if they are, we don't want to include it in the sum
                // remember the left hand rule, thumb up
                if (upleft != Vector3.zero && up != Vector3.zero) 
                    vertnorm += Vector3.Cross(upleft - center, up - center);
                
                if (up != Vector3.zero && right != Vector3.zero) 
                    vertnorm += Vector3.Cross(up - center, right - center);
                
                if (right != Vector3.zero && downright != Vector3.zero) 
                    vertnorm += Vector3.Cross(right - center, downright - center);
                
                if (downright != Vector3.zero && down != Vector3.zero) 
                    vertnorm += Vector3.Cross(downright - center, down - center);
                
                if (down != Vector3.zero && left != Vector3.zero) 
                    vertnorm += Vector3.Cross(down - center, left - center);
                
                if (left != Vector3.zero && upleft != Vector3.zero) 
                    vertnorm += Vector3.Cross(left - center, upleft - center);

                // normalize and make the normal a good size to show
                vertnorm = vertnorm.normalized * 0.25f;

                // set the line renderer points
                LineRenderer lr = vertgrid[x, y].transform.Find("Normal").GetComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, grid[x, y]); // and the vertex
                lr.SetPosition(1, grid[x, y] + vertnorm); // at the end of the normal
            }
        }
    }
}
