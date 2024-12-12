using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class ObjectCloth : MonoBehaviour
{
    public class Stick
    {
        public Vertex v1, v2;
        public float constraintLength;
        public Stick(Vertex v1, Vertex v2, float constraintLength)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.constraintLength = constraintLength;
        }
    }

    public List<Stick> sticks = new List<Stick>();
    public Mesh clothMesh;
    public Transform cloth;
    public GameObject vertexPrefab;
    public float constraintLength = 1.0f;
    List<Vertex> vertexMap = new List<Vertex>();
    HashSet<string> addedSticks;
    public Vector3 g = new Vector3(0, -10f, 0);
    public float drag = 0;
    public float r = 0;
    public bool constraintMode = false;

    private void Start()
    {
        SetupCloth();
    }

    public void toggleConstraint()
    {
        constraintMode = !constraintMode;
    }
    void SetupCloth()
    {
        if (clothMesh == null)
        {
            Debug.LogError("Cloth Mesh not assigned!");
            return;
        }

        int[] triangles = clothMesh.triangles;
        Vector3[] vertices = clothMesh.vertices;

        // Dictionary to store vertex positions and associated Vertex objects
        vertexMap = new List<Vertex>();
        // HashSet to store already added stick pairs
        addedSticks = new HashSet<string>();

        // Create vertices and populate vertexMap
        for (int i = 0; i< vertices.Length; i++)
        {
            
            var vertexPos = vertices[i];
            GameObject vertexObj = Instantiate(vertexPrefab);
            vertexObj.transform.parent = cloth;
            vertexObj.transform.localPosition = vertexPos;
            
            Vertex vertex = vertexObj.GetComponent<Vertex>();
            if (vertex == null)
            {
                Debug.LogError("Vertex component not found on vertexPrefab!");
                return;
            }
            if (i <6)
            {
                vertex.GetComponent<MeshRenderer>().enabled = true;
                vertex.constrained = true;
            }
            vertex.pos = vertexObj.transform.localPosition;
            vertex.ind = i ;
            vertexMap.Add(vertex);
            
        }

        // Process triangles to create sticks
        for (int i = 0; i < triangles.Length; i += 3)
        {

            var id1 = triangles[i];
            var id2 = triangles[i + 1];
            var id3 = triangles[i + 2];
                     
            AddStick(id1, id2);
            AddStick(id2, id3);
            AddStick(id3, id1);
        }
        cloth.GetComponent<MeshFilter>().sharedMesh = (Mesh) Instantiate(clothMesh);
    }

    void AddStick(int id1, int id2 )
    {
        var stickKey = GetStickKey(id1,id2);
        if (!addedSticks.Contains(stickKey))
        {
            Vertex v1 = vertexMap[id1];
            Vertex v2 = vertexMap[id2];

            float distance = Vector3.Distance(v1.pos, v2.pos);
            Stick stick = new Stick(v1, v2, distance);
            sticks.Add(stick);
            addedSticks.Add(stickKey);
        }

    }
    string GetStickKey(int id1, int id2 )
    {
        if (id1 < id2)
        {
            return $"{id1}-{id2}";
        }
        return $"{id2}-{id1}";

    }

    void Update()
    {
        if (constraintMode)
        {

            return;
        }
        var vertices = cloth.GetComponent<MeshFilter>().sharedMesh.vertices;
        foreach (Transform child in cloth)
        {
            var v1 = child.GetComponent<Vertex>();
            AddGravity(v1);
        }
        foreach (var stick in sticks)
        {
            handleConstraint(stick.v1.GetComponent<Vertex>(), stick.v2.GetComponent<Vertex>(), stick.constraintLength);
        }
        
        foreach (Transform child in cloth)
        {
            var v1 = child.GetComponent<Vertex>();

            if (!v1.constrained)
            {
                HandleCollision(v1);
                v1.updateTransformPos();                
            }
            vertices[v1.ind] = v1.pos;

        }
        cloth.GetComponent<MeshFilter>().sharedMesh.vertices = vertices;
    }

    public List<Transform> handleGrab(SphereCollider collider)
    {
        List<Transform> ret = new List<Transform>();
        foreach (Transform child in transform)
        {
            var v1 = child.GetComponent<Vertex>();
            var hits = Physics.OverlapSphere(transform.TransformPoint(v1.pos), 0f);
            //Handle collision
            if (hits.Length > 0)
            {
                foreach (var hitObject in hits)
                {
                    if (hitObject == collider)
                    {
                        v1.constrained = true;
                        ret.Add(child);
                        break;
                    }
                }
            }
        }
        return ret;
    }
    void handleConstraint(Vertex v1, Vertex v2, float constraintLength)
    {
        // String constraints
        float w1 = 0, w2 = 0;
        var diff = (v2.pos - v1.pos);
        /* if (Math.Abs((diff.magnitude -constraintLength)/ constraintLength) <0.1 )
         {
             return;
         }*/
        if (!(v1.constrained || v2.constrained))
        {
            w1 = 1;
            w2 = 1;
        }
        else if (v1.constrained && !v2.constrained)
        {
            w1 = 0;
            w2 = 1;
        }
        else if (!v1.constrained && v2.constrained)
        {
            w1 = 1;
            w2 = 0;
        }
        else
        {
            return;
        }

        v1.pos = v1.pos + w1 * (diff - diff.normalized * constraintLength) / (w1 + w2);
        v2.pos = v2.pos - w2 * (diff - diff.normalized * constraintLength) / (w1 + w2);

    }
    void AddGravity(Vertex v1)
    {
        // Gravity
        if (!v1.constrained)
        {
            var prev = v1.pos;
            v1.pos = v1.pos + (v1.pos - v1.prevpos) * (1 - drag) + g * (float)Math.Pow(Time.fixedDeltaTime, 2) * (1 - drag);
            v1.prevpos = prev;
        }
    }

    void HandleCollision(Vertex v1)
    {
        var hits = Physics.OverlapSphere(transform.TransformPoint(v1.pos), 0f);
        //Handle collision
        if (hits.Length > 0)
        {
            foreach (var hitObject in hits)
            {
                if (hitObject.CompareTag("Controller"))
                {
                    continue;
                }
                if (hitObject.transform != transform)
                {


                    var ray = new Ray(transform.TransformPoint(v1.prevpos), transform.TransformDirection(v1.pos - v1.prevpos));
                    RaycastHit hit;
                    if (ray.direction.magnitude != 0)
                    {
                        if (hitObject.Raycast(ray, out hit, 3f))
                        {
                            var prev = v1.pos;
                            var normal = transform.InverseTransformDirection(hit.normal);
                            var collisionPointIdeal = transform.InverseTransformPoint(hit.point);
                            if (Math.Abs(Vector3.Dot(v1.pos - v1.prevpos, normal)) > 0.5f)
                            {
                                v1.pos = (v1.pos) + (1 + r) * Vector3.Dot((collisionPointIdeal - (v1.pos)), normal) * normal;
                                v1.prevpos = v1.prevpos - (1 + r) * Vector3.Dot((v1.prevpos) - collisionPointIdeal, normal) * normal;
                            }
                            else
                            {
                                v1.pos = v1.prevpos;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
