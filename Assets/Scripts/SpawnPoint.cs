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

    /// <summary>
    /// PointTransform değişkenine bağlı olduğu game objectin pozisyonunu(kendi pozisyonunu) atar
    /// kendi pozisyonunu x ve z pozisyonları 0 dan farklı bir değerde ise bu pozisyonlarda sıkıntı çıkabalir
    /// player x ve z si 0 dan farklı bir noktaya ışınlandığı zaman dik durmayabilir bu yüzden bu kontrol yapılır 
    /// </summary>
	void Awake()
	{
		PointTransform = transform;
#if UNITY_EDITOR
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            Debug.LogError("This spawn point has some rotation issues : " + name + " rotation : " + transform.rotation.eulerAngles);
        }
#endif
    }
    /// <summary>
    /// SpawnTimer ı (ışınlanma süresini) başlatır
    /// </summary>
    public void StartTimer()
	{
		SpawnTimer = 2f;
	}
    /// <summary>
    /// SpawnTimer 0 dan büyükse bu süreyi Time.deltaTime kadar azaltır yani iki saniye sonra SpawnTimer 0 lanır. 
    /// </summary>
    private void Update()
    {
        if (SpawnTimer > 0)
        {
            SpawnTimer -= Time.deltaTime;
        }
    }
}

