using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;


public class paintRayCast : MonoBehaviour
{
    public Camera Camera;
    public int res;
    int ID = 0;
    int picNr = 0;
    RaycastHit hit;
    int[,] imageArray = new int[512, 512];
    int[,] takenArray;


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
        takenArray = new int[512, 512];
        float camFOV = Camera.fieldOfView / 2;
        float difDeg = ((camFOV * 2) / res);
        Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;
        Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
        Renderer rend = hit.transform.GetComponent<Renderer>();
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
                nowDegX = nowDegX + difDeg;
                directionRay = Quaternion.AngleAxis(camFOV - nowDegX, new Vector3(1, 0, 0)) * Vector3.forward;
                directionRay = Quaternion.AngleAxis(camFOV - nowDegY, new Vector3(0, 1, 0)) * directionRay;
                takenArray[(int)pixelUV.x, (int)pixelUV.y] = 1;
            }
            
            nowDegY = nowDegY + difDeg;            
        }
        for (int y = 0; y < takenArray.GetLength(0); y++)
            {
                for (int x = 0; x < takenArray.GetLength(1); x++)
                {
                    imageArray[x, y] = imageArray[x, y] + takenArray[x, y];
                    if (imageArray[x,y] == 1)
                    {
                        tex.SetPixel(x, y, Color.red);
                    } else if (imageArray[x, y] == 2)
                    {
                        tex.SetPixel(x, y, Color.blue);
                    } else if (imageArray[x, y] >= 3)
                    {
                        tex.SetPixel(x, y, Color.green);
                    }
                }
            }
        tex.Apply();
        picNr = picNr + 1;

    }
}
