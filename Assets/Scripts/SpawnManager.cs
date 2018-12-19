using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerTeam
{
    None,
    BlueTeam,
    RedTeam
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _sharedSpawnPoints = new List<SpawnPoint>();
    System.Random _random = new System.Random();
	float _closestDistance;
    [Tooltip("This will be used to calculate the second filter where algorithm looks for closest friends, if the friends are away from this value, they will be ignored")]
    [SerializeField] private float _maxDistanceToClosestFriend = 30;
    [Tooltip("This will be used to calculate the first filter where algorithm looks for enemies that are far away from this value. Only enemies which are away from this value will be calculated.")]
    [SerializeField] private float _minDistanceToClosestEnemy = 10;
    [Tooltip("This value is to prevent friendly player spawning on top of eachothers. If a player is within the range of this value to a spawn point, that spawn point will be ignored")]
    [SerializeField] private float _minMemberDistance = 2;


    public DummyPlayer PlayerToBeSpawned;
    public DummyPlayer[] DummyPlayers;

    /// <summary>
    /// _sharedSpawnPoints dinamik listesine SpawnPoint tipindeki objelerin tamamını ekler.
    /// DummyPlayers dizisine bütün DummyPlayer tipindeki objeleri ekler.
    /// </summary>
     private void Awake()
    {
		_sharedSpawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
		DummyPlayers = FindObjectsOfType<DummyPlayer>();
    }

    #region SPAWN ALGORITHM
    /// <summary>
    /// spawn noktalarının sayısı kadar boş bir liste ouşturur 
    /// Işınlanma noktalarına enyakındost (DistanceToClosestFriend) uzaklığı ve 
    /// enyakındüşman uzaklığını (DistanceToClosestEnemy) hesaplanır.(calculateDistancesForSpawnPoints)
    /// Düşmanlarımıza olan uzaklığa göre ışınlanma noktası seçimi çalışır. Parametre olarak spawnPoints dizisinin referansını
    /// ve ışınlanacak oyuncunun takımını alır. _sharedSpawnPoints leri DistanceToClosestEnemy değerine göre küçükten büyüğe sıralar.
    /// Bu spawn noktalarına en yakın düşman değeri _minDistanceToClosestEnemy büyükse
    /// ve herhangi bir oyuncunun o spawn noktasına olan uzaklığı _minMemberDeğerinden yüksek ise ve o spawn noktasına
    /// 2 saniye içinde kimse ışınlanmamışsa o nokta SpawnPoints (ışınlanma noktasına) dizisine aktarılır.
    /// Eğer spawnPoints dizisi boş ise dostlarımıza olan uzaklığa göre spawn noktası seçimi çalışır.
    /// bu method parametre olarak spawnsPoints dizisinin referansını ve ışınlanacak oyuncunun takımını alır.
    /// _sharedSpawnPoints leri DistanceToClosestFriend değerine göre küçükten büyüğe sıralar.
    /// Bu spawn noktalarına en yakın dost(DistanceToClosestFriend) değeri _maxDistanceToClosestFriend büyükse
    /// ve herhangi bir oyuncunun o spawn noktasına olan uzaklığı _minMemberDeğerinden yüksek ise ve o spawn noktasına
    /// 2 saniye içinde kimse ışınlanmamışsa o nokta SpawnPoints (ışınlanma noktasına) dizisine aktarılır.
    /// Eğer spawnPoints dizisi yine boşsa _sharedSpawnPoints(paylaşımlı ışınlanma noktalatının) lerin ilk indexi ışınlanma noktası olarak atanır.
    /// spawnPoints listesinde 2 den fazla eleman yok ise spawnPoint noktasına spawnPoints listesinin ilk elemanı atanır
    /// Eğer spawnPoints listesinde 2 den fazla eleman var ise hangi  noktanın spawnPoint olcağına şu şekilde karar verilir.
    /// spawnPoints listesinin eleman sayısının yarısı alınır tam çıkmazsa bir alt değere yuvarlanır ve bu çıkan sayı ile 0 arasında rastgele bir sayı üretilir.
    /// bu üretilen sayı spawnPoints Listesinin indisi olur ve bu nokta spawnPoint olarak belirlenir.
    /// bu ışınlanma noktası en uygun ışınlanma noktası olarak gönderilir.
    /// </summary>
    /// <param name="team">player ın takımı</param>
    /// <returns>uygun bir ışınlanma noktası döndürür</returns>

    public SpawnPoint GetSharedSpawnPoint(PlayerTeam team)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>(_sharedSpawnPoints.Count);
        CalculateDistancesForSpawnPoints(team);
        GetSpawnPointsByDistanceSpawning(team, ref spawnPoints);
        if (spawnPoints.Count <= 0)
        {
            GetSpawnPointsBySquadSpawning(team, ref spawnPoints);
        }
        SpawnPoint spawnPoint = spawnPoints.Count <= 1 ? spawnPoints[0] : spawnPoints[_random.Next(0, (int)((float)spawnPoints.Count * .5f))];


        spawnPoint.StartTimer();
        return spawnPoint;
    }

    /// <summary>
    /// suitableSpawnPoints noktalarını temizler.
    /// paylaşımlı ışınlanma noktaları (_sharedSpawnPoints) arasında DistanceToClosestEnemy değerine göre küçükten büyüğe sıralama yapar.
    /// paylaşımlı ışınlanma noktalarının DistanceToClosestEnemy değeri _minDistanceToClosestEnemy değerinden büyükse ve
    /// herhangi bir oyuncunun o spawn noktasına olan uzaklığı _minMemberDeğerinden küçük ise
    /// o noktayı suitableSpawnPoints listesine ekle
    /// </summary>
    /// <param name="team">ışınlanacak oyuncunun takımı</param>
    /// <param name="suitableSpawnPoints">spawnpoints lerin referansı</param>
    private void GetSpawnPointsByDistanceSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();

        //anonmous method da _sharedSpawnPoints noktalarını arasında DistanceToClosestEnemy değerine göre
        //küçükten büyüğe sıralama yapar
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestEnemy == b.DistanceToClosestEnemy)
            {
                return 0;
            }
            if (a.DistanceToClosestEnemy > b.DistanceToClosestEnemy)
            {
                return 1;
            }
            return -1;
        });
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            if (_sharedSpawnPoints[i].DistanceToClosestEnemy > _minDistanceToClosestEnemy)
            {
                if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
                {
                    suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
                }
            }
        }
    }
    /// <summary>
    /// spawnpoint dizisiniboşaltır ve paylaşımlı ışınlanma noktalarını(_sharedSpawnPoints)
    /// DistanceToClosestFriend(en yakın Dost) değerine göre küçükten büyüğe doğru sıralar
    /// eğer paylaşılmış ışınlanma(_saharedSpawnPoints) noktaları var ise ve bu noktaların
    /// en yakın arkadaş uzaklığının(DistanceToClosestFriend) değeri maximum yakınlık uzaklığından(_maxDistanceToclosest)
    /// küçükse ve herhangi bir oyuncun o spawn noktasına olan uzaklığı _minMemberDeğerinden yüksek ise
    /// o noktayı suitableSpawnPoints listesine ekle
    /// suitableSpawnPoints noktasında hiç eleman yoksa bu listeye _saharedSpawnPoints bu noktaların ilkini ata
    /// </summary>
    /// <param name="team">ışınlanacak oyuncunun nun takımı</param>
    /// <param name="suitableSpawnPoints">spawnpoints lerin referansı</param>
    private void GetSpawnPointsBySquadSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();

        //anonmous method da _sharedSpawnPoints noktalarını arasında DistanceToClosestFriend değerine göre
        //küçükten büyüğe sıralama yapar
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestFriend == b.DistanceToClosestFriend)
            {
                return 0;
            }
            if (a.DistanceToClosestFriend > b.DistanceToClosestFriend)
            {
                return 1;
            }
            return -1;
        });
        for (int i = 0; i < _sharedSpawnPoints.Count && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend; i++)
        {
            if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
            }
        }
        if (suitableSpawnPoints.Count <= 0)
        {
            suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
        }

    }

    /// <summary>
    /// paylaşımlı ışınlanma noktalarına (_sharedSpawnPoints)
    /// o noktaya en yakın dost(DistanceToClosestFriend) ve en yakın düşman(DistanceTOClosestEnemy)
    /// mesafelerini hesaplar
    /// </summary>
    /// <param name="playerTeam">player ın Takımı </param>
    private void CalculateDistancesForSpawnPoints(PlayerTeam playerTeam)
    {
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            _sharedSpawnPoints[i].DistanceToClosestFriend = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam);
            _sharedSpawnPoints[i].DistanceToClosestEnemy = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam == PlayerTeam.BlueTeam ? PlayerTeam.RedTeam : playerTeam == PlayerTeam.RedTeam ? PlayerTeam.BlueTeam : PlayerTeam.None);
            //Düşman yakınlığını bulmak için playerTeam Tersleyip gönderiliyor
        }
    }

    /// <summary>
    /// _sharedSpawnPoints noktalarının pozisyonunu tek tek parametre olarak alır.
    /// ışınlanacak olan kişi (player) kendi ise döngü bir sonraki index'ten devam eder.
    /// Player lar disabled değil, ölmemiş, takımı var ve atnı takımda ise
    /// parametre olarak alınan mesafeye olan uzaklıkl hesaplanır.
    /// eğer döngü ilk defa dönüyorsa en yakın mesafe(_closestDistance) ilk player ınki alınır.
    /// değil ise önceki en yakın mesafe ile karşılaştırılır.
    /// eğer küçükse en yakın mesafe(_ClosestDistance) güncellenir
    /// </summary>
    /// <param name="position">paylaşılmış ışınlanma noktaları(_SharedSpawnPoints) </param>
    /// <param name="playerTeam">Player ın takımı</param>
    /// <returns>o noktadaki En yakın takım arkadaşının mesafesini döndürür</returns>
    private float GetDistanceToClosestMember(Vector3 position, PlayerTeam playerTeam)
    {
        bool firstLoop = true;
        foreach (var player in DummyPlayers)
        {
            if (PlayerToBeSpawned == player)
            {
                continue;
            }
            if (!player.Disabled && player.PlayerTeamValue != PlayerTeam.None && player.PlayerTeamValue == playerTeam && !player.IsDead())
            {
                float playerDistanceToSpawnPoint = Vector3.Distance(position, player.Transform.position);

                if (firstLoop)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                    firstLoop = false;
                }
                if (playerDistanceToSpawnPoint < _closestDistance)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                }
            }
        }
        return _closestDistance;
    }

    #endregion
	/// <summary>
	/// Test için paylaşımlı spawn noktalarından en uygun olanını seçer.
	/// Test oyuncusunun pozisyonunu seçilen spawn noktasına atar.
	/// </summary>
    public void TestGetSpawnPoint()
    {
    	SpawnPoint spawnPoint = GetSharedSpawnPoint(PlayerToBeSpawned.PlayerTeamValue);
        PlayerToBeSpawned.Transform.position = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y + 1, spawnPoint.transform.position.z);// spawnPoint.PointTransform.position;
    }

}