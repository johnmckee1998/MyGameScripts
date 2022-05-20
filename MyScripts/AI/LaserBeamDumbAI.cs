using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class LaserBeamDumbAI : DumbAi
{

    [Header("LaserStuff")]
    public float shootFromDistance; //could just use hitbuffer but this is clearer
    public float laserDPS;
    public float laserRange;
    public float laserChargeUpTime;
    public float laserduration;
    public VisualEffect chargeEffect;
    public VisualEffect shotEffect;
    public AudioSource chargeSound;
    public AudioSource shotSound;
    public AudioSource hitPlayerSound;
    [Space]
    public Transform laserOrigin;
    private bool charging;
    private bool firing;

    [Space]
    public LayerMask playerMask;


    private Vector3 targetPos;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navRigid = GetComponent<Rigidbody>();
        alive = true;
        StartCoroutine(Chase());
        navAgent.speed = MoveSpeed;
        //navAgent.angularSpeed = 360;
        

        if (GetComponent<UniversalStats>() != false)
        {
            //usesUniStats = true;
            uniStats = GetComponent<UniversalStats>();
        }

        chargeEffect.Stop();
        shotEffect.Stop();

    }

    // Update is called once per frame
    void Update()
    {
        health = uniStats.health;
        if (health <= 0)
        {
            alive = false;
            Destroy(chargeEffect.gameObject);
            Destroy(shotEffect.gameObject);
            OnDeath();
        }
    }

    private void FixedUpdate()
    {
        if (firing)
        {
            LaserCast();
        }
    }

    protected override IEnumerator Chase()
    {
        yield return null;
        while (alive)
        {
            if (navAgent.enabled)
            {
                if (hitPlayer || (Vector3.Distance(transform.position, CharacterControllerScript.instance.transform.position) <= shootFromDistance && CheckSight()) || charging || firing) //if attacked, wait for cooldown, OR if within shooting distance -> must also not be charging or firing
                    navAgent.destination = transform.position;
                else
                    navAgent.destination = CharacterControllerScript.instance.transform.position;

                
                //yield return new WaitForSeconds(0.05f);

                if (!hitPlayer && Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) <= shootFromDistance && !charging && !firing && CheckSight())
                    StartCoroutine(LaserAttack());


                //while charging look at player, while firing slowly turn to player
                if (!firing && charging)
                    transform.LookAt(CharacterControllerScript.instance.transform.position);
                else if (firing && false)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation((CharacterControllerScript.instance.transform.position + Vector3.up) - transform.position), (8f * Time.deltaTime));
                    //also move targetpos
                }

            }
            else
                Debug.Log("Nav Disabled: " + gameObject.name);
            yield return null;
        }
    }

    IEnumerator LaserAttack()
    {
        //play charge effect
        hitPlayer = true;
        chargeEffect.Play();
        chargeSound.Play();
        charging = true;
        yield return new WaitForSeconds(laserChargeUpTime);
        chargeSound.Stop();
        chargeEffect.Stop();
        shotSound.Play();
        shotEffect.Play();
        charging = false;
        firing = true;
        if (CharacterControllerScript.instance.getCrouch())
            targetPos = CharacterControllerScript.instance.transform.position + (Vector3.up * 0.5f);
        else
            targetPos = CharacterControllerScript.instance.transform.position + Vector3.up;
        shotEffect.transform.LookAt(CharacterControllerScript.instance.transform.position);
        yield return new WaitForSeconds(laserduration);
        firing = false;
        shotEffect.Stop();
        yield return new WaitForSeconds(hitTime);
        hitPlayer = false;
    }

    private void LaserCast()
    {
        Vector3 laserEnd;
        float length;

        RaycastHit rayHit;
        if (Physics.Raycast(laserOrigin.position, targetPos - laserOrigin.position, out rayHit, laserRange))
        {
            Debug.Log("Hit " + rayHit.transform.name);
            if (rayHit.collider.gameObject.layer == playerMask) //hitplayer
            {
                CharacterControllerScript.instance.health -= laserDPS * Time.fixedDeltaTime;

                if (hitPlayerSound != null)
                    hitPlayerSound.Play();
            }
            else if (rayHit.collider.gameObject.tag == "Player")
            {
                CharacterControllerScript.instance.health -= laserDPS * Time.fixedDeltaTime;

                if (hitPlayerSound != null)
                    hitPlayerSound.Play();
            }

            laserEnd = rayHit.point;

            length = Vector3.Distance(laserOrigin.position, rayHit.point);
        }
        else
        { // since it didint hit anything, calculate how far the laser should go
            laserEnd = laserOrigin.position + (laserOrigin.forward * laserRange);

            length = laserRange;
        }


        try
        {
            shotEffect.SetFloat("Length", length);
        }
        catch
        {
            Debug.Log("Failed to set length");
        }

        //set laser to use laser length
        //laserLength = laserRange;
    }

    private bool CheckSight()
    {
        RaycastHit sightRay;
        Vector3 lookpos;
        if (CharacterControllerScript.instance.getCrouch())
            lookpos = CharacterControllerScript.instance.transform.position + (Vector3.up * 0.5f);
        else
            lookpos = CharacterControllerScript.instance.transform.position + Vector3.up;
        if (Physics.Raycast(transform.position, lookpos - transform.position, out sightRay, laserRange))
        {
            if (sightRay.collider.gameObject.tag == "Player")
                return true;
            else
                return false;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetPos != null && CharacterControllerScript.instance !=null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPos, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(CharacterControllerScript.instance.transform.position, 0.5f);
            Gizmos.color = Color.red;
            Ray r = new Ray(laserOrigin.position, (targetPos - laserOrigin.position)*laserRange);
            Gizmos.DrawRay(r);
        }
    }


    protected override void OnDeath()
    {
        Destroy(chargeEffect.gameObject);
        Destroy(shotEffect.gameObject);
        base.OnDeath();
    }
}
