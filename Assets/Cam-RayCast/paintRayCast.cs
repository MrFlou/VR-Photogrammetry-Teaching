using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;


public class paintRayCast : MonoBehaviour
{
    public Camera Camera;
    public GameObject model;
    public int res;
    int picNr = 1;
    RaycastHit hit;
    float[,] imageArray = new float[512, 512];
    float[,] takenArray;
    bool heatmap = false;
    float camFOV = Camera.fieldOfView / 2;
    float difDeg = ((camFOV * 2) / res);

    void Update()
    {
        // Buttons setup from the VR Controller, When pressing the UI/Trigger button down TakeImage function will be called
        if (SteamVR_Actions._default.InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            TakeImage();
        }

        // When pressing the A button it will toggle the Heatmap on the model
        if (SteamVR_Actions._default.A_Button.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (heatmap == false) {
                model.GetComponent<Renderer>().sharedMaterial.SetFloat("_BlendFac", 1);
                heatmap = true;
            } else if (heatmap == true) {
                model.GetComponent<Renderer>().sharedMaterial.SetFloat("_BlendFac", 0);
                heatmap = false;
            }            
        } 

        // Ability to exit the program
        if (SteamVR_Actions._default.B_Button.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            Application.Quit();
            Debug.Log("B Has been pressed");
        }

    }

    void TakeImage()
    {
        Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;
        Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
        Renderer rend = hit.transform.GetComponent<Renderer>();
        Texture2D tex = rend.material.mainTexture as Texture2D;

        float max = 0;
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

        takenArray[0, 0] = 0;

        for (int y = 0; y < takenArray.GetLength(0); y++)
        {
            for (int x = 0; x < takenArray.GetLength(1); x++)
            {
                imageArray[x, y] = imageArray[x, y] + takenArray[x, y];
                if (imageArray[x,y] > max) { max = imageArray[x, y]; }
            }
        }


        Texture2D textureOut = new Texture2D(512, 512);
        for (int y = 0; y < imageArray.GetLength(0); y++)
            {
                for (int x = 0; x < imageArray.GetLength(1); x++)
                {
                    Color col = new Color(imageArray[x, y] / max, imageArray[x, y] / picNr, imageArray[x, y] / max, 1f);                    
                    textureOut.SetPixel(x, y, col);
                }
            }

        SaveTextureAsPNG(textureOut, Application.dataPath + "/grayScale.png");        
        Debug.Log("max: " + max + " - PicNr: " + picNr);
        picNr = picNr + 1;
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 512 + "Kb was saved as: " + _fullPath);
    }

}
