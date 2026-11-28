using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public static class Drive
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

    public static string OpenSettings(string page, int width, int height)
    {
        SettingsService.OpenProjectSettings(page);
        EditorWindow w = Find("ProjectSettingsWindow");
        if (w == null) return "not open";
        w.position = new Rect(60, 80, width, height);
        w.Focus();
        return "settings " + w.position;
    }

    public static string Props(string assetPath, int width, int height)
    {
        Object o = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (o == null) return "no asset";
        EditorWindow old = Find("PropertyEditor");
        Selection.activeObject = o;
        EditorApplication.ExecuteMenuItem("Assets/Properties...");
        EditorWindow w = Find("PropertyEditor");
        if (w == null) return "not open";
        w.position = new Rect(60, 80, width, height);
        w.Focus();
        return "props " + w.position;
    }

    public static string CloseWin(string typeName)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        w.Close();
        return "closed";
    }

    public static string Resize(string typeName, int width, int height)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        w.position = new Rect(60, 80, width, height);
        return "resized " + w.position;
    }

    static VisualElement Named(EditorWindow w, string name, int index)
    {
        var all = w.rootVisualElement.Query<VisualElement>(name).ToList();
        if (all.Count <= index) return null;
        return all[index];
    }

    public static string Click(string typeName, string name, int index)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        VisualElement e = Named(w, name, index);
        if (e == null) return "no element " + name;
        var scroll = e.GetFirstAncestorOfType<ScrollView>();
        if (scroll != null) scroll.ScrollTo(e);
        Rect r = e.worldBound;
        Vector2 p = r.center;
        w.SendEvent(new Event { type = EventType.MouseDown, mousePosition = p, button = 0, clickCount = 1 });
        w.SendEvent(new Event { type = EventType.MouseUp, mousePosition = p, button = 0, clickCount = 1 });
        return "clicked " + name + " at " + p;
    }

    public static string ClickText(string typeName, string text)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        var b = w.rootVisualElement.Query<Button>().ToList().FirstOrDefault(x => x.text == text && x.resolvedStyle.display != DisplayStyle.None);
        if (b == null) return "no button " + text;
        var scroll = b.GetFirstAncestorOfType<ScrollView>();
        if (scroll != null) scroll.ScrollTo(b);
        Vector2 p = b.worldBound.center;
        w.SendEvent(new Event { type = EventType.MouseDown, mousePosition = p, button = 0, clickCount = 1 });
        w.SendEvent(new Event { type = EventType.MouseUp, mousePosition = p, button = 0, clickCount = 1 });
        return "clicked '" + text + "'";
    }

    public static string Set(string typeName, string name, int index, string value)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        VisualElement e = Named(w, name, index);
        if (e == null) return "no element " + name;
        if (e is TextField tf) { tf.value = value; return "text " + tf.value; }
        if (e is Toggle tg) { tg.value = value == "true"; return "toggle " + tg.value; }
        if (e is RadioButtonGroup rg) { rg.value = int.Parse(value); return "radio " + rg.value; }
        if (e is DropdownField dd) { dd.index = int.Parse(value); return "dropdown " + dd.index; }
        if (e is ObjectField of)
        {
            of.value = string.IsNullOrEmpty(value) ? null : AssetDatabase.LoadAssetAtPath<Object>(value);
            return "object " + (of.value == null ? "null" : of.value.name);
        }
        return "unsupported " + e.GetType().Name;
    }

    public static string Dump(string typeName)
    {
        EditorWindow w = Find(typeName);
        if (w == null) return "not open";
        var sb = new StringBuilder();
        sb.AppendLine("size " + w.position.size);
        foreach (var e in w.rootVisualElement.Query<VisualElement>().ToList())
        {
            if (e.resolvedStyle.display == DisplayStyle.None) continue;
            bool hidden = false;
            for (var p = e.parent; p != null; p = p.parent) if (p.resolvedStyle.display == DisplayStyle.None) { hidden = true; break; }
            if (hidden) continue;
            if (e is Button b) sb.AppendLine("[button] " + b.text + (b.enabledInHierarchy ? "" : " (off)") + " name=" + b.name);
            else if (e is Toggle t) sb.AppendLine("[toggle] " + t.text + " = " + t.value + (t.enabledInHierarchy ? "" : " (off)") + " name=" + t.name);
            else if (e is TextField f) sb.AppendLine("[text] " + f.label + " = " + f.value);
            else if (e is Label l && !string.IsNullOrEmpty(l.text) && !(l.parent is Toggle) && !(l.parent is Button) && !(l.parent is BaseField<string>)) sb.AppendLine("  " + l.text.Replace("\n", " "));
            else if (e is RadioButtonGroup rg) sb.AppendLine("[radio] " + rg.value + " name=" + rg.name);
            else if (e is DropdownField d) sb.AppendLine("[dropdown] " + d.value);
            else if (e is ObjectField o) sb.AppendLine("[object] " + o.label + " = " + (o.value == null ? "none" : o.value.name));
            else if (e is HelpBox hb) sb.AppendLine("[help] " + hb.text);
        }
        return sb.ToString();
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
}
