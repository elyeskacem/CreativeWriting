using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro; // Needed for InputField

public class FrenchChatTTS_Fixed : MonoBehaviour
{
    [Header("3D Text Output")]
    [Tooltip("Drag your '3D Text' GameObject here")]
    public GameObject textObject;

    [Header("UI Input")]
    public TMP_InputField userInput;
    public Button sendButton;

    [Header("Settings")]
    public string openAIKey;
    [TextArea] public string systemPrompt = "Tu es un assistant utile. Tu réponds toujours en français.";

    // Private variables to hold whichever text component we find
    private TextMesh _legacyTextMesh;
    private TextMeshPro _tmpMesh; // In case you use 3D TMP later
    private bool _hasTextComponent = false;

    private const int MaxLineLength = 40; // Word wrap limit

    private void Start()
    {
        // 1. Validate UI
        if (userInput == null || sendButton == null)
        {
            Debug.LogError("❌ UI Error: Please assign InputField and Button.");
            return;
        }

        // 2. Find the Text Component on the 3D Object
        if (textObject != null)
        {
            // Try getting Legacy TextMesh
            _legacyTextMesh = textObject.GetComponent<TextMesh>();

            if (_legacyTextMesh != null)
            {
                _hasTextComponent = true;
                Update3DText("Bonjour ! (Legacy)");
            }
            else
            {
                // Fallback: Try getting TextMeshPro 3D just in case
                _tmpMesh = textObject.GetComponent<TextMeshPro>();
                if (_tmpMesh != null)
                {
                    _hasTextComponent = true;
                    Update3DText("Bonjour ! (TMP 3D)");
                }
                else
                {
                    Debug.LogError("❌ Error: The object in 'Text Object 3D' does not have a 'TextMesh' component!");
                }
            }
        }
        else
        {
            Debug.LogError("❌ Error: Please drag your 3D Text object into the 'Text Object 3D' slot.");
        }

        sendButton.onClick.AddListener(OnSendButtonPressed);
    }

    // --- Helper to update text regardless of component type ---
    private void Update3DText(string newText)
    {
        if (!_hasTextComponent) return;

        string wrappedText = WordWrap(newText);

        if (_legacyTextMesh != null)
        {
            _legacyTextMesh.text = wrappedText;
        }
        else if (_tmpMesh != null)
        {
            _tmpMesh.text = wrappedText;
        }
    }

    public void OnSendButtonPressed()
    {
        string prompt = userInput.text;
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            sendButton.interactable = false;
            userInput.interactable = false;

            Update3DText("Réflexion...");

            StartCoroutine(GetOpenAIResponse(prompt));
        }
    }

    private IEnumerator GetOpenAIResponse(string userMessage)
    {
        string url = "https://api.openai.com/v1/chat/completions";

        OpenAIRequest req = new OpenAIRequest();
        req.model = "gpt-4o-mini";
        req.messages = new List<ChatMessage>
        {
            new ChatMessage { role = "system", content = systemPrompt },
            new ChatMessage { role = "user", content = userMessage }
        };

        string json = JsonUtility.ToJson(req);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAIKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API Error: " + request.error);
            Update3DText("Erreur de connexion.");
        }
        else
        {
            var wrapper = JsonUtility.FromJson<OpenAIResponseWrapper>(request.downloadHandler.text);
            if (wrapper.choices != null && wrapper.choices.Length > 0)
            {
                string reply = wrapper.choices[0].message.content;
                Update3DText(reply);
                SpeakText(reply);
            }
        }

        sendButton.interactable = true;
        userInput.interactable = true;
        userInput.text = "";
        userInput.ActivateInputField();
    }

    private void SpeakText(string text)
    {
        Debug.Log("TTS Output: " + text);
    }

    private string WordWrap(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string[] words = text.Split(' ');
        StringBuilder builder = new StringBuilder();
        int currentLineLength = 0;
        foreach (string word in words)
        {
            if (currentLineLength + word.Length > MaxLineLength)
            {
                builder.Append("\n");
                currentLineLength = 0;
            }
            builder.Append(word + " ");
            currentLineLength += word.Length + 1;
        }
        return builder.ToString();
    }

    // --- INTERNAL DATA CLASSES ---
    [System.Serializable] private class OpenAIRequest { public string model; public List<ChatMessage> messages; }
    [System.Serializable] private class OpenAIResponseWrapper { public Choice[] choices; }
    [System.Serializable] private class Choice { public ChatMessage message; }
    [System.Serializable] private class ChatMessage { public string role; public string content; }
}