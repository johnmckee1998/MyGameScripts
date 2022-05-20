using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDHeavyBomber : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Transform bombParent;

    private UniversalStats unistats;
    private double lastShotTime;
    //private int destIndex;
    private Vector3 dest;
    private bool reachedDest;
    private Transform tempDest; //Used to make ship move to another position before moving to firing position - e.g. make it fly up from its spawn point rather than flying directly to destination
    private bool reachedTempDest;
    private Vector3 spawnPos;
    private bool returningToSpawn;


    private Transform[] bombs;
    private int bombIndex; //keeps track of which bomb to fire next
    private Transform target;

    private bool idle;

    //private enum TDArtyState { Walking, Firing, Waiting, Dead };
    //private TDArtyState state;

    void Start()
    {
        unistats = GetComponent<UniversalStats>();

        //destIndex = TowerDefenceWaveManager.instance.GetRandomMissileZone();
        //if (destIndex >= 0) //random missile zones returns -1 when no zone is found
        //    dest = TowerDefenceWaveManager.instance.missileZones[destIndex];

        spawnPos = transform.position;


        //dest = transform.position + transform.forward * 250f + transform.up * 10f;


        bombs = new Transform[ bombParent.childCount];
        for (int i = 0; i < bombs.Length; i++)
            bombs[i] = bombParent.GetChild(i);

        SetNewTargetAndDest();
    }

    private void OnDestroy()
    {
        //if (destIndex >= 0)
        //    TowerDefenceWaveManager.instance.FreeMZ(destIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!idle)
        {
            UpdateState();
            Behaviour();
        }

    }

    private void UpdateState()
    {
        if (unistats.health > 0)//not dead
        {

            if (!reachedDest && dest != null && Vector3.Distance(transform.position, dest) < 0.5f)
                reachedDest = true;

            if (!reachedTempDest && tempDest != null && Vector3.Distance(transform.position, tempDest.position) < 0.5f)
                reachedTempDest = true;

        }
    }

    private void Behaviour()
    {
        if (unistats.health > 0)
        {
            //move to temp dest
            if (tempDest != null && !reachedTempDest)
            {
                transform.position = Vector3.MoveTowards(transform.position, tempDest.position, moveSpeed * Time.fixedDeltaTime);
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, tempDest.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

                transform.rotation = Quaternion.LookRotation(newDirection);
            }
            else if (!reachedDest)
            {
                //dest = target.position;
                //dest.y = transform.position.y;

                transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.fixedDeltaTime);
                
            }
            else //reached dest
            {
                Fire();
                SetNewTargetAndDest();
            }

        }
    }

    private void Fire()
    {
        if (bombIndex < bombs.Length && (lastShotTime + (1f) < Time.timeAsDouble))
        {
            try
            {
                bombs[bombIndex].SendMessage("Drop", target);
            }
            catch
            {

            }
            bombs[bombIndex].parent = null; //unparent it

            lastShotTime = Time.timeAsDouble;


            bombIndex++;
        }
    }

    public void FlyToDest(Transform point) //temp dest -assigned by wave spawner
    {
        tempDest = point;
        dest.y = point.position.y;
    }

    private void ReturnToSpawn()
    {
        if (!returningToSpawn)
        { //reset temp dest bool so i can reuse it
            reachedTempDest = false;
            returningToSpawn = true;
        }

        if (tempDest != null && !reachedTempDest)
        {
            transform.position = Vector3.MoveTowards(transform.position, tempDest.position, moveSpeed * Time.fixedDeltaTime);
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, (transform.position - tempDest.position).normalized, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            //move to dest
            transform.position = Vector3.MoveTowards(transform.position, spawnPos, moveSpeed * Time.fixedDeltaTime);
            //dont need to rotate
            //Vector3 newDirection = Vector3.RotateTowards(transform.forward, spawnPos.forward, 360f * Mathf.Deg2Rad * Time.deltaTime, 0.0f);

            //transform.rotation = Quaternion.LookRotation(newDirection);
        }

        if (Vector3.Distance(transform.position, spawnPos) < 0.5f) //reached spawn - destory
            Destroy(gameObject);

        if (!reachedTempDest && tempDest != null && Vector3.Distance(transform.position, tempDest.position) < 0.5f) //reach temp dest - go back to spawn
            reachedTempDest = true;

    }


    private void SetNewTargetAndDest()
    {
        //get a random target - player/bot, if neither alive then bomb base
        if (TowerDefenceWaveManager.instance.friendlyAIParent.childCount > 0) //bots alive
        {
            int playerOrBot = Random.Range(0, TowerDefenceWaveManager.instance.friendlyAIParent.childCount+1); //used to choose whether or not to target player or bots

            if(CharacterControllerScript.instance.health<=0 || playerOrBot > 0) //player dead or playerOrBot chose bot
            {
                int randomBotTargetIndex = Random.Range(0, TowerDefenceWaveManager.instance.friendlyAIParent.childCount);
                Transform randTarget = TowerDefenceWaveManager.instance.friendlyAIParent.GetChild(randomBotTargetIndex);

                target = randTarget;
                dest = randTarget.position;
                dest.y = transform.position.y;
                
            }
            else //target player
            {
                target = CharacterControllerScript.instance.transform;
                dest = target.position;
                dest.y = transform.position.y;
            }

        }
        else if(CharacterControllerScript.instance.health>0) //player alive
        {
            //target player
            target = CharacterControllerScript.instance.transform;
            dest = target.position;
            dest.y = transform.position.y;
        }
        else //neither alive
        {
            //target base
            target = TDPlayerBase.instance.buildings[ TDPlayerBase.instance.GetClosestBase(transform.position)].target;
            dest = target.position;
            dest.y = transform.position.y;
        }
        reachedDest = false;

        StartCoroutine(WaitAfterBomb());
    }

    IEnumerator WaitAfterBomb()
    {
        idle = true;
        yield return new WaitForSeconds(2f);
        idle = false;
    }

}
