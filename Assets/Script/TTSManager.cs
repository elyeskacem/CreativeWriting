using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TTSManager : MonoBehaviour
{
    [Header("ElevenLabs Configuration")]
    public string apiKey = "your-elevenlabs-api-key";
    public string voiceId = "21m00Tcm4TlvDq8ikWAM"; // Rachel voice

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void ConvertTextToSpeech(string text)
    {
        StartCoroutine(TTSCoroutine(text));
    }

    IEnumerator TTSCoroutine(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        TTSRequest requestData = new TTSRequest
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new VoiceSettings
            {
                stability = 0.5f,
                similarity_boost = 0.5f
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

                if (audioClip != null)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    Debug.Log("Playing TTS audio");
                }
                else
                {
                    Debug.LogError("Failed to create AudioClip from response");
                }
            }
            else
            {
                Debug.LogError($"TTS Error: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                if (request.downloadHandler != null)
                {
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
            }
        }
    }
}

[System.Serializable]
public class TTSRequest
{
    public string text;
    public string model_id;
    public VoiceSettings voice_settings;
}

[System.Serializable]
public class VoiceSettings
{
    public float stability;
    public float similarity_boost;
}