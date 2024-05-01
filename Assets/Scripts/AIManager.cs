using System;
using System.IO;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public string imageFileName = "image-360-2.jpg"; // The name of the image file
    private string imageFilePath; // The full path to the image file
    private Texture2D texture;
    [SerializeField] private MeshRenderer sphereRenderer;

    void Start()
    {
        // Get the full path of the image file within the Assets folder
        imageFilePath = Path.Combine(Application.dataPath, "Textures", imageFileName);

        if (File.Exists(imageFilePath))
        {
            // Load the image as a byte array
            byte[] imageData = File.ReadAllBytes(imageFilePath);
            texture = new Texture2D(2048, 1024); // Initialize with the exact width and height
            texture.LoadImage(imageData);
            sphereRenderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("The image file was not found at the specified path: " + imageFilePath);
        }
    }
}