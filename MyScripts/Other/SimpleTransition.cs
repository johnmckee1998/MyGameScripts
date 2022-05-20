using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTransition : MonoBehaviour
{
    private Image img;
    public bool startTransition;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (img.fillAmount <= 0)
            Destroy(gameObject);
        else if(startTransition)
            img.fillAmount -= Time.deltaTime * 2;
    }
}
