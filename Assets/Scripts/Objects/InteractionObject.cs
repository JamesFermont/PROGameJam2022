using System.Collections;
using UnityEngine;
using System;

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

    private Action<float>[] interactions;

    void Awake()
    {
        interactions = new Action<float>[] { MoveInteraction, RotateInteraction, ScaleInteraction };
    }

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pivotTransform = transform.root;

        meshRenderer.material.color = axisColors[(int)axisType];
    }

    public void DoInteract(ProjectileMode mode)
    {
        if (!TryChangeIndex(mode))
            return;

        float delta = increment * (int)mode;

        interactions[(int)interactionType](delta);
    }

    #region Index Logic

    private bool TryChangeIndex(ProjectileMode mode)
    {
        int previousIndex = currentIndex;

        currentIndex += (int)mode;
        currentIndex = Mathf.Clamp(currentIndex, maxNegativeIndex, maxPositiveIndex);

        return currentIndex != previousIndex;
    }

    #endregion

    #region Interaction Behaviour

    public void MoveInteraction(float delta)
    {
        StartCoroutine(LerpMove(pivotTransform.position + GetAxisVector(axisType) * delta));
    }

    public void RotateInteraction(float delta)
    {
        StartCoroutine(LerpRotate(pivotTransform.rotation.eulerAngles + GetAxisVector(axisType) * delta));
    }

    private void ScaleInteraction(float delta)
    {
        StartCoroutine(LerpScale(pivotTransform.localScale + GetAxisVector(axisType) * delta));
    }

    private Vector3 GetAxisVector(AxisType axis)
    {
        Vector3 a = Vector3.zero;
        a[(int)axis] = 1f;
        return a;
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
