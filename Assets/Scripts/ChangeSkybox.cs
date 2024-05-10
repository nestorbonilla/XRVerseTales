using UnityEngine;

public class ChangeSkybox : MonoBehaviour
{
    public Material newSkybox;

    public void ChangeTheSkybox()
    {
        RenderSettings.skybox = newSkybox;
    }
}