using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret1Controller : MonoBehaviour
{
    private PlayerController player;
    public GameObject turretHead;
    // Start is called before the first frame update
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        turretHead.transform.LookAt(player.transform);
    }
}
