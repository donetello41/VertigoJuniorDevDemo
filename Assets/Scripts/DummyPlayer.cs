using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
	public string Name;
	public PlayerTeam PlayerTeamValue;
	public bool Disabled;
	public bool _IsDead;
    //Player ın ölü olup olmadığını döndürür
	public bool IsDead()
	{
		return _IsDead;
	}
	public Transform Transform;
}