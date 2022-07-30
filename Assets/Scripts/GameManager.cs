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

    private float _currentSpawnTime;
    private List<Hold> _spawnedHolds = new ();
    
    public static GameManager instance { get; private set; }

    private void Start()
    {
        if (!instance)
        {
            instance = this;
        }
        
        for (int i = 0; i < initialSpawnCount; i++) SpawnHold(initialSpawnZone);
        _currentSpawnTime = spawnInterval;
    }

    private void Update()
    {
        if (_currentSpawnTime <= 0)
        {
            SpawnHold(spawnZone);
            _currentSpawnTime = spawnInterval;
        }

        _currentSpawnTime -= Time.deltaTime;
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
        _spawnedHolds.Add(hold_copy);
        
        hold_copy.GetComponent<Renderer>().material.color = Utils.boulderColors[Random.Range(0, Utils.boulderColors.Count)];
        
        StartCoroutine(DestroyHold(hold_copy, despawnTime));
    }

    private IEnumerator DestroyHold(Hold hold, float time)
    {
        yield return new WaitForSeconds(time);

        _spawnedHolds.Remove(hold);
        Destroy(hold.gameObject);
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
    }
}
