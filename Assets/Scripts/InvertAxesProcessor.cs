using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class InvertAxesProcessor : InputProcessor<Vector2>
{
	#if UNITY_EDITOR
	static InvertAxesProcessor() {
		Initialize();
	}
	#endif

	[RuntimeInitializeOnLoadMethod]
	static void Initialize() {
		InputSystem.RegisterProcessor<InvertAxesProcessor>();
	}
	
	public override Vector2 Process(Vector2 value, InputControl control) {
		return new Vector2(value.y, value.x);
	}
}
