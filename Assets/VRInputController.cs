using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputController : MonoBehaviour
{
    
    public float maxGripRadius = 0.1f;
    public SphereCollider LCollider;
    public SphereCollider RCollider;
    public List<Transform> lvertices;
    public List<Transform> rvertices;
    public ClothController clothController;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {

            LCollider.enabled = true;
            lvertices =  clothController.handleGrab(LCollider);
            Debug.Log(lvertices.Count);
        }
        else if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
        {
            LCollider.enabled = false;
            foreach (Transform t in lvertices)
            {
                t.GetComponent<Vertex>().constrained = false;
            }
            lvertices = new List<Transform>();
        }
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            RCollider.enabled = true;
            rvertices = clothController.handleGrab(RCollider);
            Debug.Log(rvertices.Count);

        }
        else if (OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger))
        {
            RCollider.enabled = false;
            foreach (Transform t in rvertices)
            {
                t.GetComponent<Vertex>().constrained = false;
            }
            rvertices = new List<Transform>();
        }
        if (lvertices!=null)
        {
            
            foreach(Transform t in lvertices)
            {
                t.position = LCollider.transform.position;
            }

        }
        if (rvertices != null)
        {

            foreach (Transform t in rvertices)
            {
                t.localPosition = t.parent.InverseTransformPoint(RCollider.transform.position);
            }

        }

    }
    
}
