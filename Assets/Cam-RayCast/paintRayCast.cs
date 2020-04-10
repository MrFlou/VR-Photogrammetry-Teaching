using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class paintRayCast : MonoBehaviour
{
    public Camera Camera;
    public int res;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // RaycastHit hit;
        // float camFOV = Camera.fieldOfView/2;
        // float difDeg = (res / camFOV);
        // Debug.DrawRay(Camera.transform.position, Camera.transform.TransformDirection(Quaternion.AngleAxis(camFOV, new Vector3(1, 0, 0)) * Vector3.forward), Color.red);
        // Debug.DrawRay(Camera.transform.position, Camera.transform.TransformDirection(Quaternion.AngleAxis(-camFOV, new Vector3(1 , 0 ,0)) * Vector3.forward) , Color.green);
        // Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        // directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;
        // Debug.DrawRay(Camera.transform.position, Camera.transform.TransformDirection(directionRay), Color.blue);
        if (SteamVR_Actions._default.InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            TakeImage();
        }
    }

    void TakeImage()
    {
        RaycastHit hit;
        float camFOV = Camera.fieldOfView/2;
        float difDeg = ((camFOV * 2) / res);
        Debug.Log(difDeg);
        Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;
        Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;
        Texture2D tex = rend.material.mainTexture as Texture2D;

        float nowDegY = 0f;        
        for (int y = 0; y < res; y++)
        {
            float nowDegX = 0f;
            for (int x = 0; x < res; x++)
            {
                
                Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(directionRay), out hit, 100f);
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;
                tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.red);                
                nowDegX = nowDegX + difDeg;
                directionRay = Quaternion.AngleAxis(camFOV - nowDegX, new Vector3(1, 0, 0)) * Vector3.forward;
                directionRay = Quaternion.AngleAxis(camFOV - nowDegY, new Vector3(0, 1, 0)) * directionRay;
                

            }
            nowDegY = nowDegY + difDeg;            
        }
        tex.Apply();
    }
}
