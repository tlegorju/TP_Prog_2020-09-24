using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSatelliteControler : MonoBehaviour
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform satellitCamera;

    [SerializeField] private float startRho, startTheta, startPhi;

    [SerializeField]
    private float currentRho = 0, currentTheta = 0, currentPhi = 0;
    private float CurrentRho
    {
        get
        {
            return Mathf.Lerp(minRho, maxRho, currentRho);
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
            float tmp = currentTheta;
            if (currentTheta > 1)
                tmp = currentTheta - (int)currentTheta;
            else if(currentTheta < 0)
                tmp = 1+( currentTheta - (int)currentTheta);
            Debug.Log("TMP : " + tmp);
            return Mathf.Lerp(minTheta, maxTheta, tmp);
        }
        set
        {
            currentTheta = value;
        }
    }
    private float CurrentPhi { 
        get { 
            return Mathf.Lerp(minPhi, maxPhi, currentPhi); 
        }
        set
        {
            currentPhi = value;
            minRho = minRhoCurve.Evaluate(currentPhi);
            maxRho = maxRhoCurve.Evaluate(currentPhi);
        }
    }
    [SerializeField]
    private float targetRho, targetTheta, targetPhi;
    
    [SerializeField]
    private float minRho=1, maxRho=12, minTheta, maxTheta, minPhi, maxPhi;
    [SerializeField]
    private int numberOfRhoLevel = 12;
    private int currentRhoLevel = 1;
    [SerializeField]
    private int numberOfThetaLevel = 12;
    private int currentThetaLevel = 1;

    [SerializeField]
    private float speedMouseX = 150, speedMouseY = 150;

    private Coroutine lerpingCoroutine=null;
    [SerializeField] private float lerpingTime=1;

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
                targetPhi = Mathf.Clamp01(targetPhi - Input.GetAxis("Mouse Y") * Time.deltaTime);
            }
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > .1f)
            {
                targetTheta = targetTheta - Input.GetAxis("Mouse X") * Time.deltaTime;
            }
        }

        CurrentRho = Mathf.MoveTowards(currentRho, targetRho, Time.deltaTime) ;
        CurrentTheta = Mathf.MoveTowards(currentTheta, targetTheta, Time.deltaTime);
        CurrentPhi = Mathf.MoveTowards(currentPhi, targetPhi, Time.deltaTime);
    }

    private void LateUpdate()
    {
        if(cameraTarget ==null)
        {
            Debug.LogError("Camera target null");
            return;
        }

        satellitCamera.position = Vector3.Lerp(satellitCamera.position,
            SphericalToCartesianPos(CurrentPhi, CurrentTheta, CurrentRho), Time.deltaTime);

        satellitCamera.LookAt(cameraTarget);
    }

    private void ReinitializePosition()
    {
        targetRho = startRho;
        targetTheta = startTheta;
        targetPhi = startPhi;
    }

    private void MoveToNextRhoLevel()
    {
        targetRho = Mathf.Clamp01(targetRho + 1.0f / numberOfRhoLevel);
    }

    private void MoveToPreviousRhoLevel()
    {
        targetRho = Mathf.Clamp01(targetRho - 1.0f / numberOfRhoLevel);
    }

    private void MoveToNextThetaLevel()
    {
        int closestSuperiorLevel = (int)(targetTheta * numberOfThetaLevel +1);
        //if(targetTheta<0)
        //{
        //    closestSuperiorLevel = (int)(targetTheta * numberOfThetaLevel);
        //}

        if(targetTheta>=0)
        {
            if ((((float)closestSuperiorLevel) / numberOfThetaLevel) - targetTheta >= 0.03f)
            {
                targetTheta = ((float)closestSuperiorLevel) / numberOfThetaLevel;
            }
            else
            {
                targetTheta = (float)(closestSuperiorLevel + 1) / numberOfThetaLevel;
            }
        }
        else
        {
            if (Mathf.Abs((((float)closestSuperiorLevel) / numberOfThetaLevel) - targetTheta) >= 0.03f)
            {
                targetTheta = ((float)closestSuperiorLevel) / numberOfThetaLevel;
            }
            else
            {
                targetTheta = (float)(closestSuperiorLevel - 1) / numberOfThetaLevel;
            }
        }
       
        
    }

    private void MoveToPreviousThetaLevel()
    {
        int closestInferiorLevel = (int)(targetTheta * numberOfThetaLevel);
        if (targetTheta < 0)
        {
            closestInferiorLevel = (int)((targetTheta-1) * numberOfThetaLevel);
        }

        if(targetTheta>=0)
        {

            if (targetTheta - ((float)closestInferiorLevel / numberOfThetaLevel) >= 0.03f)
                targetTheta = (float)closestInferiorLevel / numberOfThetaLevel;
            else if (targetTheta >= 0)
                targetTheta = (float)(closestInferiorLevel - 1) / numberOfThetaLevel;
        }
        else
        {
            if (Mathf.Abs(targetTheta - ((float)closestInferiorLevel / numberOfThetaLevel)) >= 0.03f)
                targetTheta = (float)(closestInferiorLevel+1) / numberOfThetaLevel;
            else
            {
                targetTheta = (float)(closestInferiorLevel) / numberOfThetaLevel;
            }
        }
    }

    private Vector3 SphericalToCartesianPos(float phi, float theta, float rho)
    {
        Vector3 cartesianPos;
        float radPhi = phi * Mathf.Deg2Rad;
        float radTheta = theta * Mathf.Deg2Rad;

        float a = rho * Mathf.Cos(radPhi); //??

        cartesianPos.x = a * Mathf.Cos(radTheta);
        cartesianPos.y = rho * Mathf.Sin(radPhi);
        cartesianPos.z = a * Mathf.Sin(radTheta);

        return cartesianPos;
    }
}
