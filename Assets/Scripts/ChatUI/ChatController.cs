using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.XR.PICO.LivePreview;
using UnityEngine;
using UnityEngine.UI;
using static Unity.XR.PXR.PXR_Plugin;

public class ChatController : SingletonMono<ChatController>
{
    // chat panel transform
    [Header("Chat Panel Settings")]
    public Transform Camera;
    public GameObject chatPanel;
    [HideInInspector] public Vector3 canvasLocalPosition;
    [HideInInspector] public Vector3 canvasWorldPosition;
    [HideInInspector] public Vector3 canvasEularRotation;
    [HideInInspector] public Quaternion canvasLocalRotation;
    [HideInInspector] public Quaternion canvasWorldRotation;
    [HideInInspector] public bool switchable;
    private bool isActive;

    // scroll view
    [Header("Scroll View Settings")]
    public Transform scrollContent;
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public GameObject leftBubble;
    public GameObject rightBubble;
    private List<GameObject> bubbles;
    private bool refreshPanel;

    // input panel
    [Header("Input Panel Settings")]
    public GameObject inputPanel;
    private RectTransform inputPanelRectTransform;
    public TMP_InputField inputField;
    public Button clearButton;
    public Button sendButton;
    public Button correctButton;
    private string inputText;

    // npc interaction : deepseek model
    private DeepSeekData chatData;
    private List<Message> chatMessages;
    private string introduction;
    private bool generatable;

    [Header("Treatment Settings")]
    public bool treatmentA;
    public bool treatmentB;


    private void Start()
    {
        // set deepseek params
        InitChatAI();

        // set chatpanel
        if (chatPanel == null)
            chatPanel = transform.GetChild(0).gameObject;

        canvasLocalPosition = new Vector3(0, 0, 1);
        canvasLocalRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        isActive = true;
        switchable = true;
        SwitchChatPanelState();

        // set scroll view
        bubbles = new List<GameObject>();

        // set inputfield
        inputPanelRectTransform = inputPanel.GetComponent<RectTransform>();
        inputText = "";
        SetSystemText("（使用语音输入...）");

        // introduction for user
        introduction = "您好，我是您的古诗词学习助手，很高兴为您服务。您可以在聊天界面使用语音输入与我畅聊古诗词话题方面的任何内容，我会尽心为您答疑解惑。" +
                       "如果您想要体验当前谈及的诗词，我会实时生成该诗词描绘的场景，让您畅游诗中。下面我将为您介绍如何使用本系统：" +
                       "\n\n<color=orange>如何与系统交互？</color>" +
                       "\n\n1.将双手伸出，稍等之后可以看到手势识别模型；" +
                       "\n\n2.左手大拇指向上，四指从伸直转为合拢做出点赞手势，可以开关聊天界面；" +
                       "\n\n3.右手做出点赞手势，在听到提示音后保持手势，此时可以进行语音输入。在解除手势后语音识别结束，并在输入框中输出识别文本；" +
                       "\n\n4.橙色圆圈是手势射线在聊天面板上的落点，在指向输入框下方按钮时，将食指与大拇指接触，做出类似ok手势，可以点击按钮；" +
                       "\n\n5.如果语音识别出来的内容有部分错误，可以点击修正按钮，在等待一段时间后自动修复；" +
                       "\n\n6.点击聊天框，保持ok手势并向上或向下滑动，可以滑动聊天框；" +
                       "\n\n7.如果您想要体验当前谈及诗词的场景，输入指令\"体验场景\"并发送，稍等之后为您生成场景；" +
                       "\n\n8.在关闭面板后，您可以通过摆臂实现在诗词场景中的移动，摆臂越快移动越快。" +

                       "\n\n<color=orange>如何与我交流？</color>" +
                       "\n\n如果您不知如何开始，以下是一些用例：" + 
                       "\n\n【寻章摘句】" +
                       "\n\"想品读柳宗元的《江雪》，能为我解析吗？\"" +
                       "\n\"请问'春潮带雨晚来急，野渡无人舟自横。'出自哪首诗词？\"" +
                       "\n\n【意会古人】" +
                       "\n\"想了解苏轼在黄州时期的创作故事。\"" +
                       "\n\"王维山水诗有哪些艺术特色？\"" +
                       "\n\n【信手拈来】" +
                       "\n\"为我推荐一首冷门但惊艳的宋词\"" +
                       "\n\"随机送我一首边塞诗吧\"" +
                       "\n\n【诗情创作】" +
                       "\n\"用'明月青山'为意象作首七绝\"" +
                       "\n\"帮我把'相思'写成藏头诗\"";

        CreateBubble(leftBubble, introduction);
        scrollRect.verticalNormalizedPosition = 1;

        // Treatment
        Treatment();
    }

    private void InitChatAI()
    {
        chatData = new DeepSeekData()
        {
            model_name = DeepSeekModelName.chatModel,
            temperature = 0.1f,
            top_p = 0.5f,
            frequency_penalty = 0.2f,
            max_tokens = 1024,
            type = "text",
            name = "中国古诗词学习助手",
            prompt = "你是一位精通中国古诗词的智能助手，擅长注解、赏析古诗词。你的能力包括：" +
            "1.输出诗歌的译文及注解；" +
            "2.结合历史提供诗人创作背景知识；" +
            "3.赏析诗词的艺术手法与文学价值；" +
            "\n\n回答时先输出诗词标题、内容。回答需符合学术规范，引用文献优先选择权威资料。" +
            "当用户提出想要体验诗词描写的场景时，判断当前谈及的诗词是否涉及场景描写。" +
            "不需要解释，如果没有涉及任何场景描写，输出\"抱歉，这首诗词不具有场景描写内容。\"；如果涉及，输出诗词标题、内容，此外严禁输出其他任何额外内容。"
        };

        chatMessages = new List<Message>();
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.system, chatData.prompt);
    }


    // Called by PXR_HandPose
    public void SwitchChatPanelState()
    {
        // can not switch when player is running
        if (VRMoveController.Instance.isRunning)
            return;

        // can not switch when ASR is working
        if (!switchable)
            return;

        // set panel transform before open the panel
        if (isActive)
        {
            // open the chat panel
            canvasWorldPosition = Camera.TransformPoint(canvasLocalPosition);
            canvasWorldRotation = Camera.rotation * canvasLocalRotation;
            canvasEularRotation = canvasWorldRotation.eulerAngles;
            canvasEularRotation.z = 0;
            canvasWorldRotation = Quaternion.Euler(canvasEularRotation);

            this.transform.SetPositionAndRotation(canvasWorldPosition, canvasWorldRotation);
        }
        else
        {
            // close the chat panel
        }

        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.tickClip, 1.0f);
        chatPanel.SetActive(isActive);
        isActive = !isActive;
    }


    public bool IsChatPanelActive()
    {
        return chatPanel.activeSelf;
    }


    private void UpdateButtonInteractable()
    {
        if (inputText == "")
        {
            clearButton.interactable = false;
            sendButton.interactable = false;
            correctButton.interactable = false;
        }
        else
        {
            clearButton.interactable = true;
            sendButton.interactable = true;
            correctButton.interactable = true;
        }
    }


    // show system text in inputfield
    public void SetSystemText(string content)
    {
        inputField.text = inputText + content;
        UpdateButtonInteractable();
        LayoutRebuilder.ForceRebuildLayoutImmediate(inputPanelRectTransform);
    }


    // show asr text in inputfield
    public void SetInputText(string content)
    {
        // max input limit 
        if (inputText.Length > 100)
        {
            SetSystemText("（已到达输入上限）");
        }
        else
        {
            // update content for asr text
            StartCoroutine(TypeWriterEffect(inputField, inputText, content));
            inputText += content;
        }

        // rebuild vertical layout
        UpdateButtonInteractable();
        LayoutRebuilder.ForceRebuildLayoutImmediate(inputPanelRectTransform);
    }


    // type writer effect from currentText to newText
    private IEnumerator TypeWriterEffect(TMP_InputField inputField, string currentText, string newText)
    {
        StringBuilder subString = new StringBuilder();
        foreach (char c in newText)
        {
            subString.Append(c);
            inputField.text = currentText + subString.ToString();
            yield return new WaitForSeconds(0.05f);
        }
    }


    // type writer effect from currentText to newText
    private IEnumerator TypeWriterEffect(Text txt, string currentText, string newText)
    {
        refreshPanel = true;
        StringBuilder subString = new StringBuilder();
        foreach (char c in newText)
        {
            subString.Append(c);
            txt.text = currentText + subString.ToString();

            if (refreshPanel)
            {
                scrollRect.verticalNormalizedPosition = 0;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }


    // clear button onclick()
    public void ClearInputText()
    {
        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.tickClip, 1.0f);
        inputText = "";
        SetSystemText("（使用语音输入...）");
    }


    // send button onclick()
    public void SendInputText()
    {
        // create right bubble and left bubble
        CreateBubble(rightBubble, inputText);

        // check user cmd
        if (inputText.Contains("体验") && inputText.Contains("场景"))
        {
            if (generatable)
            {
                // Text txt = bubbles[^1].GetComponentInChildren<Text>();
                // StartCoroutine(TypeWriterEffect(txt, "", "好的，我正在生成场景，请稍后..."));
                SceneGenerator.Instance.CreateScene();
            }
            else
            {
                CreateBubble(leftBubble, "抱歉，当前讨论的诗词不涉及场景描写，无法生成对应场景。");
            }

            ClearInputText();
            return;
        }

        CreateBubble(leftBubble, "思考中...");


        // update history messages
        chatMessages.Clear();
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.system, chatData.prompt);
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.user, inputText);

        // send message to deepseek
        DeepSeekAPI.Instance.SendMessageToDeepSeek(chatData, chatMessages, HandleAIResponse);

        ClearInputText();
    }

    // correct button onclick()
    public void CorrectInputText()
    {
        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.tickClip, 1.0f);
        ASR.Instance.PostAICheck(inputText);
    }

    // callback to handle chat response
    private void HandleAIResponse(string content, bool isSuccess)
    {
        AudioSystem.Instance.SystemAudioPlay(AudioSystem.Instance.tickClip, 1.0f);
        
        if (!isSuccess)
        {
            Debug.Log(content);
            return;
        }

        // update history messages
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.assistant, content);

        // update left bubble
        Text txt = bubbles[^1].GetComponentInChildren<Text>();
        string text = content;
        text = text.Replace("*", "");
        text = text.Replace("**", "");
        text = text.Replace("***", "");
        text = text.Replace("#", "");
        text = text.Replace("##", "");
        text = text.Replace("###", "");
        text = text.Replace("\r", "");
        text = text.Replace(" ", "\u00A0");
        StartCoroutine(TypeWriterEffect(txt, "", text));

        // check current poem
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.user, "我想体验这首诗歌的场景。");
        DeepSeekAPI.Instance.SendMessageToDeepSeek(chatData, chatMessages, CheckCurrentPoem);
    }

    // check current poem whether suitable for scene generating
    private void CheckCurrentPoem(string content, bool isSuccess)
    {
        if (!isSuccess)
        {
            Debug.LogError("fail to check current poem");
            return;
        }

        // update history messages
        DeepSeekAPI.Instance.AddMessage(chatMessages, DeepSeekRole.assistant, content);

        if (content.Contains("抱歉"))
        {
            SceneGenerator.Instance.currentPoemData = null;
            generatable = false;     
        }
        else
        {
            // suitable
            SceneGenerator.Instance.currentPoemTxt = content;
            SceneGenerator.Instance.AnalysePoemParams(content);
            generatable = true;
        }
    }


    // create chat bubble
    public void CreateBubble(GameObject bubblePrefab, string content)
    {
        GameObject bubble = Instantiate(bubblePrefab, scrollContent);
        Text txt = bubble.GetComponentInChildren<Text>();
        txt.text = content;
        bubbles.Add(bubble);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    // change refreshPanel to stop vertical refresh
    public void StopRefreshPanel()
    {
        refreshPanel = false;
    }

    private void Treatment()
    {
        if (treatmentA && treatmentB)
            Debug.LogError("treatment error");

        if (!treatmentA && !treatmentB)
            return;

        if (treatmentB)
        {
            // text for poetry A, VR for poetry B
            string poetryB = "Treatment B\n在本环节中，您需要学习诗词集B的四首古诗词\n使用语音输入<color=orange>\"赏析第一首诗词\"</color>并发送，等待回复，" +
                "完成阅读后输入<color=orange>\"体验场景\"</color>体验生成的VR诗词情境，完成对第一首诗词的学习，其余三首以此类推。\n" +
                "\n第一首：《清平乐·雨晴烟晚》 冯延巳〔五代〕" +
                "\n第二首：《酬晖上人秋夜山亭有赠》 陈子昂〔唐代〕" +
                "\n第三首：《浣溪沙·半夜银山上积苏》 苏轼〔宋代〕" +
                "\n第四首：《碛中作》 岑参〔唐代〕";

            CreateBubble(leftBubble, poetryB);

            // update system prompt
            chatData.prompt += TreatmentData.contentB;
        }
        else
        {
            // text for poetry B, VR for poetry A
            string poetryA = "Treatment A\n在本环节中，您需要学习诗词集A的四首古诗词\n使用语音输入<color=orange>\"赏析第一首诗词\"</color>并发送，等待回复，" +
                "完成阅读后输入<color=orange>\"体验场景\"</color>体验生成的VR诗词情境，完成对第一首诗词的学习，其余三首以此类推。\n" +
                "\n第一首：《春思二首·其一》 贾至〔唐代〕" +
                "\n第二首：《临江仙引·渡口》 柳永〔宋代〕" +
                "\n第三首：《雪望》 洪升〔清代〕" +
                "\n第四首：《调笑令·边草》 戴叔伦〔唐代〕";

            CreateBubble(leftBubble, poetryA);

            // update system prompt
            chatData.prompt += TreatmentData.contentA;
        }

        scrollRect.verticalNormalizedPosition = 1;
    }

    private void OnDestroy()
    {
        // clear bubble objs
        if (bubbles != null)
        {
            foreach (var obj in bubbles)
            {
                if (obj != null)
                    Destroy(obj);
            }

            bubbles.Clear();
        }
    }
}
