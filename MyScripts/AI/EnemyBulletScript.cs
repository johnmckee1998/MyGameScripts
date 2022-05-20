using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    private Vector3 startLoc;
    private Vector3 curLoc;

    public float damage;
    public float range;

    private GameObject blood;
    private GameObject other;

    private Vector3 travelDir = Vector3.zero;

    public float timeToDie = 2f;
    // Start is called before the first frame update
    void Start()
    {
        startLoc = transform.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = travelDir;
        blood = transform.GetChild(0).gameObject;
        other = transform.GetChild(1).gameObject;
        Destroy(this, timeToDie);
    }

    void Update()
    {
        curLoc = transform.position;
        if (Vector3.Distance(startLoc, curLoc) > range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        UniversalStats uniStats = collision.gameObject.GetComponent<UniversalStats>();

        if (uniStats != null) //Damage objects/enemies with universal stats
        {
            uniStats.DoDamage(damage);
            uniStats.PlayHitSound();

            Rigidbody uniRb = collision.gameObject.GetComponent<Rigidbody>();

            if (uniRb != null)
                uniRb.AddForce(-80 * collision.transform.forward);
        }

        else
        {
            if (collision.gameObject.GetComponent<CharacterControllerScript>()!=null)
            {
                GameObject b = Instantiate(blood, transform.position, transform.rotation);
                collision.gameObject.GetComponent<CharacterControllerScript>().health -= damage;
                //Debug.Log("Hit!");
            }
        }
        Destroy(gameObject);
    }

    public void SetTravelDir(Vector3 dir)
    {
        travelDir = dir;
    }
}
