using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Changes a scene group's main scene the way the inspector does: through a SerializedObject, so it has Undo, then
    /// saves the group, so the save-time fix runs. Clearing it clears the stored name, GUID and path too.
    /// </summary>
    public static class SceneGroupMainScene
    {
        private const string ScenePropertyPath = "MainScene.scene";

        public static void Set(SceneGroup _group, SceneAsset _scene)
        {
            if (_group == null)
            {
                return;
            }
            SerializedObject groupObject = new SerializedObject(_group);
            SerializedProperty sceneProperty = groupObject.FindProperty(ScenePropertyPath);
            if (sceneProperty == null)
            {
                return;
            }

            string path = string.Empty;
            string guid = string.Empty;
            string name = string.Empty;
            if (_scene != null)
            {
                path = AssetDatabase.GetAssetPath(_scene);
                guid = AssetDatabase.AssetPathToGUID(path);
                name = _scene.name;
            }
            sceneProperty.FindPropertyRelative("sceneAsset").objectReferenceValue = _scene;
            sceneProperty.FindPropertyRelative("scenePath").stringValue = path;
            sceneProperty.FindPropertyRelative("sceneGuid").stringValue = guid;
            sceneProperty.FindPropertyRelative("sceneName").stringValue = name;
            groupObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(_group);
            AssetDatabase.SaveAssetIfDirty(_group);
        }
    }
}
