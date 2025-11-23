using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField messageInput;
    public TextMeshProUGUI chatDisplay;
    public Button sendButton;
    public GameObject typingIndicator;

    private VoiceAPIManager apiManager;

    void Start()
    {
        apiManager = FindAnyObjectByType<VoiceAPIManager>();

        if (apiManager == null)
        {
            Debug.LogError("VoiceAPIManager not found in scene!");
        }

        sendButton.onClick.AddListener(SendMessage);
        messageInput.onSubmit.AddListener(delegate { SendMessage(); });

        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }

        chatDisplay.text = "Welcome! Type a message to start chatting...";
    }

    void Update()
    {
        sendButton.interactable = !string.IsNullOrEmpty(messageInput.text);
    }

    void SendMessage()
    {
        string message = messageInput.text.Trim();
        if (!string.IsNullOrEmpty(message) && apiManager != null)
        {
            chatDisplay.text += $"\n\n<b>You:</b> {message}";
            messageInput.text = "";

            if (typingIndicator != null)
            {
                typingIndicator.SetActive(true);
            }

            messageInput.interactable = false;
            sendButton.interactable = false;

            apiManager.SendMessageToAI(message);

            Canvas.ForceUpdateCanvases();
        }
    }

    public void DisplayAIResponse(string response)
    {
        if (typingIndicator != null)
        {
            typingIndicator.SetActive(false);
        }

        messageInput.interactable = true;
        sendButton.interactable = true;

        chatDisplay.text += $"\n\n<b>AI:</b> {response}";

        Canvas.ForceUpdateCanvases();
        messageInput.Select();
        messageInput.ActivateInputField();
    }
}