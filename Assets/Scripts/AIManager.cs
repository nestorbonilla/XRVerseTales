using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using OVRSimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class AIManager : MonoBehaviour
{
    const int _imageWidth = 2048;
    const int _imageHeight = 1024;
    private readonly HttpClient _httpClient = new HttpClient();
    
    private string _defaultImageFileName = "image-360-2.jpg";
    private string _imageFilePath;
    private Texture2D _texture;
    [SerializeField] private Material _backgroundMaterial;
    private Dictionary<string, byte[]> _imageByPage = new Dictionary<string, byte[]>();
    
    
    private string _serverUrl = "http://127.0.0.1:5000/";
    private string _serverActionPrompt = "generate_image";
    private bool _isGeneratingImage = false;

    void Start()
    {
        _imageFilePath = Path.Combine(Application.dataPath, "Textures", _defaultImageFileName);

        if (File.Exists(_imageFilePath))
        {
            byte[] imageData = File.ReadAllBytes(_imageFilePath);
            _texture = new Texture2D(_imageWidth, _imageHeight);
            _texture.LoadImage(imageData);
            _backgroundMaterial.mainTexture = _texture;
        }
        else
        {
            Debug.LogError("Image not found at path: " + _imageFilePath);
        }
    }

    public void PaintBackgroundImage(JSONObject requestData, string name)
    {
        if (_imageByPage.ContainsKey(name))
        {
            SetTexture(_imageByPage[name]);
        }
        else
        {
            StartCoroutine(GenerateImageCoroutine(requestData, name));
        }
    }
    
    private void SetTexture(byte[] imageData)
    {
        Texture2D texture  = new Texture2D(_imageWidth, _imageHeight);
        texture.LoadImage(imageData);
        _backgroundMaterial.mainTexture  = texture;
    }
    
    private IEnumerator GenerateImageCoroutine(JSONObject requestData, string name)
    {
        _isGeneratingImage = true;
        string jsonData = requestData.ToString();
        UnityWebRequest www = new UnityWebRequest(_serverUrl + _serverActionPrompt, "POST");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("POST request successful: " + www.downloadHandler.text);
            byte[] imageData = www.downloadHandler.data;
            SetTexture(imageData);
            _imageByPage.Add(name, imageData);
        }
        else
        {
            // Muestra el error en caso de que ocurra uno
            Debug.LogError("Error in POST request: " + www.error);
        }
        _isGeneratingImage = false;
    }
}