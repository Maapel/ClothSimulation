using System;
using System.Collections;
using UnityEngine;

public class RopeController : MonoBehaviour
{
    public float damping;
    public float constraintLength;
    public Transform Rope ;
    public int N;
    public GameObject VertexPrefab;
    public Vector3 g;
    public float r;
    int i = 0;
    float t = 0;
    float dt;
    // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i < N; i++) {
            var obj = Instantiate(VertexPrefab, Rope);
            if(i==0)
            {
                obj.GetComponent<Vertex>().constrained = true;
            }
            obj.transform.localPosition = g.normalized *constraintLength*i ;
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        dt = Time.time - t;
        if (dt > 0.01f)
        {
            for (int i = 0; i < N; i++)
            {

                var v1 = Rope.GetChild(i).GetComponent<Vertex>();

                AddGravity(v1);

            }
            t = Time.time;
        }
        for (int i = 0; i < N - 1; i++)
        {
            var v1 = Rope.GetChild(i).GetComponent<Vertex>();
            var v2 = Rope.GetChild(i + 1).GetComponent<Vertex>();
            AlignVertices(v1, v2);

        }
        for (int i = 0; i < N; i++)
        {

            var v1 = Rope.GetChild(i).GetComponent<Vertex>();
            if (!v1.constrained)
            {
                HandleCollision(v1);
                v1.updateTransformPos();
            }

        }
        /*var v1 = Rope.GetChild(i).GetComponent<Vertex>();

        AddGravity(v1);
        if (i < N - 1)
        {
            var v2 = Rope.GetChild(i + 1).GetComponent<Vertex>();
            AlignVertices(v1, v2);
        }
        i++;
        i = i % N;*/
    }

    void AddGravity(Vertex v1)
    {
        // Gravity
        if (!v1.constrained)
        {

            
            var prev = v1.pos;
            
                v1.pos = v1.pos + (v1.pos - v1.prevpos) * (1 - damping) + g * (1 - damping) * dt*dt;
                
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

                if (hitObject.transform != transform)
                {


                    var ray = new Ray(transform.TransformPoint(v1.prevpos), transform.TransformDirection(v1.pos - v1.prevpos));
                    RaycastHit hit;
                    if (hitObject.Raycast(ray, out hit, 3f))
                    {
                        var prev = v1.pos;
                        var normal = transform.InverseTransformDirection(hit.normal);
                        var collisionPointIdeal = transform.InverseTransformPoint(hit.point) + normal*v1.radius;
                        if (Math.Abs(Vector3.Dot(v1.pos - v1.prevpos, normal)) > 0.5f* v1.radius)
                        {
                            v1.pos = (v1.pos) + (1 + r) * Vector3.Dot((collisionPointIdeal - (v1.pos)), normal) * normal;
                            v1.prevpos = v1.prevpos - (1 + r) * Vector3.Dot((v1.prevpos) - collisionPointIdeal, normal) * normal;
                        }
                        else
                        {
                            v1.pos = v1.prevpos;
                        }
                    };
                    break;
                }
            }
        }
    }
    void AlignVertices(Vertex v1, Vertex v2)
    {

       
       
        

        // String constraints
        float w1 = 0, w2 = 0;
        var diff = (v2.pos - v1.pos);
       /* if (Math.Abs((diff.magnitude -constraintLength)/ constraintLength) <0.1 )
        {
            return;
        }*/
        if (!(v1.constrained||v2.constrained))
        {
            w1 = 1;
            w2 = 1;
        }
        else if(v1.constrained && !v2.constrained) {
            w1 = 0;
            w2 = 1;
        }
        else if (!v1.constrained && v2.constrained)
        {
            w1 = 1;
            w2 =0;
        }
        else {
            return;
        }

       
        
        v1.pos = v1.pos + w1 * ( diff -  diff.normalized * constraintLength)/(w1+w2);
        v2.pos = v2.pos - w2 * (diff - diff.normalized * constraintLength) / (w1 + w2);

        

        
        
        /*if (!v1.constrained)
        {
            v1.updateTransformPos();
        }
        if (!v2.constrained)
        {
            v2.updateTransformPos();
        }*/
    }
}
