using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.XR;

public class XRGizmos_Circle : MonoBehaviour
{
    [SerializeField] private float _radius = 0.25f;
    [SerializeField] [Range(0.001f, 0.3f)] private float thickness = 0.01f;
    [SerializeField] private Color _color_CannotPlace = Color.red;
    [SerializeField] private Color _color_CanPlace = Color.green;
    [SerializeField] private Vector3 _verticalSurfacePositionOffset = new Vector3(-0.1f, 0, 0);
    [SerializeField] private Vector3 _horizonatalSurfacePositionOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _verticalSurfaceRotationOffset = new Vector3(90, 0, 0);
    public bool showGizmo = true;
    private Color _circleColor;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _circleColor = _color_CannotPlace;
    }

    private void Update()
    {
        if (showGizmo)
        {
            XRGizmos.DrawCircle(_transform.position, _transform.rotation, _radius, _circleColor, thickness);
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
        _transform.position = position + _verticalSurfacePositionOffset;
        _transform.rotation = rotation * Quaternion.Euler(_verticalSurfaceRotationOffset);
        UpdateColor(canPlaceBookShelf);
    }

    private void UpdateColor(bool canPlaceBookShelf)
    {
        if(canPlaceBookShelf) _circleColor = _color_CanPlace;
        else _circleColor = _color_CannotPlace;
    }
}
