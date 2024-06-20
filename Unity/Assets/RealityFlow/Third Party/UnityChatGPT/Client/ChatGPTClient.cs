using DilmerGames.Core.Singletons;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGPTClient : Singleton<ChatGPTClient>
{
    [SerializeField]
    private ChatGTPSettings chatGTPSettings;

    public IEnumerator Ask(string prompt, Action<ChatGPTResponse> callBack)
    {
        var url = chatGTPSettings.debug ? $"{chatGTPSettings.apiURL}?debug=true" : chatGTPSettings.apiURL;

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            var requestParams = JsonConvert.SerializeObject(new ChatGPTRequest
            {
                Model = chatGTPSettings.apiModel,
                Messages = new ChatGPTChatMessage[]
                {
                    new ChatGPTChatMessage
                    {
                        role = "user",
                        content = prompt
                    }
                }
            });

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestParams);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;
            request.disposeCertificateHandlerOnDispose = true;

            request.SetRequestHeader("Content-Type", "application/json");

            // Retrieve the API key from the environment manager
            string apiKey = EnvConfigManager.Instance.OpenAIApiKey;
            string organization = EnvConfigManager.Instance.OpenAIOrganization;

            // Set the request headers
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            else
            {
                Debug.LogError("API key is null or empty.");
            }

            if (!string.IsNullOrEmpty(organization))
            {
                request.SetRequestHeader("OpenAI-Organization", organization);
            }
            else
            {
                Debug.LogWarning("OpenAI-Organization is null or empty.");
            }

            var requestStartDateTime = DateTime.Now;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Request error: " + request.error);
            }
            else
            {
                string responseInfo = request.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<ChatGPTResponse>(responseInfo)
                    .CodeCleanUp();

                response.ResponseTotalTime = (DateTime.Now - requestStartDateTime).TotalMilliseconds;

                // Write response to a file
                WriteResponseToFile(responseInfo);

                callBack(response);
            }
        }
    }

    private void WriteResponseToFile(string response)
    {
        string path = Application.persistentDataPath + "/ChatGPTResponse.json";
        try
        {
            File.WriteAllText(path, response);
            Debug.Log("Response written to file: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write response to file: " + e.Message);
        }
    }
}
