using UnityEngine;
using System.Collections.Generic;
using Valve.VR;
using System.IO;

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

    //Ghotst Check
    public GameObject ghost;
    GameObject clone;
    public GameObject cameraModel;
    public Transform target;
    public Transform empty;
    float distance;
    float range = 0.15f;
    float dot;
    float ghostDot;
    public Color green;
    public Color yellow;
    public float xRotation;
    int picsTaken = 0;
    int maxObjects = 0;
    public float yRotation;
    bool tPressed = false;
    bool fullCircle = false;
    bool fullCircle2 = false;


    List<Transform> ghosts = new List<Transform>();

    //Ghost Check

    private void Start()
    {
        //a clone is spawned in front of the empty object at start
        Vector3 emptyPos = empty.transform.position;
        Vector3 emptyDirection = empty.transform.forward;
        Quaternion emptyRotation = empty.transform.rotation;
        float spawnDistance = 2;

        Vector3 spawnPos = emptyPos + emptyDirection * spawnDistance;

        clone = Instantiate(ghost, spawnPos, emptyRotation);

        ghosts.Add(clone.transform);
    }


    void Update()
    {
        //Ghost Check
        Vector3 emptyPos = empty.transform.position;
        Vector3 emptyDirection = empty.transform.forward;
        Quaternion emptyRotation = empty.transform.rotation;
        float spawnDistance = 2;

        Vector3 spawnPos = emptyPos + emptyDirection * spawnDistance;


        //distance between ghost and camModel
        distance = Vector3.Distance(clone.transform.position, cameraModel.transform.position);

        //dot = the rotation of the camModel
        //ghost = the rotation of the ghost
        dot = Vector3.Dot(cameraModel.transform.forward, target.transform.forward);
        ghostDot = Vector3.Dot(clone.transform.forward, target.transform.forward);

        //when t is pressen "tPressed" becomes true. when "tPressed" is true, a clone is spawned and added to the ghosts list
        if (tPressed == true)
        {
            clone = Instantiate(ghost, spawnPos, emptyRotation);
            ghosts.Add(clone.transform);
            tPressed = false;
        }

        if (distance < range && dot < ghostDot + 0.04 && dot > ghostDot - 0.04)
        {
            //if distance between the two is less than range, and the rotation of the camModel is within 0.02 degrees of the ghost, change color to green
            clone.GetComponent<Renderer>().material.color = green;

            //if t is pressed, a picture is taken and the empty object rotates
            if (SteamVR_Actions._default.InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
            {

                TakeImage();

            }

        }
        else
        {
            //if the camera model is not within range, the ghost returns to its original color
            clone.GetComponent<Renderer>().material.color = yellow;
        }

        //when the ghost has taken its first full rotation around the object, the empty object rotates downwards on the y axis
        if (xRotation * picsTaken >= 360 && fullCircle == false)
        {
            empty.transform.Rotate(yRotation, 0, 0, Space.World);
            fullCircle = true;
        }

        //when the ghost has taken its second full rotation around the object, the empty object rotates upwards on the y axis
        if (xRotation * picsTaken >= 720 && fullCircle2 == false)
        {
            empty.transform.Rotate(-yRotation * 2, 0, 0, Space.World);
            fullCircle2 = true;
        }


        //Ghots Check
        // Buttons setup from the VR Controller, When pressing the UI/Trigger button down TakeImage function will be called
        if (SteamVR_Actions._default.InteractUI.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            //Match.Update();
            //TakeImage();
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

        // Calculation of the FOV and the degree pr ray that is needed to cover the cameras view.
        float camFOV = Camera.fieldOfView / 2;
        float difDeg = ((camFOV * 2) / res);
        
        // Direction of the first ray and prepairs the direction for future ray casts
        Vector3 directionRay = Quaternion.AngleAxis(camFOV, new Vector3(0, 1, 0)) * Vector3.forward;
        directionRay = Quaternion.AngleAxis(-camFOV, new Vector3(1, 0, 0)) * directionRay;

        // Test ray to see of there is an object infront of the camera
        Renderer rend = model.transform.GetComponent<Renderer>();
        Texture2D tex = rend.material.mainTexture as Texture2D;

        float max = 0;
        takenArray = new float[512, 512];
        float nowDegY = 0f;
        

        //when t is pressed the pressent clone on the list gets removed as a new one takes its place
        while (ghosts.Count > maxObjects)
        {
            if (ghosts[0] != null)
                ghosts[0].gameObject.SetActive(false);
            ghosts.RemoveAt(0);

        }
        
        // This part goes over the X and Y reselution of the camera, for each "Pixel" a ray will be casted and calculated where it hits on the model
        // Each position on the models texture that is hit will be noted in an Array that has the same dimentions as the texture of the model.
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

        // This combiens the arrays from previous pictures into one array
        // This part also finds the higest number in the array
        for (int y = 0; y < takenArray.GetLength(0); y++)
        {
            for (int x = 0; x < takenArray.GetLength(1); x++)
            {
                imageArray[x, y] = imageArray[x, y] + takenArray[x, y];
                if (imageArray[x,y] > max) { max = imageArray[x, y]; }
            }
        }

        // Here a new texture is made and give apporitive color for each position on the previous array
        Texture2D textureOut = new Texture2D(512, 512);
        for (int y = 0; y < imageArray.GetLength(0); y++)
            {
                for (int x = 0; x < imageArray.GetLength(1); x++)
                {
                    Color col = new Color(imageArray[x, y] / max, imageArray[x, y] / picNr, imageArray[x, y] / max, 1f);                    
                    textureOut.SetPixel(x, y, col);
                }
            }

        // Exporting the texture and printing debug log
        SaveTextureAsPNG(textureOut, Application.dataPath + "/grayScale.png");        
        Debug.Log("max: " + max + " - PicNr: " + picNr);
        picNr = picNr + 1;
        Debug.Log("Pictures taken: " + picsTaken);

        empty.transform.Rotate(0, xRotation, 0, Space.World);

        tPressed = true;

        picsTaken += 1;
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 512 + "Kb was saved as: " + _fullPath);
    }

}
