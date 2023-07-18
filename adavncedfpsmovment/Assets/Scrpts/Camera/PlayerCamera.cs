using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{


    public float sensX;
    public float sensY;

    public Transform orientation; //player position

    //rotation values for your camera
    private float xRot;
    private float yRot;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //locking the cursor
        Cursor.visible = false;

        
    }

    // Update is called once per frame
    void Update()
    {
        //Getting mouse Inputs

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY= Input.GetAxisRaw("Mouse Y")*Time.deltaTime* sensY;

        //Handling Rotations 
        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);//clamping our xrot to -90 and 90



        //Unity uses Euleration to handle player and camera rotations 
        transform.rotation = Quaternion.Euler(xRot, yRot, 0);//rotating camera
        orientation.rotation = Quaternion.Euler(0, yRot, 0); // rotation our player arounf y axis


    }
}
