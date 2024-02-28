using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RopeCreator : MonoBehaviour
{
    public float radius = 0.1f; // Radius of the rope
    public int segments = 10; // Number of segments around the circumference of the rope
    public Material ropeMaterial; // Material for the rope
    GameObject ropeObject;
    
    void Start()
    {
        
        // Create empty GameObjects along the path

        // Create rope mesh
        //Mesh mesh = GenerateRopeMesh(transform);

        // Create GameObject to hold the mesh
        //GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = ropeMaterial;
    }

    private void Update()
    {
        Mesh mesh = GenerateRopeMesh(transform);
        GetComponent<MeshFilter>().mesh = mesh;

        //GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    /*List<GameObject> Createparent()
    {

        List<GameObject> parent = new List<GameObject>();

        foreach (Vector3 point in points)
        {
            GameObject emptyObject = new GameObject("EmptyObject");
            emptyObject.transform.parent = transform;
            emptyObject.transform.position = point;
            parent.Add(emptyObject);
        }

        return parent;
    }*/

    Mesh GenerateRopeMesh(Transform parent)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        var prev = parent.GetChild(1).localPosition;
        Vector3 normal = Vector3.zero;
        foreach ( Transform emptyObject in parent)
        {
            emptyObject.GetComponent<Vertex>().radius = radius;
            /*if ( !emptyObject.GetComponent<SphereCollider>())
            {
                var collider = emptyObject.AddComponent<SphereCollider>();
                collider.radius = radius;
            }*/
            Vector3 center = emptyObject.localPosition;
            if (vertices.Count == 0)
            {
                normal = -(center - prev).normalized;
            }
            else if (Vector3.Dot(center - prev, normal) < 0)
            {
                normal = Vector3.Cross(Vector3.Cross(normal, center - prev), normal).normalized;
            }
            else
            {
                normal = (center - prev).normalized;
            }
            
           
            emptyObject.transform.up = normal;
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector3 offset = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, normal) * emptyObject.transform.forward * radius;
                Vector3 vertex = center + offset;

                vertices.Add(vertex);
                if (vertices.Count> segments)
                {
                    int baseIndex = vertices.Count - segments-1;
                    triangles.Add(baseIndex);
                    if ((baseIndex + 1) % segments == 0)
                    {
                        triangles.Add(baseIndex + 1 - segments);
                        triangles.Add(baseIndex + 1);

                    }
                    else
                    {
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 1 + segments);
                    }

                    triangles.Add(baseIndex);
                    if ((baseIndex + 1) % segments == 0)
                    {
                        triangles.Add((baseIndex + 1));
                        triangles.Add(baseIndex + segments);
                    }
                    else
                    {
                        triangles.Add((baseIndex + 1 + segments));
                        triangles.Add(baseIndex + segments);
                    }
                }
            }
            prev = emptyObject.transform.localPosition;
        }

        // Create mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
