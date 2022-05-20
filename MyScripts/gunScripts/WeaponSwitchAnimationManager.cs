using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchAnimationManager : MonoBehaviour
{
    private WeaponSelection switchScript;

    private Animator curAnim;

    public GameObject gun;
    //private MeshRenderer gunMesh;
    private GunBase gunScript;

    public AudioClip[] reloadNoises;
    private AudioSource audioS;

    private void Start()
    {
        switchScript = GetComponentInParent<WeaponSelection>();

        curAnim = GetComponent<Animator>();

        //gunMesh = gun.GetComponent<MeshRenderer>();
        gunScript = GetComponentInChildren<GunBase>();
        audioS = GetComponent<AudioSource>();
        
    }

    private void Update()
    {
        //foreach (AudioSource aud in reloadNoises)
        //    aud.pitch = Time.timeScale;
        audioS.pitch = Time.timeScale;
    }

    public void SwitchGunAnim()
    {
        try
        {
            curAnim.SetBool("Switch", true);
        }
        catch
        {
            Debug.Log("Gun anim not set up");
        }
    }

    public void SwitchGun()
    {
        if (switchScript == null)
            switchScript = WeaponSelection.instance;

        switchScript.EquipGun();
        try
        {
            curAnim.SetBool("Switch", false);
        }
        catch
        {
            Debug.Log("Gun anim not set up");
        }
    }

    //public void EquipGun(bool b)
    //{
        //gunMesh.enabled = b;
       // gunScript.enabled = b;
    //}

    public void PlayClick1()
    {
        audioS.PlayOneShot(reloadNoises[0], audioS.volume);
    }

    public void PlayClick2()
    {
        audioS.PlayOneShot(reloadNoises[1], audioS.volume);
    }

    public void PlayClick(int i)
    {
        if (i < reloadNoises.Length)
        {
            try
            {
                audioS.PlayOneShot(reloadNoises[i], audioS.volume);
            }
            catch
            {
                //Idk this keeps happening
            }
        }
    }

    public void ReloadDone()
    {
        
        gunScript.FinishReload();
        
    }

    public void EndCycle()
    {
        gunScript.FinishCycle();
        curAnim.SetBool("Cycle", false);
    }

    public void LoadShot() //used by singlereload to load a single shot
    {
        if (gunScript.CurMag < gunScript.MagSize && gunScript.AmmoPool > 0)
        {
            gunScript.CurMag++;
            gunScript.AmmoPool--;
        }
        if (gunScript.CurMag == gunScript.MagSize || gunScript.AmmoPool <= 0) //mag full
            ReloadDone();
    }

    public void RotateCamPitch(float f)
    {
        CameraMove.instance.ChangePitch(f);
    }

    public void RotateCamYaw(float f)
    {
        CameraMove.instance.ChangeYaw(f);
    }

    public void Shot()
    {
        curAnim.SetBool("Shoot", false);
    }
}
