// DeepSeek models name
using System.Collections.Generic;

public static class DeepSeekModelName
{
    public static string chatModel = "deepseek-chat";
    public static string reasonerModel = "deepseek-reasoner";
}

public static class DeepSeekRole
{
    public static string system = "system";
    public static string user = "user";
    public static string assistant = "assistant";
}

// DeepSeek Params
public class DeepSeekData
{
    public string name;
    public string prompt;
    public string model_name;
    public int max_tokens;
    public float temperature;
    public float top_p;
    public float frequency_penalty;
    public string type;
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public List<Message> messages;
    public int max_tokens;
    public float temperature;
    public float top_p;
    public float frequency_penalty;
    public string type;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class DeepSeekResponse
{
    public Choice[] choices;
}
