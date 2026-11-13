using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateTextFile
{
    [MenuItem("Assets/Create/Text File")]
    private static void Create()
    {
        // Use the selected folder (or the selected asset's folder), else Assets
        string folder = "Assets";
        if (Selection.activeObject != null)
        {
            folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folder))
                folder = Path.GetDirectoryName(folder).Replace("\\", "/");
        }

        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/New Text File.txt");
        File.WriteAllText(path, "");
        AssetDatabase.ImportAsset(path);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}