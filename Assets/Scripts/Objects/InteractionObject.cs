using System.Collections;
using UnityEngine;

using static Gun;

public class InteractionObject : MonoBehaviour
{
    [Header("TYPE")]
    public InteractionType interactionType;
    public AxisType axisType;

    [SerializeField, ColorUsage(false, true)] private Color[] axisColors = new Color[3];

    [Header("BEHAVIOUR")]
    [SerializeField] private float increment;
    [Range(0, 20)]
    [SerializeField] private int maxPositiveIndex;
    [Range(-20, 0)]
    [SerializeField] private int maxNegativeIndex;
    [SerializeField] private float lerpTime = 1f;

    private int currentIndex;
    private MeshRenderer meshRenderer;
    private Transform pivotTransform;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pivotTransform = transform.root;

        meshRenderer.material.color = axisColors[(int)axisType];
    }

    public void DoInteract(ProjectileMode mode)
    {
        if (CheckMaxIncrementReached(mode))
            return;

        switch ((int)interactionType)
        {
            case 0:
                MoveInteraction(increment * (int)mode);
                break;
            case 1:
                RotateInteraction(increment * (int)mode);
                break;
            case 2:
                ScaleInteraction(increment * (int)mode);
                break;
            default:
                break;
        }

        SetIncrementIndex(mode);
    }

    #region Index Logic

    private bool CheckMaxIncrementReached(ProjectileMode mode)
    {
        bool maxIncrementReached = false;

        if ((int)mode == 1)
            maxIncrementReached = currentIndex == maxPositiveIndex;
        else if ((int)mode == -1)
            maxIncrementReached = currentIndex == maxNegativeIndex;

        return maxIncrementReached;
    }

    private void SetIncrementIndex(ProjectileMode mode)
    {
        if ((int)mode == 1)
            currentIndex++;
        else if ((int)mode == -1)
            currentIndex--;
    }

    #endregion

    #region Interaction Behaviour

    public void MoveInteraction(float increment)
    {
        switch ((int)axisType)
        {
            case 0:
                StartCoroutine(LerpMove(pivotTransform.position + new Vector3(increment, 0f, 0f)));
                break;
            case 1:
                StartCoroutine(LerpMove(pivotTransform.position + new Vector3(0f, increment, 0f)));
                break;
            case 2:
                StartCoroutine(LerpMove(pivotTransform.position + new Vector3(0f, 0f, increment)));
                break;
            default:
                break;
        }
    }

    public void RotateInteraction(float increment)
    {
        switch ((int)axisType)
        {
            case 0:
                StartCoroutine(LerpRotate(pivotTransform.rotation.eulerAngles + new Vector3(increment, 0f, 0f)));
                break;
            case 1:
                StartCoroutine(LerpRotate(pivotTransform.rotation.eulerAngles + new Vector3(0f, increment, 0f)));
                break;
            case 2:
                StartCoroutine(LerpRotate(pivotTransform.rotation.eulerAngles + new Vector3(0f, 0f, increment)));
                break;
            default:
                break;
        }
    }

    public void ScaleInteraction(float increment)
    {
        switch ((int)axisType)
        {
            case 0:
                StartCoroutine(LerpScale(pivotTransform.localScale + new Vector3(increment, 0f, 0f)));
                break;
            case 1:
                StartCoroutine(LerpScale(pivotTransform.localScale + new Vector3(0f, increment, 0f)));
                break;
            case 2:
                StartCoroutine(LerpScale(pivotTransform.localScale + new Vector3(0f, 0f, increment)));
                break;
            default:
                break;
        }
    }

    IEnumerator LerpMove(Vector3 targetPosition)
    {
        float currentTime = 0f;
        Vector3 initialPosition = pivotTransform.position;

        while (currentTime <= lerpTime)
        {
            pivotTransform.position = Vector3.Lerp(initialPosition, targetPosition, (currentTime / lerpTime));
            currentTime += Time.deltaTime;
            yield return null;
        }

        pivotTransform.position = targetPosition;
    }

    IEnumerator LerpRotate(Vector3 targetRotation)
    {
        float currentTime = 0f;
        Vector3 initialRotation = pivotTransform.rotation.eulerAngles;

        while (currentTime <= lerpTime)
        {
            pivotTransform.rotation = Quaternion.Euler(Vector3.Lerp(initialRotation, targetRotation, (currentTime / lerpTime)));
            currentTime += Time.deltaTime;
            yield return null;
        }

        pivotTransform.rotation = Quaternion.Euler(targetRotation);
    }

    IEnumerator LerpScale(Vector3 targetScale)
    {
        float currentTime = 0f;
        Vector3 initialScale = pivotTransform.localScale;

        while (currentTime <= lerpTime)
        {
            pivotTransform.localScale = Vector3.Lerp(initialScale, targetScale, (currentTime / lerpTime));
            currentTime += Time.deltaTime;
            yield return null;
        }

        pivotTransform.localScale = targetScale;
    }

    #endregion

    public enum InteractionType
    {
        Move,
        Rotate,
        Scale
    }

    public enum AxisType
    {
        X,
        Y,
        Z
    }
}
