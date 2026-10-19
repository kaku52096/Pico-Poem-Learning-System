# Pico Poem Learning System

在 PICO 头显里，以对话形式了解、赏析中国古诗词，再按诗词意象自动生成可体验的三维场景。

## 体验流程

在虚拟现实环境中使用手势与语音进行控制、输入。

左手使用点赞手势开关交互面板

<img src="docs/images/handpose.png" width="640">

右手启用点赞手势时，可输入语音；结束手势后自动转为文字

<img src="docs/images/mainmenu.png" width="640">

用户与诗词学习助手对话时，系统生成赏析并判断当前讨论诗词是否适合生成虚拟现实场景，同时缓存地形、天气、声音等参数。当用户想要体验此场景时，若缓存已就绪，系统按参数程序化生成地形、水体、建筑、物品、天气、声音等。

<img src="docs/images/jiangxue.png" width="640">

> 千山鸟飞绝，  
> 万径人踪灭。  
> 孤舟蓑笠翁，  
> 独钓寒江雪。  
>
> ——〔唐〕柳宗元《江雪》

<img src="docs/images/mushan.png" width="640">

> 渡口、向晚，乘瘦马、陟平冈。西郊又送秋光。对暮山横翠，衬残叶飘黄。凭高念远，素景楚天，无处不凄凉。
> 
> ——〔宋〕柳永《临江仙引·渡口》

场景生成后，从空中下落进入场景。使用摆臂移动在场景中行走探索。

## 功能模块

- 手势与射线：[VRMoveController.cs](Assets/Scripts/VRCController/VRMoveController.cs) 根据双手位移做摆臂移动；界面点击和滑动使用 XR Interaction Toolkit；自定义手势使用 PICO Hand Pose。

- 语音：[ASR.cs](Assets/Scripts/SpeechRecog/ASR.cs) 在手势开始和结束之间录音，[WavUtility.cs](Assets/Scripts/SpeechRecog/WavUtility.cs) 把音频存成 wav，再交给 [asr_server.py](SpeechRecog/WebAPI/asr_server.py)。服务端用 FastAPI 接收文件，用 SenseVoice 转成文本。

- 大模型：[DeepSeekAPI.cs](Assets/Scripts/ChatUI/DeepSeekAPI.cs) 调用 DeepSeek 进行对话；[ChatController.cs](Assets/Scripts/ChatUI/ChatController.cs) 负责诗词赏析、语音识别纠错，以及场景参数分析。

- 界面：[RoundedImage.cs](Assets/Scripts/ChatUI/RoundedImage.cs) 按气泡尺寸绘制圆角矩形；交互内容放在场景里的 `UICanvas`。

- 场景：[SceneGenerator.cs](Assets/Scripts/SceneGenerator/SceneGenerator.cs) 按参数选择 Map Magic 图并生成地形，同时驱动水体、天气和声音；参数定义在 [SceneGeneratorData.cs](Assets/Scripts/SceneGenerator/SceneGeneratorData.cs)；[AudioSystem.cs](Assets/Scripts/ChatUI/AudioSystem.cs) 播放系统提示音、诗词物象声和情绪配乐。

场景参数取值：

- 地形：山峦、岛屿、平原

- 地表：林地、雪地、沙地

- 景物：渔舟、水榭、长亭、寺庙、村落、营帐

- 季节：春、夏、秋、冬

- 时段：白昼、清晨、黄昏、夜晚

- 天气：晴天、多云、雾、阴天、小雨、中雨、大雨、小雪、中雪、大雪、雷雨、雷雪、沙尘

- 声音：鼓声、钟声、琴声、笛声、鸡鸣、犬吠、猿啼、马蹄声、乌啼、鸟鸣、雁鸣、蝉鸣、虫声

- 情绪配乐：宁静恬淡、豪迈旷达、轻快明朗、孤寂惆怅、苍凉悲壮

## 环境配置

- Unity `2022.3.50f1c1`
- PICO 4 系列头显，以及 PICO Developer Center
- [PICO Unity Integration SDK](https://developer-cn.picoxr.com/resources/#sdk) 3.1.0
- PICO Unity Live Preview Plugin 1.0.5

用于生成地形的插件 Map Magic 无法在安卓环境运行，因此程序在 PC 上运行，通过 Live Preview 有线串流到头显。Live Preview 串流本身不带音频输出，请在 Windows 声音设置中把麦克风和扬声器选为有线耳机或蓝牙耳机。PICO 4 上请只运行 PDC 完成串流；PDC 与 PICO 官方串流软件同时开启时会报错。

### PICO SDK 路径

[Packages/manifest.json](Packages/manifest.json) 里下面两个包指向开发机上的绝对路径。请从 PICO 开发者网站下载相同版本，安装后把路径改成自己电脑上的目录：

```json
"com.unity.pico.livepreview": "file:D:/Dev/Unity/SDK/Unity Live Preview Plugin-1.0.5-20250211",
"com.unity.xr.picoxr": "file:D:/Dev/Unity/SDK/PICO Unity Integration SDK_3.1.0_20250109"
```

### DeepSeek API Key

打开 [DeepSeekAPI.cs](Assets/Scripts/ChatUI/DeepSeekAPI.cs)，把 `apiKey` 的 `"<>"` 换成自己的 Key。

## 运行步骤

1. 创建语音识别环境。进入 `SpeechRecog/SenseVoice` 后执行：

```shell
conda create -n sensevoice python=3.10
conda activate sensevoice
pip install -r requirements.txt
```

SenseVoice 与 VAD 模型已放在 `SpeechRecog/SenseVoice/iic/`。

2. 启动服务：

```shell
conda activate sensevoice
cd SpeechRecog/WebAPI
python asr_server.py
```

浏览器打开 `http://127.0.0.1:5000/`，页面表明服务已开启即可。终端出现 `Uvicorn running on http://0.0.0.0:5000` 时，进程已在监听。

3. 打开 PICO Developer Center，用数据线连接头显并完成串流。

4. 在 Unity 编辑器中进入 `Assets/Scenes/MainScene`，点击播放。戴上头显后按交互面板上的引导体验。

## 场景与目录

系统只有主场景 `MainScene`，赏析、语音和场景生成都在这一场景里完成。

<img src="docs/images/mainscene.png" alt="主场景层级" width="640">

- `XR Interaction Setup` 中的 `XR Origin`：定位、手势、射线和摄像机
- `UICanvas`：聊天与引导界面
- `RunTime`：`ASR` 负责语音，`DS` 负责大模型，`AudioSystem` 负责提示音、物象声和背景音乐，`UniStorm VR System` 负责时间和天气，`Ocean` 是 Crest 水体，`InfinityMapMagic` 负责地形，`SceneGenerator` 按参数驱动这些模块
- `HandPoseGenerator`：PICO SDK 提供的自定义手势组件

自研代码和资源的位置：

- [Assets/Scripts](Assets/Scripts)：功能脚本。`ChatUI` 负责对话、大模型和界面，`SpeechRecog` 负责录音与识别请求，`SceneGenerator` 负责场景生成，`VRCController` 负责摆臂移动
- [Assets/SceneGenerator](Assets/SceneGenerator)：Map Magic 图、地表与景物预制体
- [Assets/Scenes](Assets/Scenes)：主场景
- `Assets/Audio`、`Assets/Font`、`Assets/HandPose`、`Assets/Prefabs`：音频、字体、自定义手势和预制体
- [SpeechRecog](SpeechRecog)：`WebAPI/asr_server.py` 是本地语音服务，`SenseVoice` 是识别代码与模型

`Assets/Packages` 中是随工程放入的第三方资源，包括 Map Magic 2、Crest、UniStorm，以及植被和建筑模型。这些资源各自带有许可，本仓库的说明不代替原作者的授权。
