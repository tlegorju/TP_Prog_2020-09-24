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
            return Mathf.Lerp(minTheta, maxTheta, currentTheta);
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
            minRho = minRhoCurve.Evaluate(Mathf.Lerp(minPhi, maxPhi, currentPhi));
            maxRho = maxRhoCurve.Evaluate(Mathf.Lerp(minPhi, maxPhi, currentPhi));
        }
    }
    [SerializeField]
    private float targetRho, targetTheta, targetPhi;
    
    [SerializeField]
    private float minRho=1, maxRho=12, minTheta, maxTheta, minPhi, maxPhi;
    [SerializeField]
    private float numberOfRhoLevel = 12;
    private float currentRhoLevel = 1;
    [SerializeField]
    private float numberOfThetaLevel = 12;
    private float currentThetaLevel = 1;

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
        //if (Input.GetKeyDown(KeyCode.Space))
        //    ReinitializePosition();
        //else if (Input.GetKeyDown(KeyCode.LeftControl))
        //    MoveToPreviousThetaLevel();
        //else if (Input.GetKeyDown(KeyCode.LeftAlt))
        //    MoveToNextThetaLevel();
        //else if(Input.mouseScrollDelta.y<-.1f)
        //    MoveToNextRhoLevel();
        //else if (Input.mouseScrollDelta.y > .1f)
        //    MoveToPreviousRhoLevel();
        //else if(Input.GetMouseButton(1))
        if (Input.GetMouseButton(1))
        {
            if(Mathf.Abs(Input.GetAxis("Mouse Y"))>.1f)
            {
                targetPhi = Mathf.Clamp01(targetPhi - Input.GetAxis("Mouse Y") * speedMouseY * Time.deltaTime);
            }
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > .1f)
            {
                targetTheta = targetTheta - Input.GetAxis("Mouse X") * speedMouseX * Time.deltaTime;
                if (targetTheta < 0)
                    targetTheta += 1;
                else if (targetTheta >= 1)
                    targetTheta -= 1;
            }
            //StartLerpCoroutine();
        }

        CurrentRho = Mathf.Lerp(currentRho, targetRho, Time.deltaTime) ;
        CurrentTheta = Mathf.Lerp(currentTheta, targetTheta, Time.deltaTime);
        CurrentPhi = Mathf.Lerp(currentPhi, targetPhi, Time.deltaTime);
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



        //StartLerpCoroutine();
    }

    private void MoveToNextRhoLevel()
    {
        currentRhoLevel = Mathf.Min((currentRhoLevel + 1),numberOfRhoLevel);
        targetRho = Mathf.Lerp(minRho, maxRho, currentRhoLevel / numberOfRhoLevel);

        //StartLerpCoroutine();
    }

    private void MoveToPreviousRhoLevel()
    {
        currentRhoLevel = Mathf.Max(currentRhoLevel-1,0);
        targetRho = Mathf.Lerp(minRho, maxRho, currentRhoLevel / numberOfRhoLevel);

        //StartLerpCoroutine();
    }

    private void MoveToNextThetaLevel()
    {
        currentThetaLevel = (currentThetaLevel + 1) % (numberOfThetaLevel + 1);
        targetTheta = Mathf.Lerp(minTheta, maxTheta, currentThetaLevel / numberOfThetaLevel);

        //StartLerpCoroutine();
    }

    private void MoveToPreviousThetaLevel()
    {
        currentThetaLevel = ((currentThetaLevel - 1) < 0) ? numberOfThetaLevel : currentThetaLevel - 1;
        targetTheta = Mathf.Lerp(minTheta, maxTheta, currentThetaLevel / numberOfThetaLevel);

        //StartLerpCoroutine();
    }

    //private void StartLerpCoroutine()
    //{
    //    if (lerpingCoroutine != null)
    //        StopCoroutine(lerpingCoroutine);
    //    lerpingCoroutine = StartCoroutine(LerpPosition());
    //}

    //private IEnumerator LerpPosition()
    //{
    //    float beforeLerpRho = currentRho,
    //        beforeLerpTheta = currentTheta,
    //        beforeLerpPhi = currentPhi;

    //    float startTime = Time.time;
           
    //    while(Time.time < startTime+lerpingTime)
    //    {
    //        float interval = (Time.time - startTime) / lerpingTime;

    //        currentRho = Mathf.Lerp(beforeLerpRho, targetRho, interval);
    //        currentTheta = Mathf.Lerp(beforeLerpTheta, targetTheta, interval);
    //        currentPhi = Mathf.Lerp(beforeLerpPhi, targetPhi, interval);

    //        yield return null;
    //    }

    //    lerpingCoroutine = null;
    //}

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
