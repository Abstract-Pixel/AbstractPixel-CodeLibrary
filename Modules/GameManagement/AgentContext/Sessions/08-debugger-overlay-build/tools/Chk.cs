using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AbstractPixel.GameManagement;
using AbstractPixel.GameManagement.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public static class Chk
{
    public static string Info()
    {
        var s = GameManagementSettingsAssets.Find();
        return "settings=" + AssetDatabase.GetAssetPath(s) + " toggle=" + s.DebuggerToggleAction + " scene=" + EditorSceneManager.GetActiveScene().path
            + " dirty=" + EditorSceneManager.GetActiveScene().isDirty
            + " preloaded=" + string.Join(",", PlayerSettings.GetPreloadedAssets().Select(o => o == null ? "null" : o.name))
            + " inputBg=" + InputSystem.settings.backgroundBehavior + "/" + InputSystem.settings.editorInputBehaviorInPlayMode;
    }

    const string Temp = "Assets/ZZ_Trial";

    public static string Prep()
    {
        if (!AssetDatabase.IsValidFolder(Temp)) AssetDatabase.CreateFolder("Assets", "ZZ_Trial");
        File.WriteAllText(Temp + "/Dbg.inputactions", "{\"name\":\"Dbg\",\"maps\":[{\"name\":\"Debug\",\"actions\":[{\"name\":\"Toggle\",\"type\":\"Button\",\"id\":\"11111111-1111-1111-1111-111111111111\"}],\"bindings\":[{\"path\":\"<Keyboard>/backquote\",\"action\":\"Toggle\",\"id\":\"33333333-3333-3333-3333-333333333333\"}]}],\"controlSchemes\":[]}");
        AssetDatabase.ImportAsset(Temp + "/Dbg.inputactions");
        var action = AssetDatabase.LoadAllAssetsAtPath(Temp + "/Dbg.inputactions").OfType<InputActionReference>().First();
        var s = GameManagementSettingsAssets.Find();
        s.DebuggerToggleAction = action;
        EditorUtility.SetDirty(s);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        EditorSceneManager.OpenScene("Packages/com.abstractpixel.library/Modules/GameManagement/Samples/01 Minimal Game/Scenes/Minimal Game.unity");
        EditorApplication.isPlaying = true;
        return "playing soon, toggle=" + action.name;
    }

    public static string Restore()
    {
        var s = GameManagementSettingsAssets.Find();
        s.DebuggerToggleAction = null;
        EditorUtility.SetDirty(s);
        AssetDatabase.SaveAssetIfDirty(s);
        AssetDatabase.DeleteAsset(Temp);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.PointersAndKeyboardsRespectGameViewFocus;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices;
        EditorSceneManager.OpenScene("Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/10 Launch Scene/Scenes/Sample 10 Splash.unity");
        return "restored";
    }

    public static string Leave(string state) { GameStates.Leave(St(state), "check script"); return "left " + state; }
    public static string Name(string from, string to) { var x = St(from); x.name = to; return "named " + to; }

    public static string Focus(string type)
    {
        var w = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(x => x.GetType().Name == type);
        if (w == null) return "open: " + string.Join(",", Resources.FindObjectsOfTypeAll<EditorWindow>().Select(x => x.GetType().Name));
        w.Focus();
        return "focused " + EditorWindow.focusedWindow.GetType().Name + " shown=" + AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "AbstractPixel.GameManagement.Debugger").GetType("AbstractPixel.GameManagement.Debugging.DebuggerOverlay").GetProperty("IsShown").GetValue(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "AbstractPixel.GameManagement.Debugger").GetType("AbstractPixel.GameManagement.Debugging.DebuggerOverlay").GetProperty("Instance").GetValue(null));
    }

    public static string OpenScene() { EditorWindow.GetWindow<SceneView>().Focus(); return "scene view"; }
    public static string CloseScene() { EditorWindow.GetWindow<SceneView>().Close(); return "closed"; }

    public static string Spy()
    {
        var f = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.NonPublic | BindingFlags.Static);
        var old = (Action<Event>)f.GetValue(null);
        SessionState.SetString("spy", "");
        old += e => { if (e != null && e.isKey) SessionState.SetString("spy", SessionState.GetString("spy", "") + e.type + ":" + e.keyCode + " in " + (EditorWindow.focusedWindow == null ? "none" : EditorWindow.focusedWindow.GetType().Name) + "; "); };
        f.SetValue(null, old);
        return "spying";
    }
    public static string Heard() { return "heard: " + SessionState.GetString("spy", ""); }

    public static string Stop() { EditorApplication.isPlaying = false; return "stopping"; }

    static T Dev<T>() where T : InputDevice
    {
        var d = InputSystem.devices.OfType<T>().FirstOrDefault();
        return d != null ? d : InputSystem.AddDevice<T>();
    }

    public static string Key(string key)
    {
        var kb = Dev<Keyboard>();
        if (key == "up") InputSystem.QueueStateEvent(kb, new KeyboardState());
        else InputSystem.QueueStateEvent(kb, new KeyboardState((Key)Enum.Parse(typeof(Key), key)));
        return "key " + key;
    }

    public static string F8()
    {
        var game = Resources.FindObjectsOfTypeAll<EditorWindow>().First(w => w.GetType().Name == "GameView");
        game.Focus();
        bool used = game.SendEvent(new Event { type = EventType.KeyDown, keyCode = KeyCode.F8 });
        game.SendEvent(new Event { type = EventType.KeyUp, keyCode = KeyCode.F8 });
        return "sent F8 used=" + used;
    }

    static StateSO St(string n) { return GameManagementSettingsAssets.Find().Catalog.States.First(s => s.name == n); }
    public static string Req(string state) { GameStates.Request(St(state), "check script"); return Status(); }

    public static string Status()
    {
        var sb = new StringBuilder("time=" + Time.timeScale + " top=" + (GameStates.HasMachine && GameStates.Top != null ? GameStates.Top.name : "none"));
        var docs = Resources.FindObjectsOfTypeAll<UIDocument>().Where(d => d.name == "[Game State Debugger Overlay]").ToArray();
        sb.Append(" overlays=" + docs.Length);
        foreach (var d in docs)
        {
            var root = d.rootVisualElement.Q(null, "gm-game-top");
            var panel = d.rootVisualElement.Q(null, "gm-overlay");
            sb.Append(" display=" + root.resolvedStyle.display + " panel=" + panel.worldBound);
            sb.Append(" text=" + string.Join("|", panel.Query<Label>().ToList().Select(l => l.text)));
        }
        return sb.ToString();
    }

    public static string Shot(string path)
    {
        ScreenCapture.CaptureScreenshot(path);
        EditorApplication.QueuePlayerLoopUpdate();
        return "shot " + path;
    }
}
