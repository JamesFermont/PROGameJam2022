using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GravityBoxVolume : GravitySource {
	private enum KillDirection {
		None = 0,
		Up = 1,
		Down = -1
	}
	
	[SerializeField] private float gravity = 9.81f;
	[SerializeField] private KillDirection killDirection;
	[SerializeField] private Transform respawnPoint;
	private bool _hasPlayer;
	
	public override Vector3 GetGravity(Vector3 position) {
		if ( !_hasPlayer ) {
			return Vector3.zero;
		}

		float g = -gravity;
		return transform.up * g;
	}

	private void OnTriggerEnter(Collider other) {
		if ( other.CompareTag("Player") ) {
			_hasPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if ( other.CompareTag("Player") ) {
			_hasPlayer = false;
		}

		if ( killDirection != KillDirection.None ) {
			Vector3 exitPointY = new Vector3(0f, other.transform.position.y - transform.position.y, 0f).normalized;
			int dot = Mathf.RoundToInt(Vector3.Dot(transform.up, exitPointY));
			Debug.Log(dot);
			if ( dot == (int)killDirection ) {
				Debug.Log("Counts as kill!");
				other.transform.position = respawnPoint.position;
			}
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(transform.position, transform.up * 5f);
	}
}
