using UnityEngine;
using System.Collections;

public class SpawnPointGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Color _color;
    /// <summary>
	/// Arayüzden tanımlanan renge göre
	/// sahne ekranındayken bağlı olduğu gameObject in x,y,z pozisyonunda (y +1 fazla ekranda gözükmesi için) küre çizer.
	/// </summary>
	void OnDrawGizmos()
	{
		// Draw spawn point gizmo  
		Gizmos.color = _color;
		Vector3 startPoint = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);
		Gizmos.DrawSphere(startPoint, 1);
	}
#endif
}