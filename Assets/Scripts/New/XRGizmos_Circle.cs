using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.XR;

public class XRGizmos_Circle : MonoBehaviour
{
    [SerializeField] private float _circleRadius = 0.25f;
    [SerializeField] [Range(0.001f, 0.3f)] private float _circleThickness = 0.01f;
    [SerializeField] [Range(0.001f, 1f)] private float _pointerSize = 0.5f;
    [SerializeField] [Range(0.001f, 0.3f)] private float _pointerThickness = 0.006f;
    [SerializeField] private Color _color_CannotPlace = Color.red;
    [SerializeField] private Color _color_CanPlace = Color.green;
    [SerializeField] private Vector3 _horizonatalSurfacePositionOffset = new Vector3(0, -0.02f, 0);
    [SerializeField] private Vector3 _verticalSurfaceRotationOffset = new Vector3(90, 0, 0);
    public bool showGizmo = true;
    private Color _gizmoColor;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _gizmoColor = _color_CannotPlace;
    }

    private void Update()
    {
        if (showGizmo)
        {
            // Draw a circle behind the bookshelf and in front of the vertical surface 
            XRGizmos.DrawCircle(_transform.position, _transform.rotation, _circleRadius, _gizmoColor, _circleThickness);
            // Draw a pointer pointing to the forward position of the bookshelf
            XRGizmos.DrawPointer(_transform.position, _transform.up, _gizmoColor,_pointerSize, _pointerThickness );
            
            // XRGizmos.DrawAxis(_transform, lineThickness: thickness);
        }
    }
    
    public void PlaceOnHorizontalSurface(Vector3 position, Quaternion rotation, bool canPlaceBookShelf)
    {
        _transform.position = position + _horizonatalSurfacePositionOffset;
        _transform.rotation = rotation;
        UpdateColor(canPlaceBookShelf);
    }
    
    public void PlaceOnVerticalSurface(Vector3 position, Quaternion rotation, bool canPlaceBookShelf)
    {
        // An offset is added to the bestPose position to accommodate for all 4 vertical surfaces (assuming walls in this case)
        // and ensure the circle is drawn behind the bookshelf and in front of the vertical surface 

        float offset = 0.09f;
        
        // If vertical surface is parallel to 'Vector3.right'
        if (Mathf.Abs(Vector3.Dot(_transform.right, Vector3.right)) >= 0.9f)
        {
            bool isPositiveParallelToWorld_XAxis = Vector3.Dot(_transform.right, Vector3.right) >= 0.9f;
            if (isPositiveParallelToWorld_XAxis) _transform.position = position +  new Vector3(0, 0, -offset);
        
            bool isNegativeParallelToWorld_XAxis = Vector3.Dot(_transform.right, Vector3.right) <= -0.9f;
            if(isNegativeParallelToWorld_XAxis) _transform.position = position +  new Vector3(0, 0, offset);
        }
        // If vertical surface is parallel to 'Vector3.right'
        else if (Mathf.Abs(Vector3.Dot(_transform.up, Vector3.right)) >= 0.9f)
        {
            bool isPositiveParallelToWorld_ZAxis = Vector3.Dot(_transform.up, Vector3.right) >= 0.9f;
            if (isPositiveParallelToWorld_ZAxis) _transform.position = position + new Vector3(-offset, 0, 0);

            bool isNegativeParallelToWorld_ZAxis = Vector3.Dot(_transform.up, Vector3.right) <= -0.9f;
            if(isNegativeParallelToWorld_ZAxis) _transform.position = position +  new Vector3(offset, 0, 0);
        }
        else _transform.position = position;
     
        _transform.rotation = rotation * Quaternion.Euler(_verticalSurfaceRotationOffset);

        UpdateColor(canPlaceBookShelf);
    }

    private void UpdateColor(bool canPlaceBookShelf)
    {
        if(canPlaceBookShelf) _gizmoColor = _color_CanPlace;
        else _gizmoColor = _color_CannotPlace;
    }
}
