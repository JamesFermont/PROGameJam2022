using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [Range(0, 20)]
    [SerializeField] private int maxNegativeIndex;
    [Space]
    [SerializeField] private Transform pivotTransform;

    private int currentPositiveIndex;
    private int currentNegativeIndex;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pivotTransform = pivotTransform == null ? this.transform : pivotTransform;

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
            maxIncrementReached = currentPositiveIndex == maxPositiveIndex;
        else if ((int)mode == -1)
            maxIncrementReached = currentNegativeIndex == maxNegativeIndex;

        return maxIncrementReached;
    }

    private void SetIncrementIndex(ProjectileMode mode)
    {
        if ((int)mode == 1)
        {
            currentPositiveIndex += currentNegativeIndex > 0 ? 0 : 1;
            currentNegativeIndex += currentNegativeIndex > 0 ? -1 : 0;
        }
        else if ((int)mode == -1)
        {
            currentNegativeIndex += currentPositiveIndex > 0 ? 0 : 1;
            currentPositiveIndex += currentPositiveIndex > 0 ? -1 : 0;
        }
    }

    #endregion

    #region Interaction Behaviour

    public void MoveInteraction(float increment)
    {
        switch ((int)axisType)
        {
            case 0:
                pivotTransform.position += new Vector3(increment, 0f, 0f);
                break;
            case 1:
                pivotTransform.position += new Vector3(0f, increment, 0f);
                break;
            case 2:
                pivotTransform.position += new Vector3(0f, 0f, increment);
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
                pivotTransform.Rotate(new Vector3(increment, 0f, 0f));
                break;
            case 1:
                pivotTransform.Rotate(new Vector3(0f, increment, 0f));
                break;
            case 2:
                pivotTransform.Rotate(new Vector3(0f, 0f, increment));
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
                pivotTransform.localScale += new Vector3(increment, 0f, 0f);
                break;
            case 1:
                pivotTransform.localScale += new Vector3(0f, increment, 0f);
                break;
            case 2:
                pivotTransform.localScale += new Vector3(0f, 0f, increment);
                break;
            default:
                break;
        }
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

    public enum ProjectileMode
    {
        Positive = 1,
        Negative = -1
    }
}
