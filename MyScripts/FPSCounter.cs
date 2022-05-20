using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{

    private TextMeshProUGUI fpsText;
    // Start is called before the first frame update
    void Start()
    {
        fpsText = gameObject.GetComponent<TextMeshProUGUI>();

        if (PlayerPrefs.GetInt("FPSCounter", 0) == 1)
            fpsText.enabled = true;
        else
            fpsText.enabled = false;

        StartCoroutine(countFps());
    }

    // Update is called once per frame
    IEnumerator countFps()
    {
        while (true)
        {
            if (fpsText.enabled)
                fpsText.text = "FPS " + ((int)(1f / Time.unscaledDeltaTime)).ToString();

            if (PlayerPrefs.GetInt("FPSCounter", 0) == 1)
                fpsText.enabled = true;
            else
                fpsText.enabled = false;

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
