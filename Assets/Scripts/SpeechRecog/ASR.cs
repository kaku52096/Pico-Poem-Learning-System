using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

public class ASR : SingletonMono<ASR>
{
    // record audio
    private AudioClip recordedClip;
    private bool isRecording;
    private string audioPath;

    // WebRequest
    private string androidUrl = "http://192.168.3.5:5000/asr";
    private string localURL = "http://127.0.0.1:5000/asr";

    // post asr check : deepseek model
    private DeepSeekData data;
    private List<Message> messages;


    private void Start()
    {
        audioPath = Application.persistentDataPath + "/" + "temp.wav";
        isRecording = false;

        // set deepseek data
        InitASRPostAI();
    }

    // initialize LLM post check for ASR output
    private void InitASRPostAI()
    {
        data = new DeepSeekData()
        {
            model_name = DeepSeekModelName.reasonerModel,
            temperature = 0.1f,
            top_p = 0.5f,
            frequency_penalty = 0.5f,
            max_tokens = 200,
            type = "text",
            name = "语音识别纠错助手",
            prompt = "你是一个语音识别文本校对助手，请修正以下对话文本中因语音识别造成的错字、标点，不改变文本本身的语义和风格。" +
            "对话的主题是中国古诗词。直接输出修正后的结果，不要添加任何解释、说明。"
        };

        messages = new List<Message>();
    }

    private IEnumerator StartRecordAudio()
    {
        // lock chat panel
        ChatController.Instance.switchable = false;
        isRecording = true;

        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.openClip, 0.25f);
        yield return null;

        // start microphone record
        recordedClip = Microphone.Start(null, false, 30, 44100);
    }

    private IEnumerator StopRecordAudio()
    {
        // end microphone record
        Microphone.End(null);

        yield return null;
        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.closeClip, 0.25f);

        // save audio clip to wav file
        WavUtility.SaveAudioClipToWav(recordedClip, audioPath);

        // ASRWeb request
        yield return null;
        ChatController.Instance.SetSystemText("（语音识别中...）");
        StartCoroutine(AudioToText());

        // unlock chat panel
        ChatController.Instance.switchable = true;
        isRecording = false;
    }

    // Called by PXR_HandPose
    public void ASRStart()
    {
        // check chatPanel
        if (!ChatController.Instance.IsChatPanelActive()) return;        

        if (isRecording) return;

        StartCoroutine(StartRecordAudio());
    }

    // Called by PXR_HandPose
    public void ASREnd()
    {
        // check chatPanel
        if (!ChatController.Instance.IsChatPanelActive()) return;

        if (!isRecording) return;

        StartCoroutine(StopRecordAudio());
    }


    /// <summary>
    /// use web api to recog audio file
    /// </summary>
    /// <returns></returns>
    IEnumerator AudioToText()
    {
        // check file
        if (!File.Exists(audioPath))
        {
            Debug.LogError($"文件不存在: {audioPath}");
            ChatController.Instance.SetSystemText("（音频文件不存在）");
            yield break;
        }

        // set wav file
        WWWForm form = new();
        byte[] fileData = File.ReadAllBytes(audioPath);
        form.AddBinaryData("file", fileData, Path.GetFileName(audioPath), "audio/wav");

        // send request
        UnityWebRequest request = UnityWebRequest.Post(localURL, form);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("speech recognition success");
            string asrtext = request.downloadHandler.text;

            asrtext = asrtext.Substring(1, asrtext.Length - 2);     // remove ""
            ChatController.Instance.SetInputText(asrtext);          // send asrtext to chat panel
        }
        else
        {
            Debug.LogError($"fail: {request.error}");
            if (request.responseCode == 400)
                Debug.LogError("wrong file");

            if (request.responseCode == 500)
                Debug.LogError("fail to save wav file in web");

            ChatController.Instance.SetSystemText("（语音识别未开启）");
        }
    }

    public void PostAICheck(string content)
    {
        // create messages
        messages.Clear();
        DeepSeekAPI.Instance.AddMessage(messages, DeepSeekRole.system, data.prompt);
        DeepSeekAPI.Instance.AddMessage(messages, DeepSeekRole.user, content);
        DeepSeekAPI.Instance.SendMessageToDeepSeek(data, messages, HandleAIResponse);

        // show system text
        ChatController.Instance.SetSystemText("（修正中...）");
    }

    // callback to handle post check result
    private void HandleAIResponse(string content, bool isSuccess)
    {
        if (isSuccess)
        {
            //content = content.Substring(1, content.Length - 2);         // remove ""
            ChatController.Instance.ClearInputText();
            ChatController.Instance.SetInputText(content);
        }
        else
        {
            ChatController.Instance.SetSystemText("（语音识别后处理出错）");
        }
    }
}
