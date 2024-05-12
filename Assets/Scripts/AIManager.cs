using System;
using System.IO;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public string imageFileName = "image-360-2.jpg";
    private string imageFilePath;
    private Texture2D texture;
    [SerializeField] private Material backgroundMaterial;

    void Start()
    {
        imageFilePath = Path.Combine(Application.dataPath, "Textures", imageFileName);

        if (File.Exists(imageFilePath))
        {
            byte[] imageData = File.ReadAllBytes(imageFilePath);
            texture = new Texture2D(2048, 1024);
            texture.LoadImage(imageData);
            backgroundMaterial.mainTexture = texture;
        }
        else
        {
            Debug.LogError("The image file was not found at the specified path: " + imageFilePath);
        }
    }
}