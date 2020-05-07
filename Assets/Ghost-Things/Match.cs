using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match : MonoBehaviour
{
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
   
    // Start is called before the first frame update
    void Start()
    {

        //a clone is spawned in front of the empty object at start
        Vector3 emptyPos = empty.transform.position;
        Vector3 emptyDirection = empty.transform.forward;
        Quaternion emptyRotation = empty.transform.rotation;
        float spawnDistance = 10;

        Vector3 spawnPos = emptyPos + emptyDirection * spawnDistance;

        clone = Instantiate(ghost, spawnPos, emptyRotation);

        ghosts.Add(clone.transform);

    }
    

    // Update is called once per frame
    void Update()
    {
        Vector3 emptyPos = empty.transform.position;
        Vector3 emptyDirection = empty.transform.forward;
        Quaternion emptyRotation = empty.transform.rotation;
        float spawnDistance = 10;

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

        if (distance < range && dot < ghostDot + 0.02 && dot > ghostDot - 0.02)
        {
          //if distance between the two is less than range, and the rotation of the camModel is within 0.02 degrees of the ghost, change color to green
            clone.GetComponent<Renderer>().material.color = green;

            //if t is pressed, a picture is taken and the empty object rotates
            if (Input.GetKeyDown("t"))
            {
                Debug.Log("Pictures taken" + picsTaken);
                
                empty.transform.Rotate(0, xRotation, 0, Space.World);

                tPressed = true;

                picsTaken += 1;
              
                //when t is pressed the pressent clone on the list gets removed as a new one takes its place
                while (ghosts.Count > maxObjects)
                {
                    if (ghosts[0] != null)
                        ghosts[0].gameObject.SetActive(false);
                    ghosts.RemoveAt(0);

                }
                
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
            empty.transform.Rotate(- yRotation * 2, 0, 0, Space.World);
            fullCircle2 = true;
        }

    }

   

}
