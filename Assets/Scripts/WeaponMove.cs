using UnityEngine;

[ExecuteAlways]
public class WeaponMove : MonoBehaviour {
    [SerializeField] private Transform walkTransform, shootTransform;
    [SerializeField, Range(0f, 1f)] private float t;

    private void Update() {
        if ( !walkTransform || !shootTransform ) {
            return;
        }

        Vector3 position = Vector3.Lerp(walkTransform.position, shootTransform.position, t);
        Quaternion rotation = Quaternion.Slerp(walkTransform.rotation, shootTransform.rotation, t);
        transform.SetPositionAndRotation(position, rotation);
    }
}
