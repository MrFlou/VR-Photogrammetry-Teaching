using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


public class paintRayCast : MonoBehaviour
{
    public Camera Camera;
    public int res;
    int ID = 0;
    int picNr = 1;
    RaycastHit hit;
    float[,] imageArray = new float[512, 512];
    float[,] takenArray;



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
        
        float camFOV = Camera.fieldOfView / 2;
        float difDeg = ((camFOV * 2) / res);
        Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;
        Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
        Renderer rend = hit.transform.GetComponent<Renderer>();
        Texture2D tex = rend.material.mainTexture as Texture2D;


        takenArray = new float[512, 512];
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
        Texture2D textureOut = new Texture2D(512, 512);
        for (int y = 0; y < takenArray.GetLength(0); y++)
            {
                for (int x = 0; x < takenArray.GetLength(1); x++)
                {
                    imageArray[x, y] = imageArray[x, y] + takenArray[x, y];                    
                    Color col = new Color(imageArray[x, y] / picNr, imageArray[x, y] / picNr, imageArray[x, y] / picNr, 1f);                    
                    textureOut.SetPixel(x, y, col);
                /* Standard Red,Blue,Green
                 if (takenArray[x,y] == 1)
                {
                    tex.SetPixel(x, y, Color.red);
                } else if (imageArray[x, y] == 2)
                {
                    tex.SetPixel(x, y, Color.blue);
                } else if (imageArray[x, y] >= 3)
                {
                    tex.SetPixel(x, y, Color.green);
                }*/
                }
            }

        SaveTextureAsPNG(textureOut, Application.dataPath + "/grayScale.png");
        tex.Apply();
        picNr = picNr + 1;

    }
    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 512 + "Kb was saved as: " + _fullPath);
    }

}
