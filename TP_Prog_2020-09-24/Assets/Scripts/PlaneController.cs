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
    [SerializeField] float headingRotSpeed = 5;
    [SerializeField] float pitchRotSpeed = 5;
    [SerializeField] float bankRotSpeed = 5;
    [SerializeField] AnimationCurve bankCurve;
    private float heading, pitch;
    private float bank; //Value between -1 & 1 computed by the animation curve


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
        transform.rotation = Quaternion.identity; //Réinitialize the transform rotation

        //Rotate the transform according to the heading and the pitch
        transform.rotation = Quaternion.AngleAxis(heading, Vector3.up) * Quaternion.AngleAxis(pitch, transform.right) * transform.rotation;

        //Tilt the GFX to give a bette feedback
        planeGfx.localRotation = Quaternion.AngleAxis(bankCurve.Evaluate(bank), Vector3.forward);
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
        pitch = Mathf.Clamp(pitch+pitchRotSpeed * Time.deltaTime * axisValue, minPitch, maxPitch);
    }

    private void UpdateBank(float axisValue)
    {
        bank = Mathf.Clamp(Mathf.Lerp(bank, -axisValue, Time.deltaTime * bankRotSpeed), -1, 1);
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
