using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ArtilleryBombardment : MonoBehaviour
{
    public static ArtilleryBombardment instance;

    public GameObject shell;
    //Too be used - pooling
    //[Tooltip("The ID used by object pooler")]
    //public string shellID;

    public Transform shellSpawnPoint;
    public int bombardmentAmount = 10;
    public float bombardmentRatePerSec;
    public float bombardRadius;
    public float bombardDelay = 2f;
    [Space]
    public VisualEffect[] vfx;
    public AudioSource[] sfx;

    private void Start()
    {
        instance = this;
    }

    public void FireBombardment()
    {
        StartCoroutine(Bombard());
    }

    IEnumerator Bombard()
    {
        //Play firing effects
        for (int i = 0; i < vfx.Length; i++)
        { 
            vfx[i].Play();
            if (i < sfx.Length) 
                sfx[i].Play();
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(bombardDelay);
        //Spawn Shells
        for(int i =0; i < bombardmentAmount; i++)
        {
            //calculate offset
            Vector3 offset = new Vector3(Random.Range(-bombardRadius, bombardRadius),0f, Random.Range(-bombardRadius, bombardRadius));

            //Spawn shell
            GameObject g = Instantiate(shell, shellSpawnPoint.position + offset, shellSpawnPoint.rotation);

            yield return new WaitForSeconds(1f/bombardmentRatePerSec);
        }
    }
}
