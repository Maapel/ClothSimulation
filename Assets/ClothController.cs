using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ClothController : MonoBehaviour
{
    public int w, h;
    public float constraint; 
    public GameObject vertexPrefab;
    public Vector3 g = new Vector3(0, -10f , 0);
    public float drag=0;
    public float r= 0;
    List<Transform[]> sticks = new List<Transform[]>();
    List<Transform[]> sticks_diag = new List<Transform[]>();

    void Start()
    {
        GetComponent<ClothMeshCreator>().w = w;
        GetComponent<ClothMeshCreator>().h = h ;

        CreateCloth();    
    }
    void Update()
    {
        foreach (Transform child in transform)
        {
            var v1 = child.GetComponent<Vertex>();
            AddGravity(v1);


        }
        foreach ( var stick in sticks )
        {
            handleConstraint(stick[0].GetComponent<Vertex>() , stick[1].GetComponent<Vertex>(), constraint);
        }
        foreach (var stick in sticks_diag)
        {
            handleConstraint(stick[0].GetComponent<Vertex>(), stick[1].GetComponent<Vertex>(), constraint * (float) Math.Sqrt(2));
        }
       
        foreach ( Transform child in transform)
        {
            var v1 = child.GetComponent<Vertex>();
            
            if (!v1.constrained)
            {
                HandleCollision(v1);
                v1.updateTransformPos();
            }
        }
    }
    public List<Transform> handleGrab(SphereCollider collider)
    {
        List<Transform> ret = new List<Transform>();
        foreach(Transform child in transform)
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
    void CreateCloth()
    {
        
        for(int j = 0; j <= h; j++) { 
            for (int i = 0; i <= w; i++)
                {
                    var obj =Instantiate(vertexPrefab);
                obj.transform.parent = transform;
                obj.transform.localPosition = i*constraint*transform.right - j * transform.up*constraint;
                if (i != 0)
                {
                    sticks.Add(new Transform[2] { transform.GetChild((i - 1) + j*(w + 1)), transform.GetChild(i + j*(w + 1)) });

                }
                if (j != 0)
                {
                    sticks.Add(new Transform[2] { transform.GetChild((j - 1)*(w+1) + i ), transform.GetChild(j*(w+1) + i ) });

                }
                else
                {
                    if (i == 0|| i==w)
                    {
                        obj.GetComponent<Vertex>().constrained = true;
                    }
                }
                if(i!=0 && j != 0)
                {
                    sticks_diag.Add(new Transform[2] { transform.GetChild(i - 1 + (j-1) *( w+1)), transform.GetChild(i + j * (w+1)) });
                    sticks_diag.Add(new Transform[2] { transform.GetChild(i - 1 + j * (w + 1)), transform.GetChild(i + (j-1) * (w + 1)) });
                }
            }
        }

    }
    void handleConstraint(Vertex v1 , Vertex v2  , float constraintLength)
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
            v1.pos =  v1.pos + (v1.pos - v1.prevpos)*(1-drag) + g * (float)Math.Pow(Time.fixedDeltaTime, 2)* (1-drag);
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
