using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [SerializeField] Transform planeGfx;

    [Header("Camera")]
    [SerializeField] Transform planeCamera;
    [SerializeField] Transform cameraTargetPosition;
    [SerializeField] Transform cameraTargetLookAt;
    [SerializeField] float cameraTranslateSpeed=5;
    [SerializeField] float cameraLookAtSpeed = 5;

    [Header("Movement")]
    [SerializeField] float minVelocity = 1;
    [SerializeField] float maxVelocity = 7;    //The bound are shown before the velocity in order for the header to show up in the inspector
    private float velocity;
    public float Velocity { get { return velocity; } set { velocity = Mathf.Clamp(value, minVelocity, maxVelocity); } }
    [SerializeField] float acceleration=3;
    [Space(5)]

    [Header("Rotation")]
    [SerializeField] float minPitch = -90;
    [SerializeField] float maxPitch = 90;
    [SerializeField] float minBank = -10;
    [SerializeField] float maxBank = 10;
    [SerializeField] float headingRotSpeed = 5;
    [SerializeField] float pitchRotSpeed = 5;
    [SerializeField] float bankRotSpeed = 5;
    private float heading, pitch, bank;
    private float Pitch { get { return pitch; } set { pitch = Mathf.Clamp(value, minPitch, maxPitch); } }
    private float Bank { get { return bank; } set { bank = Mathf.Clamp(value, minBank, maxBank); } }


    // Start is called before the first frame update
    void Start()
    {
        if (planeCamera == null)
            planeCamera = Camera.main.transform;    //if we didn't give a camera, we find one
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .05f)  //If there is a horizontal input, we compute the heading
            UpdateHeading(Input.GetAxis("Horizontal"));
        if (Mathf.Abs(Input.GetAxis("Vertical")) > .05f)    //If there is a vertical input, we compute the pitch
            UpdatePitch(Input.GetAxis("Vertical"));

        UpdateBank(Input.GetAxis("Horizontal"));            //Compute the bank according to the value of the horizontal input

        UpdateRotation();                                   //Update the rotation of the plane and its gfx

        if (Input.GetKey(KeyCode.Space))                    //Compute the desired velocity
            Accelerate();
        else
            Decelerate();

        UpdatePos();                                        //Update the position of the plane
    }

    private void LateUpdate()                               //Update de camera following the plane
    {
        UpdateCameraPos();
        UpdateCameraLookAt();
    }

    private void UpdatePos()
    {
        transform.position += velocity * transform.forward * Time.deltaTime;
    }

    private void Accelerate()
    {
        Velocity += acceleration * Time.deltaTime;
    }

    private void Decelerate()
    {
        Velocity -= acceleration * Time.deltaTime;
    }

    private void UpdateRotation()
    {
        transform.rotation = Quaternion.Euler(Pitch, heading, 0);
        planeGfx.localRotation = Quaternion.AngleAxis(Bank, Vector3.forward);
    }

    private void UpdateHeading(float axisValue)
    {
        heading += headingRotSpeed * Time.deltaTime * axisValue;

        if (heading < 0)
            heading += 360;
        else if (heading >= 360)
            heading -= 360;
    }

    private void UpdatePitch(float axisValue)
    {
        Pitch += pitchRotSpeed * Time.deltaTime * axisValue;
    }

    private void UpdateBank(float axisValue)
    {
        Bank = Mathf.Lerp(Bank, -axisValue*maxBank, Time.deltaTime*bankRotSpeed);
    }

    private void UpdateCameraPos()
    {
        planeCamera.position = Vector3.Lerp(planeCamera.position, cameraTargetPosition.position, Time.deltaTime * cameraTranslateSpeed);
    }

    private void UpdateCameraLookAt()
    {
        planeCamera.rotation = Quaternion.Slerp(planeCamera.rotation, Quaternion.LookRotation((cameraTargetLookAt.position - planeCamera.position).normalized), cameraLookAtSpeed*Time.deltaTime);
    }
}
