using UnityEngine;

public class ArtilleryCamManager : MonoBehaviour
{
    public static GameObject cameraOutput;

    private void Start()
    {
        cameraOutput = gameObject;

        cameraOutput.SetActive(false);
    }
}
