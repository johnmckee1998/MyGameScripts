using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyCounter : MonoBehaviour
{
    public GameObject endTrigger;
    public int targetKeyAmount = 0;
    private int currentKeyAmount;
    public Text keyCountDisplay; 

    // Start is called before the first frame update
    void Start()
    {
        currentKeyAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        keyCountDisplay.text = ("Keys to find " + (targetKeyAmount - currentKeyAmount).ToString());
        if (currentKeyAmount == targetKeyAmount)
            endTrigger.GetComponent<EndingScript>().setAct(true);
    }

    public void addKey(int i)
    {
        currentKeyAmount += i;
    }

}
