using System;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// A tiny local listener so a sample's "scene button" in the docs can ping that scene in Unity's
    /// Project window with no setup: the docs page calls GET /ping?path=&lt;asset path&gt; on this port
    /// while the Editor is open. If nothing answers, the docs page falls back on its own (see docs.js).
    /// </summary>
    [InitializeOnLoad]
    internal static class SaveSystemScenePingServer
    {
        const int Port = 58357;
        static HttpListener listener;

        static SaveSystemScenePingServer()
        {
            Start();
            EditorApplication.quitting += Stop;
            // Without this, a recompile's domain reload abandons the listener's background thread instead
            // of releasing the port, so the next reload's Start() fails to bind and the stale thread (still
            // running the old code) silently keeps answering pings.
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
        }

        static void Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                listener.Start();
                listener.BeginGetContext(OnContext, null);
            }
            catch (Exception)
            {
                // Another Editor instance already owns the port, or HttpListener isn't available here.
                // The scene button still works; it just won't be able to ping this instance.
                listener = null;
            }
        }

        static void Stop()
        {
            try
            {
                listener?.Stop();
            }
            catch (Exception)
            {
                // Already stopped.
            }
            listener = null;
        }

        static void OnContext(IAsyncResult result)
        {
            HttpListenerContext context;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (Exception)
            {
                return;
            }
            listener?.BeginGetContext(OnContext, null);

            string path = context.Request.QueryString["path"];
            bool ok = !string.IsNullOrEmpty(path);
            if (ok)
            {
                // AssetDatabase and EditorGUIUtility must run on the main thread; this callback runs on a
                // thread pool thread. EditorApplication.update (self-removing) hops there more reliably
                // than delayCall, which can sit un-fired while the Editor window isn't focused.
                EditorApplication.CallbackFunction tick = null;
                tick = () =>
                {
                    EditorApplication.update -= tick;
                    PingScene(path);
                };
                EditorApplication.update += tick;
            }

            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            byte[] body = Encoding.UTF8.GetBytes(ok ? "ok" : "missing path");
            context.Response.StatusCode = ok ? 200 : 400;
            context.Response.ContentLength64 = body.Length;
            context.Response.OutputStream.Write(body, 0, body.Length);
            context.Response.OutputStream.Close();
        }

        static void PingScene(string assetPath)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                SaveLog.Warning($"[SaveSystem] The docs' scene button couldn't find '{assetPath}'.");
                return;
            }
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
