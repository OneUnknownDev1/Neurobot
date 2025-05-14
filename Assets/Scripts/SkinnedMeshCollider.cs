using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshCollider : MonoBehaviour
{
	[SerializeField] [Range(0.01f, 1.0f)] float updateTime;
	[SerializeField] MeshCollider meshCollider;

	SkinnedMeshRenderer skinnedMeshRenderer;
	float currentTime = 0.0f;

	void Awake()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
	}

	void Update()
	{
		currentTime += Time.deltaTime;

		if (currentTime >= updateTime)
		{
			currentTime = 0.0f;
			UpdateCollider();
		}
	}

	void UpdateCollider() {
		Mesh colliderMesh = new Mesh();
		skinnedMeshRenderer.BakeMesh(colliderMesh);
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = colliderMesh;
	}
}
