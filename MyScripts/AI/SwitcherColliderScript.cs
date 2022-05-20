using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherColliderScript : MonoBehaviour
{
    public Transform player;
    public GameObject viewBlock;
    public float offset = 200f;
    public static bool isBlocked = false;
    public static float currOffset = 200f;
    public LayerMask layermask;

    // Start is called before the first frame update
    void Start()
    {
        isBlocked = false;
        currOffset = offset;
    }

    // Update is called once per frame
    void Update()
    {
        if(!SwitcherAI.switched)
            transform.position = new Vector3(player.position.x, player.position.y + 0.9f, player.position.z + offset);
        else
            transform.position = new Vector3(player.position.x, player.position.y + 0.9f, player.position.z - offset);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("BUMP Enter");
        if (!other.gameObject.layer.Equals(21)) //ignore the sensor layer
        {
            isBlocked = true;
            viewBlock.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("BUMP Exit");
        if (!other.gameObject.layer.Equals(21))
        {
            isBlocked = false;
            viewBlock.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.layer.Equals(21))
        {
            isBlocked = true;
            viewBlock.SetActive(true);
        }
    }
}
