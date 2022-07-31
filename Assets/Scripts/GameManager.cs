using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform initialSpawnZone;
    [SerializeField] private Transform spawnZone;
    [SerializeField] private GameObject hold;
    [SerializeField] private float spawnInterval;
    [SerializeField] private float despawnTime;
    [SerializeField] private float spawnWidthDampener;
    [SerializeField] private float spawnHeightDampener;
    [SerializeField] private int initialSpawnCount;
    [SerializeField] private float _vicinityUnits;
    [SerializeField] private float _initialHoldsSpeed;
    [SerializeField] private float _holdsSpeedAcceleration;
    
    [SerializeField] private Hold lhInitialHold;
    [SerializeField] private Hold rhInitialHold;
    [SerializeField] private Hold lfInitialHold;
    [SerializeField] private Hold rfInitialHold;

    public float failureHeight;

    private float _currentSpawnTime;
    private List<Hold> _spawnedHolds = new ();
    
    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (lhInitialHold != null) SpawnDefaultHold(lhInitialHold);
        if (rhInitialHold != null) SpawnDefaultHold(rhInitialHold);
        if (lfInitialHold != null) SpawnDefaultHold(lfInitialHold);
        if (rfInitialHold != null) SpawnDefaultHold(rfInitialHold);
        
        for (int i = 0; i < initialSpawnCount; i++) SpawnHold(initialSpawnZone);
        _currentSpawnTime = spawnInterval;
    }

    private void Update()
    {
        if (_currentSpawnTime <= 0)
        {
            if (spawnZone) SpawnHold(spawnZone);
            _currentSpawnTime = spawnInterval;
        }

        _currentSpawnTime -= Time.deltaTime;

        AccelerateHolds(_holdsSpeedAcceleration);
    }

    private void SpawnDefaultHold(Hold hold)
    {
        hold.GetComponent<Renderer>().material.color = Utils.boulderColors[Random.Range(0, Utils.boulderColors.Count)];
        hold.SetSpeed(_initialHoldsSpeed);
        _spawnedHolds.Add(hold);
    }

    private void SpawnHold(Transform zone)
    {
        Vector3 hold_position = Utils.GetRandomPositionInZone(zone, spawnWidthDampener, spawnHeightDampener);

        while (_spawnedHolds.Any(spawned_hold => Utils.PositionsOverlap(
                   Utils.Vec3ToVec2Int(spawned_hold.gameObject.transform.localPosition), 
                   Utils.Vec3ToVec2Int(hold_position), _vicinityUnits
        )))
        {
            hold_position = Utils.GetRandomPositionInZone(zone, spawnWidthDampener, spawnHeightDampener);
        }

        Hold hold_copy = Instantiate(hold, hold_position, hold.transform.rotation).GetComponent<Hold>();
        hold_copy.name += _spawnedHolds.Count;
        hold_copy.SetSpeed(_initialHoldsSpeed);
        _spawnedHolds.Add(hold_copy);
        
        hold_copy.GetComponent<Renderer>().material.color = Utils.boulderColors[Random.Range(0, Utils.boulderColors.Count)];
        
        if (despawnTime > 0) StartCoroutine(DestroyHold(hold_copy, despawnTime));
    }

    private IEnumerator DestroyHold(Hold hold, float time)
    {
        yield return new WaitForSeconds(time);

        _spawnedHolds.Remove(hold);
        Destroy(hold.gameObject);
    }

    private void AccelerateHolds(float acceleration)
    {
        foreach (Hold hold in _spawnedHolds)
        {
            hold.Accelerate(acceleration);
        }

        _initialHoldsSpeed += acceleration;
        despawnTime -= acceleration * 10;
    }
    
    private void OnDrawGizmos()
    {
        if (initialSpawnZone)
        {
            Vector3 scale = new Vector3(initialSpawnZone.localScale.x - 2 * spawnWidthDampener,
                initialSpawnZone.localScale.y / 2 - 2 * spawnHeightDampener, 2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(initialSpawnZone.position, scale);
        }
        
        if (spawnZone)
        {
            Vector3 scale = new Vector3(spawnZone.localScale.x - 2 * spawnWidthDampener,
                spawnZone.localScale.y / 2 - 2 * spawnHeightDampener, 2f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnZone.position, scale);
        }

        Gizmos.color = Color.black;
        Gizmos.DrawLine(new Vector3(-100f, failureHeight, -.5f), new Vector3(100f, failureHeight, -.5f));
    }
}
