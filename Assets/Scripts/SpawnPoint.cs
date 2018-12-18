using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public Transform PointTransform { get; private set; }
	private float _distanceToClosestEnemy;
	private float _distanceToClosestFriend;

	public float SpawnTimer { get; private set; }

	public float DistanceToClosestEnemy//düşman yakınlığı
	{
		get
		{
			return _distanceToClosestEnemy;
		}

		set
		{
			_distanceToClosestEnemy = value;
		}
	}

	public float DistanceToClosestFriend//dost yakınlığı
	{
		get
		{
			return _distanceToClosestFriend;
		}

		set
		{
			_distanceToClosestFriend = value;
		}
	}

	void Awake()
	{
		PointTransform = transform;
        SpawnTimer = 0f;
#if UNITY_EDITOR
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            Debug.LogError("This spawn point has some rotation issues : " + name + " rotation : " + transform.rotation.eulerAngles);
        }
#endif
    }
    public void StartTimer()
	{
		SpawnTimer = 2f;
	}

    private void Update()
    {
        Debug.Log(SpawnTimer);
        if (SpawnTimer > 0)
        {
            Debug.Log(SpawnTimer);
            SpawnTimer -= Time.deltaTime;
        }
    }
}

