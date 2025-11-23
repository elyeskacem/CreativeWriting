using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class OpenAITest : MonoBehaviour
{
    [Header("OpenAI Settings")]
    public string apiKey = "YOUR_OPENAI_API_KEY";
    public string modelName = "gpt-4.1";

    void Start()
    {
        // Test a simple prompt
        StartCoroutine(SendOpenAIRequest("Say hello from Unity!"));
    }

    IEnumerator SendOpenAIRequest(string prompt)
    {
        string url = "https://api.openai.com/v1/chat/completions";

        // Build request JSON
        string json = JsonUtility.ToJson(new ChatRequest(prompt, modelName));

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log("Sending request to OpenAI...");

            // Send request
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response:\n" + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public Message[] messages;

    public ChatRequest(string prompt, string modelName)
    {
        this.model = modelName;
        this.messages = new Message[] { new Message(prompt) };
    }
}

[System.Serializable]
public class Message
{
    public string role = "user";
    public string content;

    public Message(string content)
    {
        this.content = content;
    }
}
