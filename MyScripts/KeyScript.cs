using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    private AudioSource pickup;
    public GameObject light;

    private void Start()
    {
        pickup = GetComponent<AudioSource>();
        
    }

    private void Update()
    {
        light.transform.RotateAround(transform.position, Vector3.up, 30 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && transform.GetComponent<MeshRenderer>().enabled)
        {
            other.GetComponent<KeyCounter>().addKey(1);
            //Destroy(gameObject);
            transform.GetComponent<MeshRenderer>().enabled = false;
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
                //child.GetComponent<MeshRenderer>().enabled = false;

            pickup.PlayOneShot(pickup.clip, 1);
        }
    }
}
