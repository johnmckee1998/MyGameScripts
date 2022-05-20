using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheatCodeScript : MonoBehaviour
{
    [Tooltip("Note: despite being strings, each value should only have a single letter, strings are simply used because they are needed for the getkeydown function")]
    public string[] cheatCode;
    private int cheatIndex;

    public bool allowRepeat = true;
    private bool invoked;
    public UnityEvent cheatEvent;

    [Space]
    [Tooltip("Events cant use gameobjects directly, onyl basic variables like string and int, so this slot is for events from the cheatcode script that need a gameobject")]
    public GameObject eventGameObject;
    public bool giveGun;
    // Start is called before the first frame update
    void Start()
    {
        cheatIndex = 0; 
    }

    // Update is called once per frame
    void Update()
    {
        if (!invoked)
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(cheatCode[cheatIndex]))
                {
                    cheatIndex++;
                }
                else
                    cheatIndex = 0;
            }
        }
        if (cheatIndex == cheatCode.Length)
        {
            if (cheatEvent != null)
                cheatEvent.Invoke();
            if(giveGun)
                AddGun();
            cheatIndex = 0; //reset to allow for repeat input
            if(!allowRepeat)
                invoked = true;
        }
    }

    public void AddGun()
    {
        WeaponSelection.instance.AddGun(Instantiate(eventGameObject, WeaponSelection.instance.transform.position, WeaponSelection.instance.transform.rotation));
    }
}
