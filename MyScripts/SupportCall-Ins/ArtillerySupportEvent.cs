using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtillerySupportEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ArtilleryBombardment.instance.shellSpawnPoint.position = new Vector3(transform.position.x, ArtilleryBombardment.instance.shellSpawnPoint.position.y, transform.position.z);
        StartCoroutine(Fire());
    }

    IEnumerator Fire()
    {
        yield return new WaitForSeconds(0.5f);
        ArtilleryBombardment.instance.FireBombardment();
        yield return null;
        Destroy(gameObject);
    }
}
