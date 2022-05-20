using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    [Header("FootStep Sounds")]
    public AudioClip[] concreteSounds; //Could make arrays 2D - [0,x] is walk sounds, [1,x] is run sounds, take int or bool as second input to choose which one - CANT - inspector doesnt like 2d arrays
    public AudioClip[] concreteRunSounds;
    public AudioClip[] metalSounds;
    public AudioClip[] metalRunSounds;
    public AudioClip[] woodSounds;
    public AudioClip[] woodRunSounds;
    public AudioClip[] dirtSounds;
    public AudioClip[] dirtRunSounds;
    public AudioClip[] grassSounds;
    public AudioClip[] grassRunSounds;
    public AudioClip[] mudSounds;
    public AudioClip[] mudRunSounds;
    public AudioClip[] sandSounds;
    public AudioClip[] sandRunSounds;

    [Header("TooBeUsed")]
    public AudioClip[] stoneSounds;
    public AudioClip[] snowSounds;
    //more


    private CharacterControllerScript player;

    private int prevIndex = 100;
    private MaterialEffects.Sound walkSoundType;
    private float walkSoundTimer;
    private float playerVelocity;
    private Vector3 prevPlayerPos;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        StartCoroutine(UpdatePlayerSoundType());
    }

    private void Update()
    {
        if (player == null)
            player = CharacterControllerScript.instance;

        if(CharacterControllerScript.Active)
            UpdatePlayerFootstepSound();
    }

    private void FixedUpdate()
    {
        //UpdatePlayerSoundType();
    }

    public AudioClip GetSound(MaterialEffects.Sound s, bool walk = false)
    {
        
        switch (s)
        {
            //Can set up cases like this: (two cases with a condition)
            case MaterialEffects.Sound.concrete when walk:
                    return PickRandSound(concreteSounds);

            case MaterialEffects.Sound.concrete:
                    return PickRandSound(concreteRunSounds);

            //Or set them up like this: (one case with and if else)
            case MaterialEffects.Sound.metal:
                    if (walk)
                        return PickRandSound(metalSounds);
                    else
                        return PickRandSound(metalRunSounds);

            case MaterialEffects.Sound.wood:
                    if (walk)
                        return PickRandSound(woodSounds);
                    else
                        return PickRandSound(woodRunSounds);

            case MaterialEffects.Sound.dirt:
                    if (walk)
                        return PickRandSound(dirtSounds);
                    else
                        return PickRandSound(dirtRunSounds);

            case MaterialEffects.Sound.grass:
                    if (walk)
                        return PickRandSound(grassSounds);
                    else
                        return PickRandSound(grassRunSounds);

            case MaterialEffects.Sound.mud:
                    if (walk)
                        return PickRandSound(mudSounds);
                    else
                        return PickRandSound(mudRunSounds);

            case MaterialEffects.Sound.sand:
                    if (walk)
                        return PickRandSound(sandSounds);
                    else
                        return PickRandSound(sandRunSounds);

            default:
                return PickRandSound(null);
        }



    }

    private AudioClip PickRandSound(AudioClip[] aud)
    {
        if (aud == null)
            return null;
        if (aud.Length > 0)
        {
            int rand = Random.Range(0, aud.Length);
            int count = 0;
            while (rand == prevIndex && count<10)//loop up to 10 times to try find another index
            {
                rand = Random.Range(0, aud.Length);
                count++;
            }
            prevIndex = rand;
            return aud[rand];
            
        }
        else
            return null;
    }

    private IEnumerator UpdatePlayerSoundType()
    {
        yield return null;
        prevPlayerPos = player.transform.position;
        while (true)
        {
            RaycastHit rayHit;
            //Debug.DrawRay(pCam.transform.position, pCam.transform.forward*10, Color.white, 5.0f, false);
            if (Physics.Raycast(player.transform.position, -player.transform.up, out rayHit, 2f))
                if (rayHit.transform.gameObject.GetComponent<MaterialEffects>())
                    walkSoundType = rayHit.transform.gameObject.GetComponent<MaterialEffects>().typeOfMaterial;
            

            playerVelocity = Vector3.Distance(player.transform.position, prevPlayerPos) / (Time.fixedDeltaTime);
            prevPlayerPos = player.transform.position;

            yield return new WaitForSeconds(Time.fixedDeltaTime);//slower velocity updates 
        }
    }

    private void UpdatePlayerFootstepSound()
    {
        if (CharacterControllerScript.characterController.isGrounded)
        {
            if (playerVelocity>0.1 && walkSoundTimer <= 0)
            {



                if (walkSoundType != MaterialEffects.Sound.none)
                {
                    player.walkSound.clip = GetSound(walkSoundType, !CharacterControllerScript.isSprinting);

                    player.walkSound.PlayOneShot(player.walkSound.clip, player.walkSound.volume);

                    //if (playerVelocity > 0.1) //player is moving
                    walkSoundTimer = (3 / player.speed)/(playerVelocity/ player.speed);
                    walkSoundTimer = Mathf.Clamp(walkSoundTimer, 0f, 1f); //clamp max time to prevent weirdness
                    // scale speed timing based off of player velocity (using player velocity directly causes issues -> when accelerating you can have values like 5 seconds set because it would be 2/0.4)
                }
                else
                    Debug.Log("No Sound");
                //Play sound

            }
            


        }
        if (walkSoundTimer > 0)
            walkSoundTimer -= Time.deltaTime;
        
    }
}
