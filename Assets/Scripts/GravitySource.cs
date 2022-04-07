using UnityEngine;

public class GravitySource : MonoBehaviour
{
    public virtual Vector3 GetGravity(Vector3 position) {
        return Physics.gravity;
    }

    private void OnEnable() {
        Gravity.Register(this);
    }

    private void OnDisable() {
        Gravity.Unregister(this);
    }
}
