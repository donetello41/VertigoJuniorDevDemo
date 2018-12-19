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
    public SpawnPoint GetSharedSpawnPoint(PlayerTeam team)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>(_sharedSpawnPoints.Count);

        CalculateDistancesForSpawnPoints(team);
        //Işınlanma noktalarına enyakındost (DistanceToClosestFriend) uzaklığı ve
        //enyakındüşman uzaklığını (DistanceToClosestEnemy) hesapla

        GetSpawnPointsByDistanceSpawning(team, ref spawnPoints);


        //ışınlanma Noktaları Yoksa
        if (spawnPoints.Count <= 0)
        {
            GetSpawnPointsBySquadSpawning(team, ref spawnPoints);
        }
        //ışınlanma noktaları 1 den az ise ilk indexse ışınlan 1 den fazla ise
        //ışınlanma noktalarını 1/2 ile çarp ve virgüllü çıkarsa bir alt değere yuvarla sonra bunların arasında rastgele bir index seç
        //ve ışınlanma noktası olarak belirle
        SpawnPoint spawnPoint = spawnPoints.Count <= 1 ? spawnPoints[0] : spawnPoints[_random.Next(0, (int)((float)spawnPoints.Count * .5f))];
        //ışınlanma süresini başlat
        spawnPoint.StartTimer();
        return spawnPoint;
    }

    private void GetSpawnPointsByDistanceSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        //Please apply your algorithm here
        //seçilecek olan spawn noktasına en yakın düşmanın uzaklığı _minDistanceToClosestEnemy değerinden yüksek olmalı
        //Herhangibir oyuncunun(düşman/dost) o spawn noktasına olan uzaklığı _minMemberDistance değerinden yüksek olmalı
        //spawn noktasında 2 saniye içinde bir spawn yapılmamış olmalı
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();
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

    private void GetSpawnPointsBySquadSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();

        //_sharedSpawnPoints noktalarının en yakın takım arkadaşının mesafesine göre sıralama yapıyor;
        //
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
        //paylaşılmıs ışınlanma noktalarının sayısı i den büyükse ve
        //paylaşılmış ışınlanma noktalarının i inci paylaşılmış ışınlanma noktasına olan en yakın arkadaşı
        //küçük ise  _maxDistanceToClosetFriend
        for (int i = 0; i < _sharedSpawnPoints.Count && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend; i++)
        {
            //i inci paylaşılmış ışınlanma noktasının distanceToClosestFriend <= _minMemberDistance değil ise
            //(yani takımındaki arkadaşlarının üst üste ışınlanmasını önlemek amacıyla) ve
            //DistanceToClosestEnemy <= _minMemberDistance değil ise
            //(yani düşmanın üstüne ışınlanmamak için) ve iki saniye içinde başka bir ışınlanma olmadı ise
            if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                //i inci _sharedSpawnPoints noktasını uygun ışınlanılabilicek noktalar listesine ekle 
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
            }
        }
        //uygun ışınlanılabilicek noktalar listesi boşsa
        if (suitableSpawnPoints.Count <= 0)
        {
            //uygun ışınlanılabilicek noktalar listesine paylaşılmış ışınlanma noktalarının ilk elemanını gönder
            suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
        }

    }

    private void CalculateDistancesForSpawnPoints(PlayerTeam playerTeam)
    {
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            _sharedSpawnPoints[i].DistanceToClosestFriend = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam);
            _sharedSpawnPoints[i].DistanceToClosestEnemy = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam == PlayerTeam.BlueTeam ? PlayerTeam.RedTeam : playerTeam == PlayerTeam.RedTeam ? PlayerTeam.BlueTeam : PlayerTeam.None);
            //Düşman yakınlığını bulmak için playerTeam Tersleyip gönderiliyor
        }
    }

    private float GetDistanceToClosestMember(Vector3 position, PlayerTeam playerTeam)
    {
        bool firstLoop = true;
        foreach (var player in DummyPlayers)
        {
            if (player == DummyPlayers[0])
            {
                continue;
            }

            //player disabled değil,playerTeam none değil ise, player ile aynı takımdaysa( yani dostsa ) ve ölmemişse
            if (!player.Disabled && player.PlayerTeamValue != PlayerTeam.None && player.PlayerTeamValue == playerTeam && !player.IsDead())
            {
                //takım arkadaşlarının (aynı takım üyelerinin) ışınlanma noktasına olan uzaklığını hesapla
                float playerDistanceToSpawnPoint = Vector3.Distance(position, player.Transform.position);

                if (firstLoop)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                    firstLoop = false;
                }
          
                //bu mesafe _closestDistance dan küçükse _closestDistance a bu mesafeyi ata
                if (playerDistanceToSpawnPoint < _closestDistance)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                }
            }
        }
        //en yakın takım arkadaşının mesafesini dödürüyor
        //_closestDistance döndür
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