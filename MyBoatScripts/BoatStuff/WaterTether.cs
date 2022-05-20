using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTether : MonoBehaviour
{
    public Transform player;
    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
            player = CharacterControllerScript.instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
    }
}
