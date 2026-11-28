using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AbstractPixel.GameManagement;
using AbstractPixel.GameManagement.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class Play
{
    const string Folder = "Assets/ZZ_Trial/AllCopy";

    public static string Prep()
    {
        var settings = AssetDatabase.LoadAssetAtPath<GameManagementSettings>(Folder + "/GameManagementSettings.asset");
        GameManagementSettingsAssets.Use(settings);
        string json = "{\"name\":\"PlayFlowInput\",\"maps\":[{\"name\":\"Game\",\"actions\":["
            + "{\"name\":\"Pause\",\"type\":\"Button\",\"id\":\"11111111-1111-1111-1111-111111111111\"},"
            + "{\"name\":\"Back\",\"type\":\"Button\",\"id\":\"22222222-2222-2222-2222-222222222222\"}],\"bindings\":["
            + "{\"path\":\"<Keyboard>/escape\",\"action\":\"Pause\",\"id\":\"33333333-3333-3333-3333-333333333333\"},"
            + "{\"path\":\"<Gamepad>/start\",\"action\":\"Pause\",\"id\":\"44444444-4444-4444-4444-444444444444\"},"
            + "{\"path\":\"<Keyboard>/backspace\",\"action\":\"Back\",\"id\":\"55555555-5555-5555-5555-555555555555\"},"
            + "{\"path\":\"<Gamepad>/buttonEast\",\"action\":\"Back\",\"id\":\"66666666-6666-6666-6666-666666666666\"}]}],\"controlSchemes\":[]}";
        File.WriteAllText(Folder + "/PlayFlowInput.inputactions", json);
        AssetDatabase.ImportAsset(Folder + "/PlayFlowInput.inputactions");
        return "prepared " + AssetDatabase.GetAssetPath(GameManagementSettingsAssets.Find());
    }

    static StateSO St(string n) { return GameManagementSettingsAssets.Find().Catalog.States.First(s => s.name == n); }

    static void SetField(object target, string field, object value)
    {
        FieldInfo f = null;
        for (var t = target.GetType(); t != null && f == null; t = t.BaseType)
            f = t.GetField(field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        f.SetValue(target, value);
    }

    public static string Build()
    {
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        var refs = AssetDatabase.LoadAllAssetsAtPath(Folder + "/PlayFlowInput.inputactions").OfType<InputActionReference>().ToArray();
        var pause = refs.First(r => r.action.name == "Pause");
        var back = refs.First(r => r.action.name == "Back");
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        foreach (string file in Directory.GetFiles(Folder + "/Menus", "*.prefab"))
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace('\\', '/')));
        var flow = new GameObject("Game flow");
        flow.SetActive(false);
        var start = flow.AddComponent<SceneStartRequester>();
        SetField(start, "state", St("Main Menu"));
        var p = flow.AddComponent<InputToggleRequester>();
        SetField(p, "state", St("Pause")); SetField(p, "kind", StateRequestKind.Toggle); SetField(p, "gate", St("Play")); SetField(p, "action", pause);
        var b = flow.AddComponent<BackRequester>();
        SetField(b, "state", St("Settings")); SetField(b, "backAction", back);
        flow.SetActive(true);
        return "built";
    }

    public static string Status()
    {
        var sb = new StringBuilder();
        sb.Append("top=").Append(GameStates.HasMachine && GameStates.Top != null ? GameStates.Top.name : "none");
        sb.Append(" time=").Append(Time.timeScale);
        sb.Append(" cursor=").Append(Cursor.lockState).Append("/").Append(Cursor.visible);
        sb.Append(" shown=[");
        foreach (var r in Resources.FindObjectsOfTypeAll<StateMenuReactor>().Where(x => x.gameObject.scene.IsValid()))
        {
            var c = r.transform.Find("Content");
            if (c != null && c.gameObject.activeInHierarchy)
            {
                var names = c.GetComponentsInChildren<Button>(false).Select(x => x.name);
                sb.Append(r.name.Replace("(Clone)", "")).Append(":").Append(string.Join("/", names)).Append(" ");
            }
        }
        sb.Append("] selected=").Append(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "none");
        return sb.ToString();
    }

    public static string Nav()
    {
        var sb = new StringBuilder();
        foreach (var b in Resources.FindObjectsOfTypeAll<Button>().Where(x => x.gameObject.scene.IsValid() && x.gameObject.activeInHierarchy))
            sb.Append(b.name).Append(" nav=").Append(b.navigation.mode).Append(" down=").Append(b.navigation.selectOnDown != null ? b.navigation.selectOnDown.name : "none").Append(" || ");
        return sb.ToString();
    }

    public static string Journal()
    {
        var sb = new StringBuilder();
        foreach (var r in GameStates.Machine.Journal.Records.Reverse().Take(10).Reverse())
            sb.Append(r.Kind).Append(" ").Append(r.State != null ? r.State.name : "none").Append(" by ").Append(r.Requester).Append(": ").Append(r.Reason).Append(" || ");
        sb.Append(" stack=").Append(string.Join(",", GameStates.Machine.Stack.Select(s => s.name)));
        foreach (var rc in Resources.FindObjectsOfTypeAll<StateMenuReactor>().Where(x => x.gameObject.scene.IsValid()))
            sb.Append(" || ").Append(rc.name).Append(" enabled=").Append(rc.enabled).Append(" rootActive=").Append(rc.gameObject.activeInHierarchy);
        return sb.ToString();
    }

    static T Dev<T>() where T : InputDevice
    {
        var d = InputSystem.devices.OfType<T>().FirstOrDefault();
        return d != null ? d : InputSystem.AddDevice<T>();
    }

    public static string Req(string state) { GameStates.Request(St(state), "play script"); return "requested " + state; }

    public static string Key(string key)
    {
        var kb = Dev<Keyboard>();
        var k = (Key)System.Enum.Parse(typeof(Key), key);
        InputSystem.QueueStateEvent(kb, new KeyboardState(k));
        Release(kb);
        return "key " + key;
    }

    static async void Release(InputDevice d)
    {
        await System.Threading.Tasks.Task.Delay(150);
        if (d is Keyboard) InputSystem.QueueStateEvent(d, new KeyboardState());
        else if (d is Gamepad) InputSystem.QueueStateEvent(d, new GamepadState());
        else if (d is Mouse) InputSystem.QueueStateEvent(d, new MouseState { position = ((Mouse)d).position.ReadValue() });
    }

    public static string Pad(string button)
    {
        var pad = Dev<Gamepad>();
        AbstractPixel.Core.InputDeviceTracker.SetActiveDevice(pad);
        var bt = (GamepadButton)System.Enum.Parse(typeof(GamepadButton), button);
        InputSystem.QueueStateEvent(pad, new GamepadState(bt));
        Release(pad);
        return "pad " + button;
    }

    public static string Click(string menuPrefix, string buttonName)
    {
        var mouse = Dev<Mouse>();
        var reactor = Resources.FindObjectsOfTypeAll<StateMenuReactor>().First(x => x.gameObject.scene.IsValid() && x.name.StartsWith(menuPrefix));
        var button = reactor.transform.Find("Content").GetComponentsInChildren<Button>(false).FirstOrDefault(x => x.name == buttonName);
        if (button == null) return "NOT SHOWN: " + menuPrefix + "/" + buttonName;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)button.transform).position);
        InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
        Down(mouse, point);
        return "click " + menuPrefix + "/" + buttonName + " at " + point + " screen " + Screen.width + "x" + Screen.height;
    }

    static async void Down(Mouse m, Vector2 p)
    {
        await System.Threading.Tasks.Task.Delay(250);
        InputSystem.QueueStateEvent(m, new MouseState { position = p }.WithButton(MouseButton.Left));
        await System.Threading.Tasks.Task.Delay(120);
        InputSystem.QueueStateEvent(m, new MouseState { position = p });
    }
}
