using Meta.WitAi.TTS.Utilities;
using TMPro;
using UnityEngine;

public class TTSText : MonoBehaviour
{
    private void OnEnable()
    {
        TTSSpeaker speaker = GameObject.FindObjectOfType<TTSSpeaker>();
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        speaker.Speak(text.text);
    }

    private void OnDisable()
    {
        TTSSpeaker speaker = GameObject.FindObjectOfType<TTSSpeaker>();
        speaker.Stop();
    }
}
