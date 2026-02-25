using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public static class TreatmentData
{
    public static string contentA = 
                "第一首：《春思二首·其一》 贾至〔唐代〕草色青青柳色黄，桃花历乱李花香。\r\n东风不为吹愁去，春日偏能惹恨长。\r\n" +
                "第二首：《临江仙引·渡口》 柳永〔宋代〕渡口、向晚，乘瘦马、陟平冈。西郊又送秋光。对暮山横翠，衬残叶飘黄。凭高念远，素景楚天，无处不凄凉。\r\n香闺别来无信息，云愁雨恨难忘。指帝城归路，但烟水茫茫。凝情望断泪眼，尽日独立斜阳。\r\n" +
                "第三首：《雪望》 洪升〔清代〕寒色孤村暮，悲风四野闻。\r\n溪深难受雪，山冻不流云。\r\n鸥鹭飞难辨，沙汀望莫分。\r\n野桥梅几树，并是白纷纷。\r\n" +
                "第四首：《调笑令·边草》 戴叔伦〔唐代〕边草，边草，边草尽来兵老。\r\n山南山北雪晴，千里万里月明。明月，明月，胡笳一声愁绝。";

    public static string contentB =
                "第一首：《清平乐·雨晴烟晚》 冯延巳〔五代〕雨晴烟晚。绿水新池满。双燕飞来垂柳院，小阁画帘高卷。\r\n黄昏独倚朱阑。西南新月眉弯。砌下落花风起，罗衣特地春寒。\r\n" +
                "第二首：《酬晖上人秋夜山亭有赠》 陈子昂〔唐代〕皎皎白林秋，微微翠山静。\r\n禅居感物变，独坐开轩屏。\r\n风泉夜声杂，月露宵光冷。\r\n多谢忘机人，尘忧未能整。\r\n" +
                "第三首：《浣溪沙·半夜银山上积苏》 苏轼〔宋代〕半夜银山上积苏，朝来九陌带随车。涛江烟渚一时无。\r\n空腹有诗衣有结，湿薪如桂米如珠。冻吟谁伴捻髭须。\r\n" +
                "第四首：《碛中作》 岑参〔唐代〕走马西来欲到天，辞家见月两回圆。\r\n今夜不知何处宿，平沙万里绝人烟。\r\n";

    public static string input_poetryA1 = "input: 草色青青柳色黄，桃花历乱李花香。东风不为吹愁去，春日偏能惹恨长。";
    public static string input_poetryA2 = "input: 渡口、向晚，乘瘦马、陟平冈。西郊又送秋光。对暮山横翠，衬残叶飘黄。凭高念远，素景楚天，无处不凄凉。\r\n香闺别来无信息，云愁雨恨难忘。指帝城归路，但烟水茫茫。凝情望断泪眼，尽日独立斜阳。";
    public static string input_poetryA3 = "input: 寒色孤村暮，悲风四野闻。\r\n溪深难受雪，山冻不流云。\r\n鸥鹭飞难辨，沙汀望莫分。\r\n野桥梅几树，并是白纷纷。";
    public static string input_poetryA4 = "input: 边草，边草，边草尽来兵老。\r\n山南山北雪晴，千里万里月明。明月，明月，胡笳一声愁绝。";
    public static string input_poetryB1 = "input: 雨晴烟晚。绿水新池满。双燕飞来垂柳院，小阁画帘高卷。\r\n黄昏独倚朱阑。西南新月眉弯。砌下落花风起，罗衣特地春寒。";
    public static string input_poetryB2 = "input: 皎皎白林秋，微微翠山静。\r\n禅居感物变，独坐开轩屏。\r\n风泉夜声杂，月露宵光冷。\r\n多谢忘机人，尘忧未能整。";
    public static string input_poetryB3 = "input: 半夜银山上积苏，朝来九陌带随车。涛江烟渚一时无。\r\n空腹有诗衣有结，湿薪如桂米如珠。冻吟谁伴捻髭须。";
    public static string input_poetryB4 = "input: 走马西来欲到天，辞家见月两回圆。\r\n今夜不知何处宿，平沙万里绝人烟。";

    public static string json_poetryA1 = "output json:\n{\r\n  \"terrain\": \"平原\",\r\n  \"texture\": \"林地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"春\",\r\n  \"time\": \"白昼\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"鸟鸣\"],\r\n  \"emotion\": \"孤寂惆怅\"\r\n}";
    public static string json_poetryA2 = "output json:\n{\r\n  \"terrain\": \"山峦\",\r\n  \"texture\": \"林地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"秋\",\r\n  \"time\": \"黄昏\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"马蹄声\"],\r\n  \"emotion\": \"孤寂惆怅\"\r\n}";
    public static string json_poetryA3 = "output json:\n{\r\n  \"terrain\": \"山峦\",\r\n  \"texture\": \"雪地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"冬\",\r\n  \"time\": \"黄昏\",\r\n  \"weather\": \"大雪\",\r\n  \"audios\": [\"雁鸣\"],\r\n  \"emotion\": \"孤寂惆怅\"\r\n}";
    public static string json_poetryA4 = "output json:\n{\r\n  \"terrain\": \"山峦\",\r\n  \"texture\": \"沙地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"冬\",\r\n  \"time\": \"夜晚\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"笛声\"],\r\n  \"emotion\": \"苍凉悲壮\"}";
    public static string json_poetryB1 = "output json:\n{\r\n  \"terrain\": \"平原\",\r\n  \"texture\": \"林地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"春\",\r\n  \"time\": \"黄昏\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"鸟鸣\"],\r\n  \"emotion\": \"孤寂惆怅\"\r\n}";
    public static string json_poetryB2 = "output json:\n{\r\n  \"terrain\": \"山峦\",\r\n  \"texture\": \"林地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"秋\",\r\n  \"time\": \"夜晚\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"虫声\"],\r\n  \"emotion\": \"宁静恬淡\"\r\n}";
    public static string json_poetryB3 = "output json:\n{\r\n  \"terrain\": \"山峦\",\r\n  \"texture\": \"雪地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"冬\",\r\n  \"time\": \"夜晚\",\r\n  \"weather\": \"小雪\",\r\n  \"audios\": \"无\",\r\n  \"emotion\": \"宁静恬淡\"\r\n}";
    public static string json_poetryB4 = "output json:\n{\r\n  \"terrain\": \"平原\",\r\n  \"texture\": \"沙地\",\r\n  \"objects\": \"无\",\r\n  \"season\": \"春\",\r\n  \"time\": \"夜晚\",\r\n  \"weather\": \"晴天\",\r\n  \"audios\": [\"马蹄声\"],\r\n  \"emotion\": \"豪迈旷达\"\r\n}";

    public static string analyse_fewshot =
        input_poetryA1 + json_poetryA1 +
        input_poetryA2 + json_poetryA2 +
        input_poetryA3 + json_poetryA3 +
        input_poetryA4 + json_poetryA4 +
        input_poetryB1 + json_poetryB1 +
        input_poetryB2 + json_poetryB2 +
        input_poetryB3 + json_poetryB3 +
        input_poetryB4 + json_poetryB4;
}
