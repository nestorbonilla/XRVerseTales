using UnityEngine;

public class BookAnimator : MonoBehaviour
{
    public Transform bookTransform;
    private float _minimumY = 0f;
    private float _maximumY = 0.015f;
    private float _duration = 3f;

    private float _startY;
    private float _startTime;

    private void Start()
    {
        Application.targetFrameRate = 90;
        _startY = bookTransform.position.y;
        _startTime = Time.time;
    }

    private void Update()
    {
        float t = (Time.time - _startTime) / _duration;
        float newY = _startY + Mathf.Sin(t * Mathf.PI * 2) * (_maximumY - _minimumY) / 2 + (_maximumY + _minimumY) / 2;
        bookTransform.position = new Vector3(bookTransform.position.x, newY, bookTransform.position.z);
    }
}