using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.XR;

public class PositionBookShelf : MonoBehaviour
{
    [SerializeField] private GameObject _bookShelf;
    [SerializeField] private XRGizmos_Circle _xrCircle;
    [SerializeField] private GameObject _textObject;
    private OVRCameraRig _cameraRig;
    private bool _positioningBookShelf = true;

    void Start()
    {
        if (!_cameraRig) _cameraRig = FindObjectOfType<OVRCameraRig>();
        if (!_xrCircle) _xrCircle = FindObjectOfType<XRGizmos_Circle>();
    }

    private void FixedUpdate()
    {
        if(_positioningBookShelf) GetBestPoseFromRaycastDebugger();
    }

    private void GetBestPoseFromRaycastDebugger()
    {
        Ray ray = GetRightControllerRay(out bool rightControllerAnchor_IsNull);
        if(rightControllerAnchor_IsNull) return;
        
        MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT;
        Pose? bestPose = GetBestPoseFromRaycast(ray, Mathf.Infinity, new LabelFilter(), 
                out MRUKAnchor sceneAnchor, out Vector3 surfaceNormal, positioningMethod);

        bool hitIsOnVerticalSurface = true;
        // If the normal of the raycast hit is parallel to Vector3.up,
        bool isParallel = Mathf.Abs(Vector3.Dot(surfaceNormal, Vector3.up)) >= 0.9f;
        if(isParallel) hitIsOnVerticalSurface = false;
        
        
        if (bestPose.HasValue && sceneAnchor && _bookShelf)
        {
            bool canPlaceBookShelf = MRUKAnchorCanHoldBookShelf(sceneAnchor);
            if(!hitIsOnVerticalSurface) 
                _xrCircle.PlaceOnHorizontalSurface(bestPose.Value.position, bestPose.Value.rotation, canPlaceBookShelf);
            else
            {
                _xrCircle.PlaceOnVerticalSurface(bestPose.Value.position, bestPose.Value.rotation, canPlaceBookShelf);
                // XRGizmos.DrawPointer(bestPose.Value.position, surfaceNormal, Color.magenta);
            }

            if (canPlaceBookShelf)
            {
                _bookShelf.transform.position = bestPose.Value.position;
                _bookShelf.transform.rotation = bestPose.Value.rotation;
                //If trigger button is pressed
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                {
                    _positioningBookShelf = false;
                    _xrCircle.showGizmo = false;
                    // if(_textObject) _textObject.SetActive(false);
                }
            }
        }
    }
    
    Ray GetRightControllerRay(out bool rightControllerAnchor_IsNull)
    {
        rightControllerAnchor_IsNull = false;
        if (!_cameraRig.rightControllerAnchor)
        {
            rightControllerAnchor_IsNull = true;
            return new Ray();
        }
        
        Vector3 rayOrigin = _cameraRig.rightControllerAnchor.position;
        Vector3 rayDirection = _cameraRig.rightControllerAnchor.forward;
        return new Ray(rayOrigin, rayDirection);
    }

    private bool MRUKAnchorCanHoldBookShelf(MRUKAnchor mrukAnchor)
    {
        return  (mrukAnchor.name == "WALL_FACE" || mrukAnchor.name == "TABLE" || mrukAnchor.name == "STORAGE" || 
                 mrukAnchor.name == "FLOOR" || mrukAnchor.name == "BED");
        // return true;
    }

    /// Calling 'GetBestPoseFromRaycast' function in <see cref="MRUKRoom"/> class doesn't provide the normal of the raycast hit 
    /// <param name="surfaceNormal"></param>
    /// So copied the function to this class to have access to it
    /// Alternative; "MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast"
    private Pose GetBestPoseFromRaycast(Ray ray, float maxDist, LabelFilter labelFilter, out MRUKAnchor sceneAnchor, 
        out Vector3 surfaceNormal, MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT)
        {
            sceneAnchor = null;
            Pose bestPose = new Pose();
            surfaceNormal = Vector3.up;

            if (MRUK.Instance.GetCurrentRoom().Raycast(ray, maxDist, labelFilter, out var closestHit, out sceneAnchor))
            {
                Vector3 defaultPose = closestHit.point;
                surfaceNormal = closestHit.normal;
                Vector3 poseUp = Vector3.up;
                // by default, use the surface normal for pose forward
                // caution: make sure all the cases of this being "up" are caught below
                Vector3 poseFwd = closestHit.normal;

                if (Vector3.Dot(closestHit.normal, Vector3.up) >= 0.9f && sceneAnchor.VolumeBounds.HasValue)
                {
                    // this is a volume object, and the ray has hit the top surface
                    // "snap" the pose Z to align with the closest edge
                    Vector3 toPlane = ray.origin - sceneAnchor.transform.position;
                    Vector3 planeYup = Vector3.Dot(sceneAnchor.transform.up, toPlane) > 0.0f ? sceneAnchor.transform.up : -sceneAnchor.transform.up;
                    Vector3 planeXup = Vector3.Dot(sceneAnchor.transform.right, toPlane) > 0.0f ? sceneAnchor.transform.right : -sceneAnchor.transform.right;
                    Vector3 planeFwd = sceneAnchor.transform.forward;

                    Vector2 anchorScale = sceneAnchor.VolumeBounds.Value.size;
                    Vector3 nearestCorner = sceneAnchor.transform.position + planeXup * anchorScale.x * 0.5f + planeYup * anchorScale.y * 0.5f;
                    Vector3.OrthoNormalize(ref planeFwd, ref toPlane);
                    nearestCorner -= sceneAnchor.transform.position;
                    bool xUp = Vector3.Angle(toPlane, planeYup) > Vector3.Angle(nearestCorner, planeYup);
                    poseFwd = xUp ? planeXup : planeYup;
                    float offset = xUp ? anchorScale.x : anchorScale.y;
                    switch (positioningMethod)
                    {
                        case MRUK.PositioningMethod.CENTER:
                            defaultPose = sceneAnchor.transform.position;
                            break;
                        case MRUK.PositioningMethod.EDGE:
                            defaultPose = sceneAnchor.transform.position + poseFwd * offset * 0.5f;
                            break;
                        case MRUK.PositioningMethod.DEFAULT:
                            break;
                    }
                }
                else if (Mathf.Abs(Vector3.Dot(closestHit.normal, Vector3.up)) >= 0.9f)
                {
                    // This may be the floor, ceiling or any other horizontal plane surface
                    poseFwd = new Vector3(ray.origin.x - closestHit.point.x, 0, ray.origin.z - closestHit.point.z).normalized;
                }
                bestPose.position = defaultPose;
                bestPose.rotation = Quaternion.LookRotation(poseFwd, poseUp);
            }
            else
            {
                Debug.Log("Best pose not found, no surface anchor detected.");
            }

            return bestPose;
        }

}
