using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniStorm;
using MapMagic.Core;
using MapMagic.Nodes;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using Graph = MapMagic.Nodes.Graph;
using UnityEngine.Rendering;
using Crest;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class SceneGenerator : SingletonMono<SceneGenerator>
{
    #region Analyse Poem Scene Params
    // poem text analysis : deepseek model
    private DeepSeekData analyseData;
    private List<Message> analyseMessages;
    public GeneratorData currentPoemData;
    public string currentPoemTxt;
    public bool isAnalysing;
    

    // analyse poem text to generator params
    public void AnalysePoemParams(string poemContent)
    {
        isAnalysing = true;

        // creat messages
        analyseMessages.Clear();
        DeepSeekAPI.Instance.AddMessage(analyseMessages, DeepSeekRole.system, analyseData.prompt);
        DeepSeekAPI.Instance.AddMessage(analyseMessages, DeepSeekRole.user, poemContent);

        // send message to deekseek
        DeepSeekAPI.Instance.SendMessageToDeepSeek(analyseData, analyseMessages, HandleAIResponse);
    }


    // callback to handle json output
    private void HandleAIResponse(string content, bool isSuccess)
    {
        if (!isSuccess)
        {
            Debug.Log(content);
            return;
        }

        Debug.Log(content);

        // parse json output
        Match match = Regex.Match(content, @"\{[^{}]*\}");
        if (match.Success)
        {
            string jsonParams = match.Value;
            Debug.Log(jsonParams);
            currentPoemData = ParseGeneratorData(jsonParams);
        }
        else
        {
            Debug.LogError("fail to match json output");
        }

        isAnalysing = false;
    }


    // parse json parms to unity class and check value
    private GeneratorData ParseGeneratorData(string jsonParams)
    {
        try
        {
            GeneratorData generatorParams = JsonUtility.FromJson<GeneratorData>(jsonParams);
            CheckParamsValue(generatorParams);
            return generatorParams;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fail to parse json string to unity class: {e.Message}\n Response content: {jsonParams}");
            return null;
        }
    }


    // params value check
    private void CheckParamsValue(GeneratorData generatorParams)
    {
        // terrain
        if (!GeneratorDataValue.terrains.Contains(generatorParams.terrain))
            generatorParams.terrain = GeneratorDataValue.terrains[0];

        // biome
        if (!GeneratorDataValue.textures.Contains(generatorParams.texture) || generatorParams.terrain == "岛屿")
            generatorParams.texture = GeneratorDataValue.textures[0];

        // objects
        List<string> objTemp = new();
        for (int i = 0; i < generatorParams.objects.Length; i++)
        {
            if (GeneratorDataValue.objects.Contains(generatorParams.objects[i]))
            {
                objTemp.Add(generatorParams.objects[i]);
            }
        }
        generatorParams.objects = objTemp.ToArray();

        // season
        if (!GeneratorDataValue.seasons.Contains(generatorParams.season))
            generatorParams.season = GeneratorDataValue.seasons[0];

        // time
        if(!GeneratorDataValue.times.Contains(generatorParams.time))
            generatorParams.time = GeneratorDataValue.times[0];

        // weather
        if(!GeneratorDataValue.weathers.Contains(generatorParams.weather))
            generatorParams.weather = GeneratorDataValue.weathers[0];

        // audios
        List<string> audioTemp = new();
        for (int i = 0; i < generatorParams.audios.Length; i++)
        {
            if (GeneratorDataValue.audios.Contains(generatorParams.audios[i]))
            {
                audioTemp.Add(generatorParams.audios[i]);
            }
        }
        generatorParams.audios = audioTemp.ToArray();

        // emotion
        if (!GeneratorDataValue.emotions.Contains(generatorParams.emotion))
            generatorParams.emotion = GeneratorDataValue.emotions[0];
    }

    private void InitAnalyseAI()
    {
        // set deepseek params
        analyseData = new DeepSeekData()
        {
            model_name = DeepSeekModelName.reasonerModel,
            temperature = 0.2f,
            top_p = 0.2f,
            frequency_penalty = 0.2f,
            max_tokens = 10240,
            type = "json_object",
            name = "古诗词文本参数分析",
            prompt = "你是一个精通中国古诗词的文本分析助手，需要你对输入的古诗词文本分析其描写的自然环境的各类参数，并使用 JSON 格式输出。" +
            "参数类型包括地形(terrain)，地貌(texture)，物品(objects)，季节(season)，时间(time)，天气(weather)，声音(audios)，作者的情绪(emotion)" +
            "请严格从以下预定义参数中选择最接近诗词内容的参数。考虑诗歌题材，例如对于边塞诗，即使没有明确描写，地貌也应该为沙地。" +
            "严禁出现预定义之外的参数。对于标记为单选的参数类型，必须选择一个最符合接近的参数。" +
            "对于标记为多选的参数类型，选择所有可能在场景中出现的元素" +
            $"\r\n terrain：{GeneratorDataValue.ToJsonList(GeneratorDataValue.terrains)}（单选）" +
            $"\r\n texture：{GeneratorDataValue.ToJsonList(GeneratorDataValue.textures)}（单选）" +
            $"\r\n objects：{GeneratorDataValue.ToJsonList(GeneratorDataValue.objects)}（多选）" +
            $"\r\n season：{GeneratorDataValue.ToJsonList(GeneratorDataValue.seasons)}（单选）" +
            $"\r\n time：{GeneratorDataValue.ToJsonList(GeneratorDataValue.times)}（单选）" +
            $"\r\n weather：{GeneratorDataValue.ToJsonList(GeneratorDataValue.weathers)}（单选）" +
            $"\r\n audios：{GeneratorDataValue.ToJsonList(GeneratorDataValue.audios)}（多选）" +
            $"\r\n emotion：{GeneratorDataValue.ToJsonList(GeneratorDataValue.emotions)}（单选）" +
            "\r\n 严格使用 JSON 格式返回结果，如果没有适配的选项或文本没有提及，填\"无\"。"
        };

        analyseMessages = new List<Message>();
    }
    #endregion


    #region Map Magic Graph (terrain & texture)
    // map magic
    public GameObject mainCamera;
    private Camera VRCamera;
    public GameObject InfinityMapMagic;
    private MapMagicObject InfinityMapMagicObject;
    public Graph MountainGreenGraph;    // 山峦，林地，春
    public Graph MountainAutumnGraph;   // 山峦，林地，秋
    public Graph MountainSnowGraph;     // 山峦，雪地，冬
    public Graph MountainSandGraph;     // 山峦，沙地
    private Dictionary<Tuple<string, string>, Graph> MountainGraphDict = new();

    public Graph PlainGreenGraph;       // 平原，林地，春
    public Graph PlainAutumnGraph;      // 平原，林地，秋
    public Graph PlainSnowGraph;        // 平原，雪地，冬
    public Graph PlainSandGraph;        // 平原，沙地
    private Dictionary<Tuple<string, string>, Graph> PlainGraphDict = new();

    public Graph IslandGreenGraph;      // 沙洲，林地，春
    public Graph IslandAutumnGraph;     // 沙洲，林地，秋
    public Graph IslandSnowGraph;       // 沙洲，雪地，冬
    private Dictionary<Tuple<string, string>, Graph> IslandGraphDict = new();

    private Graph currentGraph;

    // water
    public GameObject crestWater;
    public OceanDepthCache depthCache;
    private float currentWaterLevel;
    private float islandWaterLevel = 50;
    private float plainWaterLevel = 30;
    private float mountainWaterLevel = 40;
    private float sandWaterLevel = -1000;

    private void InitMapMagic()
    {
        VRCamera = mainCamera.GetComponent<Camera>();

        // set mapmagic and crest
        InfinityMapMagicObject = InfinityMapMagic.GetComponent<MapMagicObject>();
        crestWater.SetActive(false);
        InfinityMapMagic.SetActive(false);

        // set graph dict
        MountainGraphDict[new("林地", "春")] = MountainGreenGraph;
        MountainGraphDict[new("林地", "夏")] = MountainGreenGraph;
        MountainGraphDict[new("林地", "秋")] = MountainAutumnGraph;
        MountainGraphDict[new("林地", "冬")] = MountainSnowGraph;
        MountainGraphDict[new("沙地", "春")] = MountainSandGraph;
        MountainGraphDict[new("沙地", "夏")] = MountainSandGraph;
        MountainGraphDict[new("沙地", "秋")] = MountainSandGraph;
        MountainGraphDict[new("沙地", "冬")] = MountainSandGraph;
        MountainGraphDict[new("雪地", "春")] = MountainSnowGraph;
        MountainGraphDict[new("雪地", "夏")] = MountainSnowGraph;
        MountainGraphDict[new("雪地", "秋")] = MountainSnowGraph;
        MountainGraphDict[new("雪地", "冬")] = MountainSnowGraph;

        PlainGraphDict[new("林地", "春")] = PlainGreenGraph;
        PlainGraphDict[new("林地", "夏")] = PlainGreenGraph;
        PlainGraphDict[new("林地", "秋")] = PlainAutumnGraph;
        PlainGraphDict[new("林地", "冬")] = PlainSnowGraph;
        PlainGraphDict[new("沙地", "春")] = PlainSandGraph;
        PlainGraphDict[new("沙地", "夏")] = PlainSandGraph;
        PlainGraphDict[new("沙地", "秋")] = PlainSandGraph;
        PlainGraphDict[new("沙地", "冬")] = PlainSnowGraph;
        PlainGraphDict[new("雪地", "春")] = PlainSnowGraph;
        PlainGraphDict[new("雪地", "夏")] = PlainSnowGraph;
        PlainGraphDict[new("雪地", "秋")] = PlainSnowGraph;
        PlainGraphDict[new("雪地", "冬")] = PlainSnowGraph;

        IslandGraphDict[new("林地", "春")] = IslandGreenGraph;
        IslandGraphDict[new("林地", "夏")] = IslandGreenGraph;
        IslandGraphDict[new("林地", "秋")] = IslandAutumnGraph;
        IslandGraphDict[new("林地", "冬")] = IslandSnowGraph;
        IslandGraphDict[new("沙地", "春")] = IslandGreenGraph;
        IslandGraphDict[new("沙地", "夏")] = IslandGreenGraph;
        IslandGraphDict[new("沙地", "秋")] = IslandAutumnGraph;
        IslandGraphDict[new("沙地", "冬")] = IslandSnowGraph;
        IslandGraphDict[new("雪地", "春")] = IslandSnowGraph;
        IslandGraphDict[new("雪地", "夏")] = IslandSnowGraph;
        IslandGraphDict[new("雪地", "秋")] = IslandSnowGraph;
        IslandGraphDict[new("雪地", "冬")] = IslandSnowGraph;
    }

    private void RefreshMapGraph(string terrain, string texture, string season)
    {
        if (terrain == "岛屿")
        {
            currentGraph = IslandGraphDict[new(texture, season)];
        }
        else if (terrain == "山峦")
        {
            currentGraph = MountainGraphDict[new(texture, season)];
        }
        else
        {
            currentGraph = PlainGraphDict[new(texture, season)];
        }

        currentGraph.random.Seed = UnityEngine.Random.Range(0, 12345);
        InfinityMapMagic.SetActive(true);
        InfinityMapMagicObject.graph = currentGraph;

        InfinityMapMagicObject.Refresh(true);

        StartCoroutine(UpdateHeightCache());
    }

    private void RefreshWater(string terrain, string texture)
    {
        // set waterLevel
        switch (terrain)
        {
            case "岛屿":
                currentWaterLevel = islandWaterLevel;
                break;
            case "平原":
                currentWaterLevel = plainWaterLevel;
                break;
            case "山峦":
                currentWaterLevel = mountainWaterLevel;
                break;
        }

        if (texture == "沙地")
        {
            currentWaterLevel = sandWaterLevel;
        }

        crestWater.SetActive(true);
        crestWater.transform.position = new Vector3(0, currentWaterLevel, 0);
    }

    private IEnumerator UpdateHeightCache()
    {
        yield return new WaitForSeconds(5);
        depthCache.transform.position = new Vector3(mainCamera.transform.position.x, crestWater.transform.position.y, mainCamera.transform.position.z);
        depthCache.PopulateCache();
    }

    public float GetCurrentWaterLevel()
    {
        if (Application.IsPlaying(gameObject))
        {
            return currentWaterLevel;
        }
        else
        {
            return crestWater.transform.position.y;
        }
    }

    private void SetTerrain(string terrain, string texture, string season)
    {
        // draw water
        RefreshWater(terrain, texture);

        // draw graph
        RefreshMapGraph(terrain, texture, season);
    }
    #endregion


    #region Time
    private void SetTime(float time)
    {
        float targetTime = time / 24f;
        float currentTime = UniStormSystem.Instance.m_TimeFloat;

        if (currentTime == targetTime)
            return;

        StartCoroutine(ChangeTime(currentTime, targetTime, 0.0005f));
    }

    IEnumerator ChangeTime(float current, float target, float speed)
    {
        float time = current;
        if (target < current)
            target += 1;

        while (time <= target)
        {
            yield return null;
            time += speed;
            UniStormSystem.Instance.m_TimeFloat = time % 1;
        }
    }

    private Dictionary<string, float> timeDictionary = new()
    {
        {"白昼", 12},
        {"清晨", 7.5f},
        {"黄昏", 18},
        {"夜晚", 24}
    };
    #endregion


    #region Weather
    // index 0 : Clear
    // index 1 : Cloudy
    // index 2 : Foggy
    // index 3 : OverCast
    // index 4 : Light Rain
    // index 5 : Rain
    // index 6 : Heavy Rain
    // index 7 : Light Snow
    // index 8 : Snow
    // index 9 : Heavy Snow
    // index 10 : Thunder Strom
    // index 11 : Thunder Snow
    // index 12 : Dust Storm
    private void SetWeather(int weatherIndex)
    {
        var targetWeather = UniStormSystem.Instance.AllWeatherTypes[weatherIndex];

        if (UniStormSystem.Instance.CurrentWeatherType == targetWeather)
            return;
        else
            UniStormSystem.Instance.ChangeWeather(targetWeather);
    }

    private Dictionary<string, int> weatherDictionary = new Dictionary<string, int>()
    {
        {"晴天", 0},
        {"多云", 1},
        {"雾", 2},
        {"阴天", 3},
        {"小雨", 4},
        {"中雨", 5},
        {"大雨", 6},
        {"小雪", 7},
        {"中雪", 8},
        {"大雪", 9},
        {"雷雨", 10},
        {"雷雪", 11},
        {"沙尘", 12}
    };
    #endregion

    // audios, emotion goto AudioSystem.cs

    // generate poem scene by params
    private IEnumerator SceneTransition()
    {
        // reset user position
        VRMoveController.Instance.ReSetPosition();

        // limit camera farClip
        int clipFar = 10;
        VRCamera.farClipPlane = clipFar;

        // guided reading
        ChatController.Instance.CreateBubble(ChatController.Instance.leftBubble, 
            $"<color=orange>诵读一遍诗词文本（无需开启语音识别），然后关闭对话面板，生成诗词情境：</color>\n{currentPoemTxt}");

        // after reading, start vr experience when closing chat panel
        yield return new WaitUntil(() => ChatController.Instance.IsChatPanelActive() == false);
        VRMoveController.Instance.SetGravity(true);

        for (int i = 0; i < 200; i++)
        {
            clipFar += 4;
            VRCamera.farClipPlane = clipFar;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void CreateScene()
    {
        StartCoroutine(GeneratePoemScene());
    }

    private IEnumerator GeneratePoemScene()
    {
        // wait until analyse finished
        yield return new WaitUntil(() => isAnalysing == false);

        if (currentPoemData == null)
        {
            Debug.LogError("the scene params is null");
            yield break;
        }

        // switch scene
        StartCoroutine(SceneTransition());

        // set terrain and textures
        Debug.Log("terrain: " + currentPoemData.terrain);
        Debug.Log("biome: " + currentPoemData.texture);
        Debug.Log("season: " + currentPoemData.season);
        SetTerrain(currentPoemData.terrain, currentPoemData.texture, currentPoemData.season);

        // set time
        Debug.Log("time: " + currentPoemData.time);
        SetTime(timeDictionary[currentPoemData.time]);

        // set weather
        Debug.Log("weather: " + currentPoemData.weather);
        SetWeather(weatherDictionary[currentPoemData.weather]);

        // set audios
        List<AudioClip> audioList = new();
        foreach (string audio in currentPoemData.audios)
        {
            audioList.Add(AudioSystem.Instance.sceneAudioDictionary[audio]);
        }
        Debug.Log(audioList);
        AudioSystem.Instance.SceneAudioPlay(audioList);

        // set emotion bgm
        Debug.Log("emotion: " + currentPoemData.emotion);
        AudioSystem.Instance.BGMAudioPlay(AudioSystem.Instance.bgmAudioDictionary[currentPoemData.emotion]);
    }


    // Start is called before the first frame update
    void Start()
    {
        // init ai
        InitAnalyseAI();

        analyseData.prompt += TreatmentData.analyse_fewshot;

        // treatment
        if (ChatController.Instance.treatmentA || ChatController.Instance.treatmentB)
        {
            //analyseData.prompt += TreatmentData.analyse_fewshot;
        }

        // init map magic
        InitMapMagic();

        // test
        // StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        GeneratorData data1 = new()
        {
            terrain = "山峦",
            texture = "林地",
            objects = null,
            season = "冬",
            time = "白昼",
            weather = "晴天",
            audios = new string[] { "钟声" },
            emotion = "孤寂惆怅"
        };

        GeneratorData data2 = new()
        {
            terrain = "岛屿",
            texture = "雪地",
            objects = null,
            season = "春",
            time = "白昼",
            weather = "小雪",
            audios = new string[] { "钟声" },
            emotion = "孤寂惆怅"
        };

        yield return new WaitForSeconds(10);
        currentPoemData = data1;
        StartCoroutine(GeneratePoemScene());

        yield return new WaitForSeconds(30);
        currentPoemData = data2;
        StartCoroutine(GeneratePoemScene());
    }
}
