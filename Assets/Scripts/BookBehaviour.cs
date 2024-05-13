using System.Collections;
using UnityEngine;

public class BookBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject pdfReader;
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform centerEyeAnchor;
    
    private static readonly int OpenTrigger = Animator.StringToHash("OpenTrigger");
    private static readonly int CloseTrigger = Animator.StringToHash("CloseTrigger");

    public void OpenBook()
    {
        bookAnimator.SetTrigger(OpenTrigger);
        StartCoroutine(LerpRotation(Quaternion.Euler(180, 180, 270)));
        StartCoroutine(DelayedActivation(1.2f));
    }

    public void CloseBook()
    {
        pdfReader.SetActive(false);
        bookAnimator.SetTrigger(CloseTrigger);
    }
    
    IEnumerator LerpRotation(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    IEnumerator DelayedActivation(float delay)
    {
        yield return new WaitForSeconds(delay);
        pdfReader.SetActive(true);
    }
}