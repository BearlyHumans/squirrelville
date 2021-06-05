  
using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class BF_MeshExtrusion : MonoBehaviour {

    public Mesh originalMesh;
    private MeshFilter meshFilter;

    public float offsetValue = 1f;
    private float offsetValueMem = 1f;
    public int numberOfStacks = 1;
    private int numberOfStacksMem = 1;
    private int[] oldTri;
    private Vector3[] oldVert;
    private Vector3[] oldNorm;
    private Vector2[] oldUV;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertexs = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> cols = new List<Color>();
    
    void Awake()
    {
        CheckValues();
        BuildGeometry();
    }

    private void OnEnable()
    {
        CheckValues();
    }

    private void Update()
    {
        if(offsetValueMem != offsetValue || numberOfStacks != numberOfStacksMem)
        {
            ClearGeometry();
            BuildGeometry();
            offsetValueMem = offsetValue;
            numberOfStacksMem = numberOfStacks;
        }
    }

    private void CheckValues()
    {
        offsetValueMem = offsetValue;
        numberOfStacksMem = numberOfStacks;
        meshFilter = gameObject.GetComponent<MeshFilter>();
        oldTri = originalMesh.triangles;
        oldVert = originalMesh.vertices;
        oldNorm = originalMesh.normals;
        oldUV = originalMesh.uv;
    }

    private void ClearGeometry()
    {
        triangles.Clear();
        triangles.TrimExcess();
        vertexs.Clear();
        vertexs.TrimExcess();
        uvs.Clear();
        uvs.TrimExcess();
        cols.Clear();
        cols.TrimExcess();
    }

    private void BuildGeometry()
    {
        if(meshFilter == null)
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
        }
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int faces = Mathf.Min(numberOfStacks,100);
        for (int i = 0; i < faces; i++)
        {
            int triangleOffset = i * oldVert.Length;
            int indexNewV = 0;
            foreach (Vector3 v in oldVert)
            {
                vertexs.Add(v + (oldNorm[indexNewV]) * offsetValue*0.01f * i);
                uvs.Add(oldUV[indexNewV]);
                cols.Add(new Color(1 * ((float)i / (float)(faces-1)), 1 * ((float)i / (float)(faces - 1)), 1 * ((float)i / (float)(faces - 1))));

                indexNewV++;
            }
            indexNewV = 0;
            foreach (int innt in oldTri)
            {
                triangles.Add(oldTri[indexNewV] + triangleOffset);

                indexNewV++;
            }
        }
        mesh.vertices = vertexs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = cols.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }
}