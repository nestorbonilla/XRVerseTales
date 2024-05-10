using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAnchorLabels : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    private void Update()
    {
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        Vector3 rayDirection = controllerRotation * Vector3.forward;

        if (Physics.Raycast(controllerPosition, rayDirection, out RaycastHit hit))
        {
            lineRenderer.SetPosition(0, controllerPosition);
        }

    }
}
