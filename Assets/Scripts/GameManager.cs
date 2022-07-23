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
    [SerializeField] private float spawnWidthDampener;
    [SerializeField] private float spawnHeightDampener;
    [SerializeField] private int initialSpawnCount;
    [SerializeField] private float _vicinityUnits;

    private float _currentSpawnTime;
    private List<Hold> _spawnedHolds;

    private void Start()
    {
        _currentSpawnTime = spawnInterval;
        _spawnedHolds = new List<Hold>();
        for (int i = 0; i < initialSpawnCount; i++) SpawnHold(initialSpawnZone.localScale);
    }

    private void Update()
    {
        if (_currentSpawnTime <= 0)
        {
            SpawnHold(spawnZone.localScale);
            _currentSpawnTime = spawnInterval;
        }

        _currentSpawnTime -= Time.deltaTime;
    }

    private void SpawnHold(Vector3 zone)
    {
        Vector3 hold_position =new Vector3(
            Random.Range(-zone.x / 2 + spawnWidthDampener, zone.x / 2 - spawnWidthDampener),
            Random.Range(-zone.y / 4 + spawnHeightDampener, zone.y / 4 - spawnHeightDampener),
            0.25f
        );

        while (_spawnedHolds.Any(spawned_hold => 
            PositionsOverlap(Vec3ToVec2Int(spawned_hold.gameObject.transform.localPosition), Vec3ToVec2Int(hold_position), _vicinityUnits))
        )
        {
            hold_position =new Vector3(
                Random.Range(-zone.x / 2 + spawnWidthDampener, zone.x / 2 - spawnWidthDampener),
                Random.Range(-zone.y / 4 + spawnHeightDampener, zone.y / 4 - spawnHeightDampener),
                0.25f
            );
        }

        Hold hold_copy = Instantiate(hold, hold_position, hold.transform.rotation).GetComponent<Hold>();
        hold_copy.name += _spawnedHolds.Count;
        _spawnedHolds.Add(hold_copy);
    }

    private bool PositionsOverlap(Vector2Int p1, Vector2Int p2, float vicinity)
    {
        return p1.x - vicinity <= p2.x && p2.x <= p1.x + vicinity &&
               p1.y - vicinity <= p2.y && p2.y <= p1.y + vicinity;
    }

    private Vector2Int Vec3ToVec2Int(Vector3 v)
    {
        Vector3Int v_int = Vector3Int.FloorToInt(v);
        return new Vector2Int(v_int.x, v_int.y);
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
