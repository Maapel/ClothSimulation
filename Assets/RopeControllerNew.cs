using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class RopeControllerNew : MonoBehaviour
{
    [Tooltip("Stiffness of rope")]
    public float k;
    public float constraintLength;
    public Transform Rope;
    public int N;
    public float frictionConst;
    public GameObject VertexPrefab;
    public Vector3 g;
    [Tooltip("Coefficient of Restitution")]
    public float r;
    float t = 0;
    float dt = 0;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < N; i++)
        {
            var obj = Instantiate(VertexPrefab, Rope);
            if (i == 0)
            {
                obj.GetComponent<Vertex>().constrained = true;
            }
            obj.transform.localPosition = g * constraintLength * i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        dt += Time.deltaTime;
        if (dt > 0.001f)
        {
            solveForce();
            dt = 0;
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
                        var prev = v1.prevpos;
                        var normal = transform.InverseTransformDirection(hit.normal);

                        v1.force = -normal * Vector3.Dot(v1.force, normal);
                        v1.applyForce(dt);
                        v1.prevpos = prev;
                    }
                     ray = new Ray(transform.TransformPoint(v1.prevpos), transform.TransformDirection(v1.pos - v1.prevpos));
                    
                    if (hitObject.Raycast(ray, out hit, 3f))
                    {
                        var normal = transform.InverseTransformDirection(hit.normal);
                        var collisionPointIdeal = transform.InverseTransformPoint(hit.point) + normal * v1.radius;
                        if (Math.Abs(Vector3.Dot(-v1.pos + v1.prevpos, normal)) >= v1.radius)
                        {
                            v1.pos = (v1.pos) + (1 + r) * Vector3.Dot((collisionPointIdeal - (v1.pos)), normal) * normal;
                            v1.prevpos = v1.prevpos - (1 + r) * Vector3.Dot((v1.prevpos) - collisionPointIdeal, normal) * normal;
                        }
                        else
                        {
                            var closestPoint = hit.collider.ClosestPointOnBounds(transform.TransformPoint(v1.prevpos));
                            var closestNormal = transform.InverseTransformDirection(closestPoint - transform.TransformPoint(v1.prevpos));
                            if (Vector3.Dot(closestNormal, normal)<0)
                            {
                                closestNormal = - closestNormal;
                            }
                            v1.prevpos =v1.pos ;
                            v1.pos = transform.InverseTransformPoint(closestPoint) + closestNormal * v1.radius;
                                
                        }
                        
                    };
                    break;
                }
            }
        }
    }
    void solveForce()
    {
        foreach (Transform child in Rope)
        {
            var v = child.GetComponent<Vertex>();

            var force = g * v.mass;
            v.force = Vector3.zero;

            force += -(v.pos - v.prevpos)*frictionConst / dt; 
            v.force += force;
            //Spring Force


        }
        
        foreach (Transform child in Rope)
        {
            var v = child.GetComponent<Vertex>();
            v.applyForce(dt);
            
        }
        

        for (int i = 0; i < k; i++)
        {
            var prev_v = Rope.GetChild(0).GetComponent<Vertex>();

            foreach (Transform child in Rope)
            {
                   
                var v = child.GetComponent<Vertex>();
                if(v==prev_v) {
                
                    continue;
                }
                AlignVertices(prev_v, v);
                



                //Spring Force
                prev_v = v;

            }
        }

        foreach (Transform child in Rope)
        {
            var v = child.GetComponent<Vertex>();

            if (!v.constrained)
            {
                HandleCollision(v);
                v.updateTransformPos();
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
}
