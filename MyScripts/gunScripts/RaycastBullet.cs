using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastBullet : MonoBehaviour
{
    [Tooltip("Mass in grams")]
    public float mass = 10f;
    [Tooltip("velocity in m/s")]
    public float velocity = 150;
    public float damage = 10f;
    //public float spread = 0f; //should switch this to the vr method - rotate bullet on spawn and then just travel forward, no need to manually add spread each increment
    public float range = 200f;
    [Tooltip("Percentage slowdown per second - e.g drag of 1 would mean bullet would stop after 1 second, 0.1 would take 10 seconds")]
    public float drag = 0.1f;

    //private float startDamage;

    private float startVelocity; //used to reduce damage over distance

    [Tooltip("Mostly arbitrary drop - NOT CUURENTLY USED")]
    public float dropFactor = 1f;
    public LayerMask rayCastIgnore;
    [Tooltip("Used for layers that should collide, but should not do damage")]
    public LayerMask rayCastDamageIgnore;
    private Vector3 startPos;
    private float incrementCount=0; //used to incease drop factor over time

    [Tooltip("if enabled, doesnt destroy on death")]
    public bool debugAlive;
    [Tooltip("Basically weight. 1 means full effect of wind, 2 would half the effect wind has")]
    public float windResist = 1;
    //[Tooltip("Worldspace Windforce vector")]
    private Vector3 windForce;

    [Header("Penetration Settings")]
    [Tooltip("Barrier/Soft target Penetration")]
    public float PenForce = 100f; //if penforce is greater than penresistance then it goes through
    [Range(1f,10f)]
    public float slowFactor = 1f; //Slows down by this factor - can be turned up so bullet can penetrate one barrier easily, but loses alot of power e.g. large bullet like 45-70
    [Tooltip("Armour Penetration - unrealted to barrier penetration")]
    public UniversalStats.armourResistance armourPenetration;
    

    //float xSpread;
   // float ySpread;

    [Header("Special")]
    public GameObject spawnOnDeath;
    public bool noRicochet;
    public bool useExtraCollisionCheck;
    [Space]
    [Tooltip("Used for special death effects")]
    public UniversalStats.DeathType dType;
    //TODO - add more bullet properties that influence ricochet to add realism -e.g. 5.56mm bullet should be more likely than 45 acp

    //private bool finished;
    //public GameObject lineRen;

    // Start is called before the first frame update
    public bool useHitmarker = true;

    [Tooltip("Usually should be true. False is used for special cases like HEAT rounds that shouldnt loose damage")]
    public bool scaleDamageWithVelocity = true;
    private int testRicochetLimit = 5; //used to prevent edge case where a bullet can ricochet almost infintly if it clips into a collider or rebounds weirdly - basically it enlessly ricochets at a 180 degress angle so its trajectory doesnt change and it hits again


    [Header("Pooling")]
    [Tooltip("If true, then it is used by the object pooler, and so should not be destroyed but rather deactivated")]
    public bool isPooled;
    public string poolID;

    private Vector3 rangeTracker;
    private float realDamage;

    Vector3 prevPos; //for debugging

    private Vector3 realVelocity;
    private bool updatedVel;

    //private float tDam;
    private void OnEnable()
    {
        startPos = transform.position;

        //xSpread = Random.Range(-spread, spread);
        //ySpread = Random.Range(-spread, spread);

        startVelocity = velocity;

        if (GlobalStats.instance != null)
            windForce = GlobalStats.instance.windforce;

        //reset these each time the bullet is enabled so that pooled bullets dont end up using pre exsisting values that cause issues
        rangeTracker = Vector3.zero;
        incrementCount = 0;

        realDamage = damage; //real damage is damage that is later scaled by velocity

        //tDam = damage; //test
        prevPos = transform.position;

        realVelocity = transform.forward * velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!updatedVel && false)
        {
            realVelocity = transform.forward * velocity;
            updatedVel = true; //done here coz doing it in start causes it to miss rotations (and thus ignoring spread) - Fixed

            //Way to avoid this - figure out rotation including spread before instantiating, rather than spawning at gun rotation and then rotating
        }
        //tDam *= velocity / startVelocity;
        incrementCount += Time.fixedDeltaTime;
        //Debug.Log("Real: " + realDamage + " Prev: " + tDam + " Ratio: " + velocity/startVelocity);
        if (Vector3.Distance(Vector3.zero, rangeTracker) > range)
        {
            if (debugAlive)
                return;

            if (spawnOnDeath != null)
                Instantiate(spawnOnDeath, transform.position, transform.rotation);
            if (!isPooled)
                Destroy(gameObject);
            else
            {
                gameObject.SetActive(false);
                //Debug.Log("Range : " + Vector3.Distance(startPos, transform.position) + " Tracked: " + Vector3.Distance(Vector3.zero, rangeTracker));
            }
        }
        else
            Shoot();

        if (debugAlive)
        {
            Debug.DrawLine(prevPos, transform.position, Color.green , 30f);
            prevPos = transform.position;
        }
    }

    private void Shoot()//cast ray
    {
        RaycastHit rayHit;
        
        //Vector3 travelVec = transform.forward;
        //travelVec *= (velocity * Time.fixedDeltaTime);//use velocity/fixed time to get distance it should travel in this increment -> Not Needed? since i do this anyway with amx distance in raycast
        

        //travelVec.y -= (9.8f * Time.fixedDeltaTime * incrementCount);//apply drop -> due to the later normalisation, this may not be accurate

        //travelVec += (windForce/windResist) * incrementCount; //apply wind -> due to the later normalisation, this may not be accurate

        realVelocity.y -= (9.8f * Time.fixedDeltaTime); //gravity acceleration
        realVelocity += (windForce / windResist); //wind force

        //Debug.Log("Trav: " + travelVec + " Vel: " + velocity*Time.fixedDeltaTime + " Real: " + realVelocity * Time.fixedDeltaTime);

        if (!useExtraCollisionCheck)
        {
            if (Physics.Raycast(transform.position, realVelocity.normalized, out rayHit, (realVelocity*Time.fixedDeltaTime).magnitude, ~rayCastIgnore))
            {
                CheckHit(rayHit, realVelocity*Time.fixedDeltaTime);
            }
            else
                IncrementBullet(realVelocity * Time.fixedDeltaTime);
        }
        else
            CollisionCheck(realVelocity * Time.fixedDeltaTime);
        
    }


    private void IncrementBullet(Vector3 travel)//method for when ray doesnt hit - keep going
    {
        /*
        //debugging stuff
        if (yeah)
        {
            Debug.DrawRay(transform.position, travel, Color.green, 2f); //draws line from start to start+dir
            //Debug.Log("Yeah " + travelVec);
            yeah = false;
        }
        else 
        {
            Debug.DrawRay(transform.position, travel, Color.red, 2f);
            //Debug.Log("Nah " + travelVec);
            yeah = true;
        }*/

        transform.position += travel;

        rangeTracker += travel;

        //apply drag -> should scale based on magnitude of velocity **************************************************
        //velocity -= velocity * drag * Time.fixedDeltaTime;

        realVelocity -= realVelocity * drag * Time.fixedDeltaTime;


        //if(scaleDamageWithVelocity)
        //    realDamage =damage* (velocity / startVelocity); //reduce damage as velocity drops 
    }

    private void CheckHit(RaycastHit rayHit, Vector3 travelVec) //Check what should happen with this hit 
    {
        if(((1 << rayHit.collider.gameObject.layer) & rayCastDamageIgnore) == 0) //Checks to see if rayHits layer is not part of the ignore mask
                    HitObject(rayHit, travelVec);
        else
        {//damage ignored, destroy
            transform.position = rayHit.point;
            if (!isPooled)
            {
                enabled = false;
                Destroy(gameObject, 1f);
            }
            else
                gameObject.SetActive(false);
        }
        //Note: Not the best way of doing this, as all im doing is simply destroying when hitting a damageignore layer. What i should do is act as normal but not do damage - but still allow for ricochets, hit sounds and effects, and even penetration ########
    }

    private void HitObject(RaycastHit hit, Vector3 trav)//method for when ray hits
    {
        if (scaleDamageWithVelocity)
            realDamage = damage * (realVelocity.magnitude / startVelocity);
        else
            realDamage = damage;
        try
        {
            hit.transform.SendMessage("HitByBullet", realDamage, SendMessageOptions.DontRequireReceiver);
        }
        catch
        {
            Debug.Log("Failed to send Message");
        }

        transform.position = hit.point;
        CharacterControllerScript pscript = hit.transform.GetComponent<CharacterControllerScript>();
        //if (hit.transform == CharacterControllerScript.instance.transform)
        //    pscript = CharacterControllerScript.instance;

        if (pscript != null)
        {
            pscript.DoDamage(realDamage);
        }
        else
        {
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForceAtPosition((mass/1000f)*trav * realVelocity.magnitude * 10f, hit.point); //mass is in g but needs to be in kg, so /1000 -> then did *10f to give it more oomph
            }

            UniversalStats unistat = hit.transform.GetComponent<UniversalStats>();
            HitboxScript hitbox = hit.transform.GetComponent<HitboxScript>();
            if (unistat != null)
            {
                if(unistat.armour <= armourPenetration)
                    unistat.DoDamage(realDamage, useHitmarker, dType);
                unistat.PlayHitEffect(hit.point, Quaternion.LookRotation(hit.normal)); 
                unistat.AddDamage(hit.point, Quaternion.LookRotation(hit.normal));
                

                if (Ricochet(trav, hit.normal, unistat.hardness))
                    return; //dont do anything else if it has ricoched

                if (unistat.PenResistance < PenForce) 
                {//penetrate
                    Penetrate(unistat.PenResistance);
                    IncrementBullet(trav/100f);
                    return;
                }

                

                if (rb != null) //special condidtion for applying force when an aenemy dies
                {
                    if(unistat.health <= 0)
                    {
                        //rb.freezeRotation = false;
                        rb.constraints = RigidbodyConstraints.None;
                        rb.AddForceAtPosition(hit.normal * -1 * realVelocity.magnitude / 2 /* * (damage+PenForce) */ * 0.02f, hit.point);
                    }
                }

                //unistat.PlayHitSound();
            }
            else if(hitbox != null) //hit hitbox  - check before parent unitstats coz all hitboxes will have parent unistat but dont want to reference it, so check this first so then it can skip parented unistats
            {
                if(hitbox.armour <= armourPenetration)
                    hitbox.DoDamage(hit.point, Quaternion.LookRotation(hit.normal), realDamage, useHitmarker, dType);
                if (hitbox.penResistance < PenForce)
                {
                    Penetrate(hitbox.penResistance);
                    IncrementBullet(trav / 100f);
                    return;
                }
            }
            /*else if (hit.transform.GetComponentInParent<UniversalStats>()) //if it hits child collider - removed as this is acheived by hitbox
            {
                unistat = hit.transform.GetComponentInParent<UniversalStats>();

                unistat.DoDamage(damage, useHitmarker);
                unistat.PlayHitEffect(hit.point, transform.rotation);
                unistat.AddDamage(hit.point, transform.rotation);


                if (Ricochet(trav, hit.normal, unistat.hardness))
                    return; //dont do anything else if it has ricoched

                if (unistat.PenResistance < PenForce)
                {//penetrate
                    Penetrate(unistat.PenResistance);
                    IncrementBullet(trav/100f);
                    return;
                }



                if (rb != null) //special condidtion for applying force when an aenemy dies
                {
                    if (unistat.health <= 0)
                    {
                        //rb.freezeRotation = false;
                        rb.constraints = RigidbodyConstraints.None;
                        rb.AddForceAtPosition(hit.normal * -1 * velocity / 2 * 0.02f, hit.point);
                    }
                }
            }*/
            else
            {
                MaterialEffects matVfx = hit.transform.GetComponent<MaterialEffects>();
                
                if (matVfx != null) // if the bullet hits a static material with effect, play the effects
                {
                    matVfx.playHitEffect(hit.point, Quaternion.LookRotation(hit.normal));
                    //matVfx.playHitSound(hit.point); //DISABLED SOUNDS **********************************************
                    matVfx.AddDamage(hit.point, Quaternion.LookRotation(-hit.normal));
                    //matVfx.playHitSound(hit.point);

                    if (Ricochet(trav, hit.normal, matVfx.hardness))
                        return; //dont do anything else if it has ricoched
                    

                    if (matVfx.PenResistance < PenForce)
                    {//penetrate

                        Penetrate(matVfx.PenResistance);
                        IncrementBullet(trav/100f); //increment a very small amount so it clips inside collider - this way it ignores the collider it hits but wont travel too far as to miss neighbouring colliders, could be better
                        return; //return now so it isnt destroyed
                    }
                }
            }
            
        }

        //if (transform.childCount > 0)
        //{
        //    Destroy(transform.GetChild(0).gameObject,1.5f);
        //    transform.GetChild(0).parent = null;//make the trail separate so it isnt destroyed
        //}
        if (spawnOnDeath != null)
            Instantiate(spawnOnDeath, transform.position, Quaternion.LookRotation(hit.transform.forward, hit.transform.up));
        if (!isPooled)
        {
            this.enabled = false;//stop running the script, this for some reason prevents the trail from dissapearing
            Destroy(gameObject, 1.5f);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Penetrate(float resist) 
    {
        realVelocity *= 1f-(resist/PenForce);
        PenForce -= resist * slowFactor;
        //velocity -= velocity / (PenForce/2); 
        //velocity -= resist * slowFactor;//slow down as it penetrates - based on how much force it required 
        
        if (realVelocity.magnitude <= 0) //was velocity <=0, now that velocity is not changed and realvelocity is i have updated it
        {
            if (spawnOnDeath != null)
                Instantiate(spawnOnDeath, transform.position, transform.rotation);
            if (!isPooled)
                Destroy(gameObject); //make sure it doesnt go backwards by destroying if velocity drops too low
            else
                gameObject.SetActive(false);
        }


    }
    private bool Ricochet(Vector3 hitAngle, Vector3 hitNormal, float hardness)
    {

        if (noRicochet)
            return false;

        if (testRicochetLimit <= 0)
            return false;   
        testRicochetLimit--;
        if (hardness <= 0f)
            return false;

        float HAngle =  Vector3.Angle(hitAngle, hitNormal);

        float ricochetChance = ((180/HAngle)-1); //the result of 180/angle -1 is when angle is 180, result is 0, when angle is 90, result is 1, so directly on chance is 0, perpedicular to normal is 100%
        if (ricochetChance < 0.3f)
        {//dont bother if chance is really low
            return false;
        }
        
        ricochetChance *= hardness;
        if (ricochetChance < 0.3f)
        { //again, if once the hardness is applied its still low, just dont ricochet
            return false;
        }

        //Randomised component of ricochet - probably not the most realistic
        //float randomChance = Random.Range(0f, 0.8f); //used to determine if it will ricochet
        //if (ricochetChance < randomChance)
        //    return false;

        //At this point it will ricochet - so do that
        //transform.eulerAngles = Vector3.Reflect(transform.forward, hitNormal);

        //Debug.DrawRay(transform.position, hitNormal, Color.blue, 10f);
        //Debug.DrawRay(transform.position-transform.forward, hitAngle, Color.red, 10f);
        //Debug.DrawRay(transform.position, Vector3.Reflect(transform.forward, hitNormal), Color.green, 10f);

        //velocity *= ricochetChance; 

        realVelocity *= ricochetChance; //reduce velocity proportional to intensity of ricochet - high chance means steep angle, means less slow down, higher angle means more velocity loss

        Vector3 newDir = Vector3.Reflect(hitAngle, hitNormal*-1); //was transform.forward
        transform.rotation = Quaternion.LookRotation(newDir);

        realVelocity = newDir * realVelocity.magnitude; //Update real velocity - maintaining magnitude as the slowdown due to ricochet was already applied
        //Debug.Log("Boing!");
        return true;
    }

    //Not really needed  - fixed issue that this tried to solve
    private void CollisionCheck(Vector3 travelVec)
    {
        RaycastHit rayHit;
        float distance = travelVec.magnitude;
        if (Physics.Raycast(transform.position - (transform.forward * distance), travelVec.normalized, out rayHit, distance * 2f, ~rayCastIgnore)) //do the normal raycast but shift back by 1 distance and do a double length raycast     
        {                                                                                                                                 //- used for slow projectiles (coz they can miss colliders) but may be less accurate due to the different raycast origin and longer raycast
            CheckHit(rayHit, travelVec);
        }
        else
            IncrementBullet(travelVec);
    }

    public void SetNewStats(float vel, float dam)
    {
        startPos = transform.position;
        

        startVelocity = vel;
        velocity = vel;

        if (GlobalStats.instance != null)
            windForce = GlobalStats.instance.windforce;

        //reset these each time the bullet is enabled so that pooled bullets dont end up using pre exsisting values that cause issues
        rangeTracker = Vector3.zero;
        incrementCount = 0;
        damage = dam;
        realDamage = damage;

        realVelocity = transform.forward * velocity;
        updatedVel = false;
		
        testRicochetLimit = 5;//reset this otherwise pooled bullets will eventually stop ricoceting
		
        //tDam = damage;
    }
}
