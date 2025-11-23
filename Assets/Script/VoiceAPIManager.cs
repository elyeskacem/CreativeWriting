using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class VoiceAPIManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string chatApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    public string apiKey = "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";

    public void SendMessageToAI(string userMessage)
    {
        StartCoroutine(SendChatRequest(userMessage));
    }

    IEnumerator SendChatRequest(string userMessage)
    {
        // OpenRouter API expects this format
        OpenRouterRequest requestData = new OpenRouterRequest
        {
            model = "openai/gpt-3.5-turbo",
            messages = new Message[]
            {
                new Message { role = "user", content = userMessage }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(chatApiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Raw Response: " + request.downloadHandler.text);

                OpenRouterResponse response = JsonUtility.FromJson<OpenRouterResponse>(request.downloadHandler.text);

                if (response.choices != null && response.choices.Length > 0)
                {
                    string aiResponse = response.choices[0].message.content;
                    Debug.Log("AI Response: " + aiResponse);

                    ChatUI chatUI = FindAnyObjectByType<ChatUI>();
                    if (chatUI != null)
                    {
                        chatUI.DisplayAIResponse(aiResponse);
                    }
                }
            }
            else
            {
                Debug.LogError("Chat API Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);

                ChatUI chatUI = FindAnyObjectByType<ChatUI>();
                if (chatUI != null)
                {
                    chatUI.DisplayAIResponse("Sorry, I'm having connection issues.");
                }
            }
        }
    }
}

[System.Serializable]
public class OpenRouterRequest
{
    public string model;
    public Message[] messages;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class OpenRouterResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}