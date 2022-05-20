using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiUserGunScript : MonoBehaviour
{
    public EmplacedGunScript playerGun;
    public PlayerAutoTurret botTurret;
    [Tooltip("Used for special turret types (other than PlayerAutoTurret")]
    public MonoBehaviour otherBotTurret;
    [Tooltip("Used for special turret types (other than EmplacedGunScript")]
    public MonoBehaviour otherPlayerTurret;
    public GameObject placementPreview;
    private UniversalStats botStats;
    private Transform bot;
    //add varible of bot that is using gun
    public Transform botPosition;
    private BotShooting botGun;

    private bool botUsingGun;


    private int prevPlayerAmmo;
    private int prevBotAmmo;

    private int prevPlayerMag;
    private int prevBotMag;


    // Start is called before the first frame update
    void Start()
    {
        AICoverpointManager.instance.AddTurret(this);

        if (botTurret != null)
        {
            botGun = botTurret.turretGunScript;

            botTurret.gameObject.SetActive(false);
            botGun.gameObject.SetActive(false);

            //aync up ammo pools - use the larger
            if (botGun.ammoPool > playerGun.AmmoPool)
                playerGun.AmmoPool = botGun.ammoPool;
            else
                botGun.ammoPool = playerGun.AmmoPool;

            prevPlayerAmmo = playerGun.AmmoPool;
            prevBotAmmo = botGun.ammoPool;
        }
        else if (otherBotTurret != null)
            otherBotTurret.enabled = false;

        if (playerGun != null)
            playerGun.enabled = true;
        else if (otherPlayerTurret != null)
            otherPlayerTurret.enabled = true;

        //prevPlayerMag = playerGun.MagSize;
        //prevBotMag = botGun.MagSize;
    }

    private void OnDestroy()
    {
        //BotLeaveGun();
        if (bot != null)
            try
            {
                bot.SendMessage("LeaveTurret", SendMessageOptions.DontRequireReceiver);
            }
            catch
            {

            }
        AICoverpointManager.instance.RemoveTurret(this);
    }

    private void Update()
    {   //this isnt working perfectly but i think i know why - the start method is not called on bot shooting, the the difference between its start ammo pool and real ammo pool isnt calculated correctly as start ammo pool isnt assigned untill a crewman is assigned

        if (botTurret != null)
        {
            if (prevPlayerAmmo > playerGun.AmmoPool) //player has shot ammo
                botGun.ammoPool = playerGun.AmmoPool;
            else if (prevBotAmmo > botGun.ammoPool) //bot has shot ammo
                playerGun.AmmoPool = botGun.ammoPool;
            else if (prevBotAmmo < botGun.ammoPool)//bot ammo has gone up - probably refilled
                playerGun.AmmoPool = botGun.ammoPool;
            else if (prevPlayerAmmo < playerGun.AmmoPool) //player ammo has gone up - probabl refilled 
                botGun.ammoPool = playerGun.AmmoPool;


            prevPlayerAmmo = playerGun.AmmoPool;
            prevBotAmmo = botGun.ammoPool;
        }
        //prevPlayerMag = playerGun.GetMag();
        //bot doesnt have a getmag function
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        

        //botUsingGun = (bot != null || (botStats!=null&&botStats.health>0)); //if bot is null or dead then it is no longer using gun -> THIS SHOULD RUN LEAVEGUN
        //playerGun.blockUse = botUsingGun;
        //if(botUsingGun && bot != null && botStats != null && botStats.health > 0)
        //{
            //bot has died - but still marked as using gun
        //}
    }

    public void BotUseGun(Transform b, UniversalStats uS = null)
    {
        bot = b;
        if (uS != null)
            botStats = uS;
        else
            botStats = bot.GetComponent<UniversalStats>();

        botUsingGun = true;
        if (playerGun != null)
        {
            playerGun.blockUse = true;
            playerGun.enabled = false;
        }
        else if (otherPlayerTurret != null)
            otherPlayerTurret.enabled = false;

        if (botTurret != null)
        {
            botTurret.gameObject.SetActive(true);
            botGun.gameObject.SetActive(true);
        }
        else if(otherBotTurret!=null)
            otherBotTurret.enabled = true;

        

        AICoverpointManager.instance.UseTurret(this);
    }

    public void BotLeaveGun(bool andTurret = true)
    {
        /*
        if (andTurret) //if false dont leave turret - this is because it will be run by something else
        {
            try
            {
                bot.SendMessage("LeaveTurret", SendMessageOptions.DontRequireReceiver);
            }
            catch
            {

            }
        }
        */
        bot = null;
        botStats = null;
        botUsingGun = false;
        if (playerGun != null)
        {
            playerGun.blockUse = false;
            playerGun.enabled = true;
        }
        else if (otherPlayerTurret != null)
            otherPlayerTurret.enabled = true;

        if (botTurret != null)
        {
            botTurret.gameObject.SetActive(false);
            botGun.gameObject.SetActive(false);
        }
        else if (otherBotTurret != null)
            otherBotTurret.enabled = false;
        

        AICoverpointManager.instance.LeaveTurret(this);
    }
}
