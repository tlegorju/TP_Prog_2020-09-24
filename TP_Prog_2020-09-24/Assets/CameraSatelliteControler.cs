using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSatelliteControler : MonoBehaviour
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform satellitCamera;

    [SerializeField] private float startRho, startTheta, startPhi;

    [SerializeField]
    private float currentRho=0, currentTheta=0, currentPhi=0;
    private float targetRho, targetTheta, targetPhi;//Local to coroutine?
    
    [SerializeField]
    private float minRho=1, maxRho=12, minTheta, maxTheta, minPhi, maxPhi;
    [SerializeField]
    private float numberOfRhoLevel = 12;
    private float currentRhoLevel = 1;
    [SerializeField]
    private float numberOfThetaLevel = 12;
    private float currentThetaLevel = 1;

    private Coroutine lerpingCoroutine=null;
    [SerializeField] private float lerpingTime=1;


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
    }

    private void LateUpdate()
    {
        if(cameraTarget ==null)
        {
            Debug.LogError("Camera target null");
            return;
        }

        satellitCamera.position = SphericalToCartesianPos(currentPhi, currentTheta, currentRho);

        satellitCamera.LookAt(cameraTarget);
    }

    private void ReinitializePosition()
    {
        targetRho = startRho;
        targetTheta = startTheta;
        targetPhi = startPhi;

        StartLerpCoroutine();
    }

    private void MoveToNextRhoLevel()
    {
        currentRhoLevel = Mathf.Min((currentRhoLevel + 1),numberOfRhoLevel);
        targetRho = Mathf.Lerp(minRho, maxRho, currentRhoLevel / numberOfRhoLevel);

        StartLerpCoroutine();
    }

    private void MoveToPreviousRhoLevel()
    {
        currentRhoLevel = Mathf.Max(currentRhoLevel-1,0);
        targetRho = Mathf.Lerp(minRho, maxRho, currentRhoLevel / numberOfRhoLevel);

        StartLerpCoroutine();
    }

    private void MoveToNextThetaLevel()
    {
        currentThetaLevel = (currentThetaLevel + 1) % (numberOfThetaLevel + 1);
        targetTheta = Mathf.Lerp(minTheta, maxTheta, currentThetaLevel / numberOfThetaLevel);

        StartLerpCoroutine();
    }

    private void MoveToPreviousThetaLevel()
    {
        currentThetaLevel = ((currentThetaLevel - 1) < 0) ? numberOfThetaLevel : currentThetaLevel - 1;
        targetTheta = Mathf.Lerp(minTheta, maxTheta, currentThetaLevel / numberOfThetaLevel);

        StartLerpCoroutine();
    }

    private void StartLerpCoroutine()
    {
        if (lerpingCoroutine != null)
            StopCoroutine(lerpingCoroutine);
        lerpingCoroutine = StartCoroutine(LerpPosition());
    }

    private IEnumerator LerpPosition()
    {
        float beforeLerpRho = currentRho,
            beforeLerpTheta = currentTheta,
            beforeLerpPhi = currentPhi;

        float startTime = Time.time;
           
        while(Time.time < startTime+lerpingTime)
        {
            float interval = (Time.time - startTime) / lerpingTime;

            currentRho = Mathf.Lerp(beforeLerpRho, targetRho, interval);
            currentTheta = Mathf.Lerp(beforeLerpTheta, targetTheta, interval);
            currentPhi = Mathf.Lerp(beforeLerpPhi, targetPhi, interval);

            yield return null;
        }

        lerpingCoroutine = null;
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
