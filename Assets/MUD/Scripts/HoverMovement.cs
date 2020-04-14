using System.Collections;
using UnityEngine;

public class HoverMovement : MonoBehaviour
{
    [Tooltip("Rotate in X Axis")]
    [SerializeField] bool _xRotation;
    [Tooltip("Rotate in Y Axis")]
    [SerializeField] bool _YRotation;
    [Tooltip("Rotate in Z Axis")]
    [SerializeField] bool _zRotation;
    
    void Update()
    {
        if(_xRotation){
            transform.Rotate(new Vector3 (15, 0, 0) * Time.deltaTime, Space.Self);
        }
        if(_YRotation){
            transform.Rotate(new Vector3 (0, 30, 0) * Time.deltaTime, Space.Self);
        }
        if(_zRotation){
            transform.Rotate(new Vector3 (0, 0, 45) * Time.deltaTime, Space.Self);
        }
        // transform.Rotate(new Vector3 (15, 30, 45) * Time.deltaTime, Space.Self);
    }
}
