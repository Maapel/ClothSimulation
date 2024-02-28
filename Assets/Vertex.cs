using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public Vertex connectedVertex;
    public bool constrained = false;
    public Vector3 pos ; 
    public Vector3 prevpos;
    public float radius; 
    // Start is called before the first frame update
    void Start()
    {
        pos = transform.localPosition;
        prevpos = pos;
        
    }

    public void updateTransformPos()
    {
        
       /* {
            transform.localPosition = Vector3.Lerp(transform.localPosition, pos, Time.deltaTime);
        }*/
        transform.localPosition = pos;
    }
    // Update is called once per frame
    void Update()
    {
        
        pos = transform.localPosition;
    }

}
