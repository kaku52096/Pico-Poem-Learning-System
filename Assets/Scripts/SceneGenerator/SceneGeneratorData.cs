using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GeneratorData
{
    public string terrain;
    public string texture;
    public string[] objects;
    public string season;
    public string time;
    public string weather;
    public string[] audios;
    public string emotion;
}

public static class GeneratorDataValue
{
    public static string[] terrains = {"É½ÂÍ", "µºÓì", "Æ½Ô­"};
    public static string[] textures = {"ÁÖµØ", "Ñ©µØ", "É³µØ"};
    public static string[] objects = {"ÓæÖÛ", "Ë®é¿", "³¤Í¤", "ËÂÃí", "´åÂä", "ÓªÕÊ"};
    public static string[] seasons = {"´º", "ÏÄ", "Çï", "¶¬"};
    public static string[] times = {"°×Öç", "Çå³¿", "»Æ»è", "Ò¹Íí"};
    public static string[] weathers = {"ÇçÌì", "¶àÔÆ", "Îí", "ÒõÌì", "Ğ¡Óê", "ÖĞÓê", "´óÓê", "Ğ¡Ñ©", "ÖĞÑ©", "´óÑ©", "À×Óê", "À×Ñ©", "É³³¾"};
    public static string[] audios = {"¹ÄÉù", "ÖÓÉù", "ÇÙÉù", "µÑÉù", "¼¦Ãù", "È®·Í", "Ô³Ìä", "ÂíÌãÉù", "ÎÚÌä", "ÄñÃù", "ÑãÃù", "²õÃù", "³æÉù"};
    public static string[] emotions = {"Äş¾²Ìñµ­", "ºÀÂõ¿õ´ï", "Çá¿ìÃ÷ÀÊ", "¹Â¼Åã°âê", "²ÔÁ¹±¯×³"};

    public static string ToJsonList(string[] values)
    {
        string result = $"[{string.Join(", ", values.Select(s => $"{s}"))}]";
        return result;
    }
}