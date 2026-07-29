using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PigFarmGame : MonoBehaviour
{
    class Pig { public string name; public int hp, weight; }
    readonly string[] seasons = { "春", "夏", "秋", "冬" };
    readonly string[] actionNames = { "商店", "养猪", "繁殖", "卖猪" };
    readonly List<Pig> pigs = new List<Pig>();
    readonly Dictionary<int, int> uses = new Dictionary<int, int>();
    Font font; Transform root, actionArea; Text status, task, farm, message; Button next;
    int season, round, coins = 18, actions, pigId = 2, taskType, taskGoal, taskReward;

    void Awake()
    {
        font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 28);
        pigs.Add(new Pig { name = "小粉", hp = 72, weight = 35 });
        pigs.Add(new Pig { name = "花花", hp = 68, weight = 31 });
        BuildUI(); ShowIntro();
    }

    GameObject Panel(string n, Transform p, Color c, Vector2 min, Vector2 max)
    {
        var g = new GameObject(n, typeof(RectTransform), typeof(Image)); g.transform.SetParent(p, false);
        var r = (RectTransform)g.transform; r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero;
        g.GetComponent<Image>().color = c; return g;
    }
    Text Label(string n, Transform p, string v, int size, TextAnchor align, Color c, Vector2 min, Vector2 max)
    {
        var g = new GameObject(n, typeof(RectTransform), typeof(Text)); g.transform.SetParent(p, false);
        var r = (RectTransform)g.transform; r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero;
        var t = g.GetComponent<Text>(); t.font = font; t.text = v; t.fontSize = size; t.alignment = align; t.color = c;
        t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = size; return t;
    }
    Button Btn(string n, Transform p, string v, Color c, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction click)
    {
        var g = Panel(n, p, c, min, max); var b = g.AddComponent<Button>(); b.onClick.AddListener(click);
        var cb = b.colors; cb.normalColor = c; cb.highlightedColor = Color.Lerp(c, Color.white, .2f); cb.pressedColor = Color.Lerp(c, Color.black, .25f); b.colors = cb;
        Label("Label", g.transform, v, 25, TextAnchor.MiddleCenter, Color.white, new Vector2(.04f, .08f), new Vector2(.96f, .92f)); return b;
    }

    void BuildUI()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        var ui = new GameObject("RuntimeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        ui.transform.SetParent(transform, false);
        var canvas = ui.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = Camera.main; canvas.planeDistance = 1; canvas.sortingOrder = 100;
        var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        root = ui.transform;
        Panel("Background", root, new Color(.02f, .07f, .055f), Vector2.zero, Vector2.one);
        status = Label("Status", root, "", 28, TextAnchor.MiddleLeft, Color.white, new Vector2(.03f, .90f), new Vector2(.97f, .99f));
        var side = Panel("TaskPanel", root, new Color(.10f, .23f, .16f), new Vector2(.02f, .18f), new Vector2(.25f, .88f));
        Label("TaskTitle", side.transform, "本季任务", 30, TextAnchor.MiddleLeft, new Color(.98f, .75f, .3f), new Vector2(.08f, .82f), new Vector2(.92f, .98f));
        task = Label("Task", side.transform, "", 23, TextAnchor.UpperLeft, Color.white, new Vector2(.08f, .08f), new Vector2(.92f, .82f));
        var center = Panel("FarmPanel", root, new Color(.88f, .86f, .72f), new Vector2(.27f, .31f), new Vector2(.98f, .88f));
        Label("FarmTitle", center.transform, "猪舍概况", 31, TextAnchor.MiddleLeft, new Color(.08f, .18f, .12f), new Vector2(.04f, .84f), new Vector2(.96f, .98f));
        farm = Label("Farm", center.transform, "", 24, TextAnchor.UpperLeft, new Color(.1f, .2f, .14f), new Vector2(.04f, .30f), new Vector2(.96f, .84f));
        message = Label("Message", center.transform, "欢迎来到养猪牧场！", 23, TextAnchor.UpperLeft, new Color(.25f, .32f, .25f), new Vector2(.04f, .06f), new Vector2(.96f, .27f));
        actionArea = Panel("ActionArea", root, new Color(.045f, .13f, .09f), new Vector2(.25f, 0), new Vector2(1, .28f)).transform;
        next = Btn("Next", actionArea, "结束回合", new Color(.83f, .42f, .17f), new Vector2(.79f, .14f), new Vector2(.97f, .46f), EndRound);
        if (!FindObjectOfType<EventSystem>()) { var e = new GameObject("FarmEventSystem"); e.AddComponent<EventSystem>(); e.AddComponent<StandaloneInputModule>(); }
    }

    void ShowIntro()
    {
        var o = Panel("Intro", root, new Color(.02f, .07f, .055f, .98f), Vector2.zero, Vector2.one);
        Label("Title", o.transform, "养猪牧场", 82, TextAnchor.MiddleCenter, new Color(.97f, .82f, .36f), new Vector2(.2f, .60f), new Vector2(.8f, .80f));
        Label("Desc", o.transform, "四季轮转，每季 4 回合并完成随机任务\n管理金币，养育、繁殖并出售猪只", 29, TextAnchor.MiddleCenter, Color.white, new Vector2(.2f, .42f), new Vector2(.8f, .60f));
        Btn("Start", o.transform, "开始经营", new Color(.15f, .46f, .27f), new Vector2(.39f, .27f), new Vector2(.61f, .37f), () => { Destroy(o); BeginSeason(); });
    }
    void BeginSeason()
    {
        round = 1; taskType = Random.Range(0, 3); taskGoal = taskType == 0 ? 4 + season : taskType == 1 ? 75 + season * 3 : 30 + season * 10; taskReward = 12 + season * 4;
        message.text = seasons[season] + "季开始，新任务已发布！"; BeginRound();
    }
    int Progress()
    {
        if (taskType == 0) return pigs.Count; if (taskType == 2) return coins; if (pigs.Count == 0) return 0;
        int sum = 0; foreach (var p in pigs) sum += p.hp; return sum / pigs.Count;
    }
    void BeginRound() { actions = Random.Range(2, 4); uses.Clear(); for (int i = 0; i < 4; i++) uses[i] = Random.Range(1, 3); BuildActions(); Refresh(); }
    void BuildActions()
    {
        var old = actionArea.Find("Buttons"); if (old) Destroy(old.gameObject); var h = Panel("Buttons", actionArea, Color.clear, new Vector2(.03f, .12f), new Vector2(.76f, .75f));
        Color[] colors = { new Color(.35f, .55f, .76f), new Color(.28f, .62f, .38f), new Color(.65f, .39f, .73f), new Color(.78f, .48f, .25f) };
        for (int i = 0; i < 4; i++) { int a = i; var b = Btn("Action" + i, h.transform, actionNames[i] + "  " + uses[i] + "次", colors[i], new Vector2(i * .245f, 0), new Vector2(i * .245f + .225f, 1), () => Act(a)); b.interactable = actions > 0 && uses[i] > 0; }
    }
    void Act(int a)
    {
        if (actions == 0 || uses[a] == 0) return;
        if (a == 0) { int cost = Random.Range(4, 9); if (coins >= cost) { coins -= cost; foreach (var p in pigs) p.hp = Mathf.Min(100, p.hp + Random.Range(4, 10)); message.text = "购买优质饲料 -" + cost + " 金币，全体恢复健康。"; } else message.text = "金币不足，本次行动仍然消耗。"; }
        if (a == 1) { if (pigs.Count == 0) message.text = "猪舍为空。"; else { var p = pigs[Random.Range(0, pigs.Count)]; int w = Random.Range(4, 10); p.weight += w; p.hp = Mathf.Min(100, p.hp + Random.Range(3, 8)); message.text = "照料 " + p.name + "，体重 +" + w + "。"; } }
        if (a == 2) { if (pigs.Count < 2) message.text = "至少需要两只猪。"; else if (coins < 6) message.text = "繁育需要 6 金币。"; else { coins -= 6; pigs.Add(new Pig { name = "小猪" + (++pigId), hp = Random.Range(65, 86), weight = Random.Range(16, 24) }); message.text = "新猪出生！繁育费用 -6 金币。"; } }
        if (a == 3) { if (pigs.Count <= 1) message.text = "至少保留一只猪。"; else { var p = pigs[pigs.Count - 1]; int price = 8 + p.weight / 2 + p.hp / 10; pigs.Remove(p); coins += price; message.text = "出售 " + p.name + "，获得 " + price + " 金币。"; } }
        uses[a]--; actions--; BuildActions(); Refresh();
    }
    void EndRound()
    {
        if (round < 4) { round++; foreach (var p in pigs) p.hp = Mathf.Max(20, p.hp - Random.Range(1, 5)); message.text = "夜幕降临，第 " + round + " 回合开始。"; BeginRound(); return; }
        bool done = Progress() >= taskGoal; if (done) { coins += taskReward; message.text = "完成本季任务，奖励 " + taskReward + " 金币！"; } else message.text = "本季任务未完成。";
        if (season == 3) { ShowEnd(); return; } season++; ShowTransition();
    }
    void ShowTransition()
    {
        var o = Panel("Transition", root, new Color(.02f, .06f, .05f, .98f), Vector2.zero, Vector2.one);
        Label("Title", o.transform, seasons[season - 1] + "季结束", 64, TextAnchor.MiddleCenter, new Color(.96f, .75f, .28f), new Vector2(.2f, .62f), new Vector2(.8f, .80f));
        Label("Result", o.transform, message.text + "\n即将进入 " + seasons[season] + "季", 29, TextAnchor.MiddleCenter, Color.white, new Vector2(.2f, .40f), new Vector2(.8f, .62f));
        Btn("Continue", o.transform, "进入" + seasons[season] + "季", new Color(.16f, .47f, .27f), new Vector2(.39f, .25f), new Vector2(.61f, .35f), () => { Destroy(o); BeginSeason(); });
    }
    void ShowEnd()
    {
        var o = Panel("Ending", root, new Color(.02f, .06f, .05f, .98f), Vector2.zero, Vector2.one); string rank = coins >= 100 ? "传奇牧场主" : coins >= 60 ? "优秀牧场主" : "新手牧场主";
        Label("Title", o.transform, "一年经营结束", 68, TextAnchor.MiddleCenter, new Color(.96f, .75f, .28f), new Vector2(.2f, .65f), new Vector2(.8f, .82f));
        Label("Score", o.transform, rank + "\n最终金币：" + coins + "    猪只：" + pigs.Count, 30, TextAnchor.MiddleCenter, Color.white, new Vector2(.2f, .40f), new Vector2(.8f, .65f));
        Btn("Restart", o.transform, "重新开始", new Color(.16f, .47f, .27f), new Vector2(.39f, .25f), new Vector2(.61f, .35f), () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
    }
    void Refresh()
    {
        status.text = seasons[season] + "季 · 第 1 年      第 " + round + " / 4 回合      剩余行动 " + actions + "             金币 " + coins + "      猪只 " + pigs.Count;
        string tn = taskType == 0 ? "猪舍兴旺" : taskType == 1 ? "健康养殖" : "资金积累"; task.text = tn + "\n\n目标进度  " + Mathf.Min(taskGoal, Progress()) + " / " + taskGoal + "\n奖励  " + taskReward + " 金币\n\n每季 4 回合\n每回合随机 2–3 次行动";
        farm.text = pigs.Count == 0 ? "猪舍目前为空" : string.Join("\n", pigs.ConvertAll(p => "● " + p.name + "    健康 " + p.hp + "    体重 " + p.weight + "kg").ToArray());
        next.GetComponentInChildren<Text>().text = round < 4 ? "结束回合" : "进行季节结算";
    }
}
