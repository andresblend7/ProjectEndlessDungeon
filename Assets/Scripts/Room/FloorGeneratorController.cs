using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloorGeneratorController : MonoBehaviour
{
    public FloorData[] floors;
    public int maxLengthFloor = 31;
    public Transform mainCameraPosition;
    public int offsetSpawn = 0;
    public int maxFloorsOnScreen = 2;


    [Header("BlockGenerator")]
    public BlockGenerator blockGenerator;

    private int currentCountFloor = 1;
    private int spawnDistance = 0;
    private int lastSpawnPoint = 0;
    private List<FloorData> floorInstances;
    // Start is called before the first frame update
    void Start()
    {
        spawnDistance = maxLengthFloor - 10;
        floorInstances = new List<FloorData>();
        lastSpawnPoint = offsetSpawn * currentCountFloor;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCameraPosition != null)
        {

            if(mainCameraPosition.position.z >= (lastSpawnPoint- spawnDistance))
            {
                var newSpawnPoint = (lastSpawnPoint + maxLengthFloor);
                var floorInstantiate = Instantiate(floors[Random.Range(0, floors.Length)], new Vector3(0, -1, newSpawnPoint), Quaternion.identity);
                //Generar blockes
                var blockSpawnPosition = (lastSpawnPoint - offsetSpawn)+1;
                blockGenerator.Generate(blockSpawnPosition);
                //Debug.Log("Spawn Floor" + mainCameraPosition.position+ "| newSpawnPoint:"+ newSpawnPoint+ "| lastSpawnPoint: "+ lastSpawnPoint+" |blockSpawnPosition: "+ blockSpawnPosition);
                floorInstances.Add(floorInstantiate);
                currentCountFloor++;
                lastSpawnPoint = newSpawnPoint;


                if (floorInstances.Count > maxFloorsOnScreen)
                {
                    Destroy(floorInstances[0].gameObject);
                    floorInstances.RemoveAt(0);
                }
            }
          
           
        }


    }
}
