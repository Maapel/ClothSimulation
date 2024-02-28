using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothMeshCreator : MonoBehaviour
{
    // Start is called before the first frame update
    public int w, h;
    public float thickness;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        GetComponent<MeshFilter>().mesh =  GenerateClothMesh(transform);
    }
    Mesh GenerateClothMesh(Transform parent)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        var prev = parent.GetChild(1).localPosition;
        Vector3 normal = Vector3.zero;
        foreach (Transform emptyObject in parent)
        {
            vertices.Add(emptyObject.localPosition);
        }
        for (int j = 0; j < h; j++)  
        {
            for (int i = 0; i < w; i++)
            {
                parent.GetChild(i + j * (w + 1)).forward = Vector3.Cross( parent.GetChild(i + (j + 1) * (w + 1)).localPosition - parent.GetChild(i + j * (w + 1)).localPosition , parent.GetChild(i + 1 + j * (w + 1)).localPosition - parent.GetChild(i + j * (w + 1)).localPosition) ;
                triangles.Add(i+ j * (w+1));
                triangles.Add(i + 1 + j * (w+1));
                triangles.Add(i + 1 + (j+1) * (w + 1));
                triangles.Add(i + j * (w + 1));
                triangles.Add(i + 1 + (j + 1) * (w + 1));
                triangles.Add(i  + (j + 1) * (w + 1));


            }
        }
        foreach (Transform emptyObject in parent)
        {
            vertices.Add(emptyObject.localPosition + emptyObject.transform.forward * thickness);
        }
        var bias = 2*(w + 1) * (h + 1)-1 ;
        for (int j = 0; j < h; j++)  
        {
            for (int i = 0; i < w; i++)
            {
                triangles.Add(bias - (i + (j + 1) * (w + 1)));
                triangles.Add(bias - (i + 1 + (j + 1) * (w + 1)));
                triangles.Add(bias - (i + j * (w + 1)));

                triangles.Add(bias - (i + 1 + (j + 1) * (w + 1)));
                triangles.Add(bias - (i + 1 + j * (w + 1)));
                triangles.Add(bias   - (i + j * (w + 1)));
               
                




            }
        }

        // Create mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
