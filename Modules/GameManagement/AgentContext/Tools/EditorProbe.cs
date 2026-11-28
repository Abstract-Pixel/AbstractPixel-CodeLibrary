using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine;

// Shared Editor probe: windows, stills and queued input. Run with "unity command script run" (see TRAPS.md).
// Copied from the ticket 07 probes. Keep it generic: nothing tied to one ticket.
public static class EditorProbe
{
    static EditorWindow Find(string typeName)
    {
        foreach (EditorWindow w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            if (w.GetType().Name == typeName) return w;
        return null;
    }

    public static string Open(string typeName, string menuPath, int width, int height)
    {
        EditorWindow w = Find(typeName);
        if (w != null) w.Close();
        EditorApplication.ExecuteMenuItem(menuPath);
        w = Find(typeName);
        if (w == null) return "not open: " + typeName;
        w.position = new Rect(60, 80, width, height);
        w.Focus();
        return "opened " + w.position;
    }

    public static string Resize(string typeName, int width, int height)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        w.position = new Rect(60, 80, width, height);
        return "resized " + w.position;
    }

    public static string CloseWin(string typeName)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        w.Close();
        return "closed";
    }

    public static string Grab(string typeName, string outPath)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        float scale = EditorGUIUtility.pixelsPerPoint;
        Rect rect = w.position;
        int width = Mathf.RoundToInt(rect.width * scale);
        int height = Mathf.RoundToInt(rect.height * scale);
        Color[] pixels = InternalEditorUtility.ReadScreenPixel(new Vector2(rect.x * scale, rect.y * scale), width, height);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(outPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        return "saved " + outPath + " " + width + "x" + height;
    }

    static T Dev<T>() where T : InputDevice
    {
        var d = InputSystem.devices.OfType<T>().FirstOrDefault();
        return d != null ? d : InputSystem.AddDevice<T>();
    }

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
}
