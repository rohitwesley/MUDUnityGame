using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MovementTools
{
    public class FollowPath : MonoBehaviour
    {
        /// <summary>
        /// FollowPath Model
        /// </summary>
        [Tooltip("Show Debug Gizmos")]
        [SerializeField] public bool isDebug = true;
        [Tooltip("Spacing between each interpolated point on Spline")]
        [SerializeField] private float spacing = .1f;
        [Tooltip("Resoluton of the interpolation")]
        [SerializeField] private float resolution = 1;
        [Tooltip("Resoluton of the interpolation")]
        [SerializeField] private SplineTool bazierPath;
        [Tooltip("Object to follow Path")]
        [SerializeField] private Transform bazierObject;
        [Tooltip("Spped of Path")]
        [SerializeField] private float speed = 0.1f;
        Vector3[] points;
        int index = 0;
        float lasteTickTime = 0;


        // Start is called before the first frame update
        void Start()
        {
            points = bazierPath.path.CalculateEvenlySpacedPoints(spacing, resolution);
            
        }

        private void Update()
        {

            if (Time.time > lasteTickTime)
            {
                lasteTickTime += Time.deltaTime * speed;
                if (index >= points.Length - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                    bazierObject.position = Vector3.Lerp(points[index - 1], points[index], lasteTickTime);
                }
            }
        }

        /// <summary>
        /// FollowPath Debuger 
        /// </summary>
        private void OnDrawGizmos()
        {
            if (isDebug)
            {
                points = bazierPath.path.CalculateEvenlySpacedPoints(spacing, resolution);
                foreach (Vector3 p in points)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(p, spacing * .1f);
                }
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(points[index], .2f);
            }

        }

    }

}
