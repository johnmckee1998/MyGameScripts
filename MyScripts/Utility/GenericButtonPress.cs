using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericButtonPress : MonoBehaviour
{
    public bool useButton;

    public KeyCode key;

    public string buttonID;

    public UnityEvent OnDown;
    public UnityEvent OnUp;

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            if (useButton)
            {
                if (Input.GetButtonDown(buttonID))
                    OnDown?.Invoke();
                else if (Input.GetButtonUp(buttonID))
                    OnUp?.Invoke();
            }
            else
            {
                if (Input.GetKeyDown(key))
                    OnDown?.Invoke();
                else if (Input.GetKeyUp(key))
                    OnUp?.Invoke();
            }
        }
    }
}
