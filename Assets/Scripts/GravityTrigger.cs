using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GravityTrigger : MonoBehaviour {
    [SerializeField] private GravitySource gravityToEnable;
    [SerializeField] private GravitySource gravityToDisable;

    private void OnTriggerEnter(Collider other) {
        if ( other.CompareTag("Player") ) {
            if ( gravityToDisable ) {
                gravityToDisable.enabled = false;
            }
            if ( gravityToEnable ) {
                gravityToEnable.enabled = true;
            }
        }

        enabled = false;
    }

    private void OnDrawGizmos() {
        BoxCollider col = GetComponent<BoxCollider>();
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, col.size);
    }
}
