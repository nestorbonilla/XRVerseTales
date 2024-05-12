using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalMaker : MonoBehaviour
{
    public GameObject prefabToPlace;
    public GameObject debugVisual;
    
    private bool _isInitialized = false;

    private int _wallLayerMask;

    private AudioSource _audioSource;

    public void Initialized()
    {
        _isInitialized = true;
        _wallLayerMask = LayerMask.GetMask("Wall");
        debugVisual = Instantiate(debugVisual);
        _audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        if (!_isInitialized) return;
        
        Vector3 rayOrigin = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 rayDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward;
        Ray ray = new Ray(rayOrigin, rayDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _wallLayerMask))
        {
            debugVisual.transform.position = hit.point;
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                Quaternion rotation = Quaternion.LookRotation(-hit.normal);
                Vector3 placementPosition = hit.point + hit.normal * 0.1f;
                Instantiate(prefabToPlace, placementPosition, rotation);
                _audioSource.Play();
            }
        }
    }
}
