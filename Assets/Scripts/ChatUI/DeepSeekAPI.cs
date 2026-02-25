using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DeepSeekAPI : SingletonMono<DeepSeekAPI>
{
    // API Settings
    private string apiKey = "sk-5924cc514a7a416c85161ad9a989e9be";
    private string apiUrl = "https://api.deepseek.com/chat/completions";

    // API call back
    public delegate void DialogueCallback(string content, bool isSuccess);


    public void SendMessageToDeepSeek(DeepSeekData data, List<Message> messages, DialogueCallback callback)
    {
        StartCoroutine(PostRequest(data, messages, callback));
    }


    /// <summary>
    /// Coroutine to post chat request
    /// </summary>
    /// <param name="message"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    IEnumerator PostRequest(DeepSeekData data, List<Message> messages, DialogueCallback callback)
    {
        // set request body
        ChatRequest requestBody = new()
        {
            model = data.model_name,
            messages = messages,
            max_tokens = data.max_tokens,
            temperature = data.temperature,
            top_p = data.top_p,
            frequency_penalty = data.frequency_penalty,
            type = data.type,
        };

        // convert to json
        string jsonBody = JsonConvert.SerializeObject(requestBody);

        // set web request
        UnityWebRequest request = CreateWebRequest(jsonBody);

        // send request
        yield return request.SendWebRequest();
        // Debug.Log($"json response: {request.downloadHandler.text}");

        // check result
        if (IsRequestError(request))
        {
            if (request.responseCode == 429)
            {
                Debug.LogWarning("request limit, wait to retry");
                yield return new WaitForSeconds(5);
                StartCoroutine(PostRequest(data, messages, callback));
                yield break;
            }
            else
            {
                Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
                callback?.Invoke($"API request fail: {request.downloadHandler.text}", false);
                yield break;
            }
        }

        // parse json response
        DeepSeekResponse response = ParseResponse(request.downloadHandler.text);
        if (response != null && response.choices.Length > 0)
        {
            string reply = response.choices[0].message.content;
            callback?.Invoke(reply, true);
            Debug.Log($"{data.name} reply: {reply}");
        }
        else
        {
            callback?.Invoke($"{data.name} ÏÝÈë³ÁÄ¬", false);
        }
        request.Dispose();
    }


    public void AddMessage(List<Message> messages, string role, string content)
    {
        Message message = new Message() 
        {
            role = role,
            content = content
        };

        messages.Add(message);
    }


    /// <summary>
    /// create UnityWebRequest by jsonBody raw data
    /// </summary>
    /// <param name="jsonBody"></param>
    /// <returns></returns>
    private UnityWebRequest CreateWebRequest(string jsonBody)
    {
        UnityWebRequest request = new(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        return request;
    }


    /// <summary>
    /// check DeepSeek response
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private bool IsRequestError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }


    /// <summary>
    /// parse json response to unity class 
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns></returns>
    private DeepSeekResponse ParseResponse(string jsonResponse)
    {
        try
        {
            DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(jsonResponse);
            if (response == null || response.choices == null || response.choices.Length == 0)
            {
                Debug.LogError("API response format has problem or invalid data");
                return null;
            }
            return response;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fail to parse json string to unity class: {e.Message}\n Response content: {jsonResponse}");
            return null;
        }
    }
}
