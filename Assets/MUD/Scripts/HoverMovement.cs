using System.Collections;
using UnityEngine;

public class HoverMovement : MonoBehaviour
{
    /// <summary>
    /// Hover Model
    /// </summary>
    [Tooltip("Rotate in X Axis")]
    [SerializeField] bool xRotation;
    [Tooltip("Rotate in Y Axis")]
    [SerializeField] bool yRotation;
    [Tooltip("Rotate in Z Axis")]
    [SerializeField] bool zRotation;
    [Tooltip("Show Debug Gizmos")]
    [SerializeField] bool isDebug = true;

    /// <summary>
    /// Hover Object on specific axes
    /// </summary>
    void Update()
    {
        if(xRotation){
            transform.Rotate(new Vector3 (15, 0, 0) * Time.deltaTime, Space.Self);
        }
        if(yRotation){
            transform.Rotate(new Vector3 (0, 30, 0) * Time.deltaTime, Space.Self);
        }
        if(zRotation){
            transform.Rotate(new Vector3 (0, 0, 45) * Time.deltaTime, Space.Self);
        }
    }


    /// <summary>
    /// Hover Debuger 
    /// </summary>
    private void OnDrawGizmos()
    {
        if(isDebug)
        {
            Gizmos.color = Color.red;


        }

    }

}
