using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class ButtonExtended : Button
{
    public int test;
    public UnityEvent onDown;
    public UnityEvent onUp;

    public override void OnPointerDown(PointerEventData eventData)
    {
        onDown?.Invoke(); //the ? is bascially a null check, kind of cool (it is a null conditional operator)
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        onUp?.Invoke();
        base.OnPointerUp(eventData);
        //hide text
    }
}
