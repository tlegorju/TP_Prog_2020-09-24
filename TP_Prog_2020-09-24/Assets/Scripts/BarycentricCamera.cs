using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BarycentricCamera : MonoBehaviour
{

    [SerializeField]
    float minDistance;
    [SerializeField]
    float minOrthographicSize;
    [SerializeField]
    float maxOrthographicSize;
    [SerializeField]
    float zoomSpeed = 1f;

    float orthographicSize = 0;
    float orthographicSizeZ = 0;
    float orthographicSizeX = 0;
    float minPointX= Mathf.Infinity;
    float maxPointX = Mathf.NegativeInfinity;
    float minPointZ = Mathf.Infinity;
    float maxPointZ = Mathf.NegativeInfinity;

    Dictionary<string, float> distances;
    float[] wheight;
    List<BoxCollider> blist;

    // Start is called before the first frame update
    void Start()
    {
        blist = new List<BoxCollider>(FindObjectsOfType<BoxCollider>());
        
    }

    // Update is called once per frame
    void Update()
    {
            FindBarycenter();
    }

    private void OnGUI()
    {
        for (int i = 0; i < blist.Count; i++)
        {
            var position = Camera.main.WorldToScreenPoint(blist[i].transform.position);
            var textSize = GUI.skin.label.CalcSize(new GUIContent(""+wheight[i]));
            GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), "" + wheight[i]);
            
        }
    }

    /***this function is used to find the barycenter point of our cubes present in the scene
    *we first calculate the distances between all our points and we prevent the doubles values.
    *We stocks them in a dictionnary in order to reuse them more efficiently
    *then we evaluate their weight and we calculate the barycenter
    * before setting the camera position, we calculate the good size for our camera to contain all our objects in the scene
     */
    private void FindBarycenter()
    {
        distances = new Dictionary<string, float>();

        float coef = 0;
        wheight = new float[blist.Count];

        for (int i = 0; i < blist.Count - 1; i++)
        {
            for (int j = i + 1; j < blist.Count; j++)
            {
                if (i == j)
                    continue;
                if (Vector3.Distance(blist[i].transform.position, blist[j].transform.position) < minDistance)
                {
                    distances.Add(i + "-" + j, minDistance);
                }
                else
                {
                    distances.Add(i + "-" + j, Vector3.Distance(blist[i].transform.position, blist[j].transform.position));
                }

            }
        }

        for (int i = 0; i < blist.Count; i++)
        {
            foreach (string s in distances.Keys)
            {
                if (s.Contains(i + ""))
                {

                    wheight[i] += 1 / Mathf.Pow(distances[s], 1);

                }
            }
            coef += wheight[i];
        }

        Vector3 gravityPoint = new Vector3();
        for (int i = 0; i < blist.Count; i++)
        {
            gravityPoint += (wheight[i] / coef) * blist[i].transform.position;
        }
        CalculateOrthographicSize(gravityPoint);
        Camera.main.transform.position = new Vector3(gravityPoint.x, Camera.main.transform.position.y, gravityPoint.z);
    }

    /***
     * To find the orthographic size we first find the rect that contains all the object by finding the minimum and maximum x and z positions
     * then we calculate our vertical orthographic size and we clamp it to our boundaries set by the designer
     * and finally we set our new size.
     */
    void CalculateOrthographicSize(Vector3 gravityPoint)
    {
        minPointX = Mathf.Infinity;
        minPointZ = Mathf.Infinity;
        maxPointX = Mathf.NegativeInfinity;
        maxPointZ = Mathf.NegativeInfinity;
        for (int i = 0; i < blist.Count; i++)
        {
            minPointX = Mathf.Min(minPointX, blist[i].gameObject.transform.position.x);
            minPointZ = Mathf.Min(minPointZ, blist[i].gameObject.transform.position.z);
            maxPointX = Mathf.Max(maxPointX, blist[i].gameObject.transform.position.x);
            maxPointZ = Mathf.Max(maxPointZ, blist[i].gameObject.transform.position.z);
        }
        orthographicSizeZ = 2 * Mathf.Max(Mathf.Abs(gravityPoint.z - minPointZ), Mathf.Abs(gravityPoint.z - maxPointZ));
        orthographicSizeX = 2 * Mathf.Max(Mathf.Abs(gravityPoint.x - minPointX), Mathf.Abs(gravityPoint.x - maxPointX));

        orthographicSizeZ = (orthographicSizeX > orthographicSizeZ * Camera.main.aspect) ? orthographicSizeX / Camera.main.aspect : orthographicSizeZ;

        orthographicSize = Mathf.Clamp(orthographicSizeZ, minOrthographicSize, maxOrthographicSize);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, orthographicSize, Time.deltaTime * zoomSpeed);
    }
}
