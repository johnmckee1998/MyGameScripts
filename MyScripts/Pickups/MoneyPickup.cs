using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    private AudioSource pickupSound;
    public int amount = 1000;
    // Start is called before the first frame update
    void Start()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && transform.GetComponent<MeshRenderer>().enabled)
        {
            if (pickupSound != null)
                pickupSound.Play();

            PlayerMoney.Money += amount;

            transform.GetComponent<MeshRenderer>().enabled = false;

            if (transform.childCount > 0)
                foreach (Transform chld in transform)
                    chld.gameObject.SetActive(false);

            if (transform.parent != null)
                Destroy(transform.parent.gameObject, 3f);
            else
                Destroy(gameObject, 3f);
        }
    }
}
