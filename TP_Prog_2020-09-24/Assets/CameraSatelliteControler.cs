using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSatelliteControler : MonoBehaviour
{
    [SerializeField] Transform cameraTarget;    //The object we want to look at
    [SerializeField] Transform satellitCamera;  //The camera we want to move around

    [SerializeField] private float startRho, startTheta, startPhi;  //The position we want to start at

    private float currentRho = 0, currentTheta = 0, currentPhi = 0; //The current position of the camera
    private float CurrentRho
    {
        get
        {
            return currentRho;
        }
        set
        {
            currentRho = value;
        }
    }
    private float CurrentTheta
    {
        get
        {
            return currentTheta;
        }
        set
        {
            currentTheta = value;
        }
    }
    private float CurrentPhi { 
        get {
            return currentPhi;
        }
        set
        {
            currentPhi = value;                                //We change the bound of rho when we change phi
            minRho = minRhoCurve.Evaluate(currentPhi);
            maxRho = maxRhoCurve.Evaluate(currentPhi);
        }
    }

    private float targetRho, targetTheta, targetPhi;            //Used for interpolating the position
    private float TargetTheta { get { return targetTheta; } 
        set {
            targetTheta = value;    //If we did a full degree turn, we make sure the values remain between 0 & 1 while keeping the same rotation 
            if(targetTheta<0)
            {
                targetTheta += 1;
                currentTheta += 1;
            }
            else if(targetTheta>=1)
            {
                targetTheta -= 1;
                currentTheta -= 1;
            }
        } }
    
    [SerializeField]
    private float minRho=1, maxRho=12, minTheta, maxTheta, minPhi, maxPhi;

    [SerializeField]
    private int numberOfRhoLevel = 12;
    [SerializeField]
    private int numberOfThetaLevel = 12;

    [SerializeField]
    private float speedMouseX = 15, speedMouseY = 15;
    
    [SerializeField] private AnimationCurve minRhoCurve;
    [SerializeField] private AnimationCurve maxRhoCurve;


    // Start is called before the first frame update
    void Start()
    {
        ReinitializePosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ReinitializePosition();

        else if (Input.GetKeyDown(KeyCode.LeftControl))
            MoveToPreviousThetaLevel();
        else if (Input.GetKeyDown(KeyCode.LeftAlt))
            MoveToNextThetaLevel();

        else if(Input.mouseScrollDelta.y<-.1f)
            MoveToNextRhoLevel();
        else if (Input.mouseScrollDelta.y > .1f)
            MoveToPreviousRhoLevel();

        if (Input.GetMouseButton(1))
        {
            if (Mathf.Abs(Input.GetAxis("Mouse Y")) > .1f)
            {
                targetPhi = Mathf.Clamp01(targetPhi - Input.GetAxis("Mouse Y") * Time.deltaTime * speedMouseY);
            }
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > .1f)
            {
                TargetTheta = TargetTheta - Input.GetAxis("Mouse X") * Time.deltaTime * speedMouseX;
            }
        }

        CurrentRho = Mathf.MoveTowards(CurrentRho, targetRho, Time.deltaTime) ;
        CurrentTheta = Mathf.MoveTowards(CurrentTheta, targetTheta, Time.deltaTime);
        CurrentPhi = Mathf.MoveTowards(CurrentPhi, targetPhi, Time.deltaTime);
    }

    private void LateUpdate()
    {
        if(cameraTarget ==null || satellitCamera==null)
        {
            Debug.LogError("Camera target or satellit camera null");
            return;
        }

        satellitCamera.position = Vector3.Lerp(satellitCamera.position,
            SphericalToCartesianPos(GetInterpolatePhi(), GetInterpolateTheta(), GetInterpolateRho()), Time.deltaTime);

        satellitCamera.LookAt(cameraTarget);
    }

    //Make the camera go back to the start position
    private void ReinitializePosition()
    {
        targetRho = startRho;
        TargetTheta = startTheta;
        targetPhi = startPhi;
    }

    //Unzoom the camera
    private void MoveToNextRhoLevel()
    {
        targetRho = Mathf.Clamp01(targetRho + 1.0f / numberOfRhoLevel); //Rho is only modified by the mouse wheel, so the targetRho is always on a valid step
    }

    //Zoom the camera
    private void MoveToPreviousRhoLevel()
    {
        targetRho = Mathf.Clamp01(targetRho - 1.0f / numberOfRhoLevel);
    }

    //Turn the camera counter clockwise
    private void MoveToNextThetaLevel()
    {
        int closestSuperiorLevel = (int)(TargetTheta * numberOfThetaLevel)+1;   //The closest greater step the target is from
        float angleClosestLevel = (float)closestSuperiorLevel / numberOfThetaLevel; //The angle corresponding to the step


        if (Mathf.Abs(angleClosestLevel - targetTheta) < .3f / numberOfThetaLevel) //If the difference between the step angle and our current angle is greater than 1/3 of a step
        {
            TargetTheta = angleClosestLevel + 1f / numberOfThetaLevel;   // we consider targetTheta to be on the step and go to the next
        }
        else
        {
            TargetTheta = angleClosestLevel;    //Else we go to the closest step
        }
    }

    //Turn the camera clockwise
    private void MoveToPreviousThetaLevel()
    {
        int closestInferiorLevel = (int)(TargetTheta * numberOfThetaLevel);     //The closest smaller step the target is from
        float angleClosestLevel = (float)closestInferiorLevel / numberOfThetaLevel; //The angle corresponding to the step

        if (Mathf.Abs(angleClosestLevel - TargetTheta) <.3f/numberOfThetaLevel) //If the difference between the step angle and our current angle is greater than 1/3 of a step
        {
            TargetTheta = angleClosestLevel - 1f / numberOfThetaLevel;   // we consider targetTheta to be on the step and go to the previous one
        }
        else
        {
            TargetTheta = angleClosestLevel;    //Else we go to the closest step
        }
    }

    private Vector3 SphericalToCartesianPos(float phi, float theta, float rho)
    {
        Vector3 cartesianPos;
        float radPhi = phi * Mathf.Deg2Rad;         //The radians value of the angles
        float radTheta = theta * Mathf.Deg2Rad;

        float a = rho * Mathf.Cos(radPhi); 

        cartesianPos.x = a * Mathf.Cos(radTheta);
        cartesianPos.y = rho * Mathf.Sin(radPhi);
        cartesianPos.z = a * Mathf.Sin(radTheta);

        return cartesianPos;
    }

    private float GetInterpolateTheta()
    {
        return Mathf.Lerp(minTheta, maxTheta, currentTheta);
    }

    private float GetInterpolateRho()
    {
        return Mathf.Lerp(minRho, maxRho, currentRho);
    }

    private float GetInterpolatePhi()
    {
        return Mathf.Lerp(minPhi, maxPhi, currentPhi);
    }
}
