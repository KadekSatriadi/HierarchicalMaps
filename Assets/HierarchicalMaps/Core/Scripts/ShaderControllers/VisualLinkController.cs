using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualLinkController : MonoBehaviour
{
  

    public Transform top;
    public Transform bottom;
    public float topRadius;
    public float bottomRadius;
    public float alpha = 0.35f;
    public Color color;

    Material material;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        material.renderQueue = 3500;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public float rotationXOffset;
    public float rotationZOffset;
    public float rotationYOffset;

    // Update is called once per frame
    void Update()
    {
        if(top != null && bottom != null)
        {   
            //Vector3 topPoint = transform.InverseTransformPoint(top.transform.position - new Vector3(0, 0.025f, 0));
            Vector3 topPoint = transform.InverseTransformPoint(top.transform.position + new Vector3(0,0,0.01f));
            Vector3 toRotationPivot = transform.InverseTransformPoint(top.transform.position);

            material.SetVector("_topPosition", topPoint);
            material.SetVector("_bottomPosition", transform.InverseTransformPoint(bottom.transform.position));
            material.SetFloat("_topRadius", topRadius * 10.25f);
            material.SetFloat("_bottomRadius", bottomRadius * 10.25f);
            material.SetFloat("_alpha", alpha);
            GetComponent<MeshRenderer>().enabled = true;


            //Rotation
          //  rotationXOffset = (Mathf.Abs(bottom.localEulerAngles.x) > 180)? -(360 - bottom.localEulerAngles.x) : bottom.localEulerAngles.x;
          //  rotationZOffset = (Mathf.Abs(bottom.localEulerAngles.z) > 180)? -(360 - bottom.localEulerAngles.z) : bottom.localEulerAngles.z;
            //rotationYOffset = (Mathf.Abs(bottom.localEulerAngles.y) > 180)? -(360 - bottom.localEulerAngles.y) : bottom.localEulerAngles.y;
          //  material.SetVector("_topRotationPivot", topPoint);
          //  material.SetFloat("_topRotationZ", - (top.rotation.eulerAngles.z + rotationZOffset));
          //  material.SetFloat("_topRotationX", -(top.rotation.eulerAngles.x + rotationXOffset));
          //  material.SetFloat("_topRotationY", 180 + (top.rotation.eulerAngles.y + rotationYOffset));
          //  material.SetColor("_color", color);
            //Debug.DrawRay(top.transform.position, top.up, Color.yellow);
            //Debug.DrawRay(top.transform.position, top.right, Color.red);
            //Debug.DrawRay(top.transform.position, top.forward, Color.blue);

            //Debug.DrawRay(topPoint, transform.InverseTransformPoint(top.forward) - topPoint, Color.white);

        }
    }
}
