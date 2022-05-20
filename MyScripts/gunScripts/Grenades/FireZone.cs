using UnityEngine;

public class FireZone : MonoBehaviour
{
    public float dps = 10f;
    public bool damagePlayer = true;
    public float timeAlive = 5f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timeAlive > 0)
            timeAlive -= Time.deltaTime;
        else
            Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Equals("Player"))
            CharacterControllerScript.instance.health -= dps * Time.deltaTime;

        UniversalStats unistats = other.GetComponent<UniversalStats>();
        if (unistats !=null)
        {
            unistats.health -= (dps * Time.fixedDeltaTime); //special case where dodamage isnt used - otherwise a fuckton of hitmarks are used
            unistats.SetOnFire();
        }
    }
}
