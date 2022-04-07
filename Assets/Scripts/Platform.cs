using UnityEngine;

public class Platform : MonoBehaviour {
    [SerializeField] private Material unvisitedMaterial;
    [SerializeField] private Material visitedMaterial;

    private MeshRenderer _meshRenderer;
    
    private void Awake() {
        _meshRenderer ??= GetComponent<MeshRenderer>();
        _meshRenderer.material = unvisitedMaterial;
    }

    private void OnCollisionEnter(Collision other) {
        if ( other.gameObject.CompareTag("Player") ) {
            _meshRenderer.material = visitedMaterial;
            enabled = false;
        }
    }
}
