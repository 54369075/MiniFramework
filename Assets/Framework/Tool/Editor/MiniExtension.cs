﻿using UnityEditor;
using UnityEngine;

public class MiniExtension
{
    [MenuItem("Assets/获取AssetPath")]
    static void GetResAssetPath()
    {
        if (Selection.assetGUIDs == null) return;
        string assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
        TextEditor textEditor = new TextEditor();
        textEditor.text = assetPath;
        textEditor.OnFocus();
        textEditor.Copy();
    }
    [MenuItem("MiniFramework/Tool/打开StreamingAssets")]
    static void OpenStreamingAssetsFinder()
    {
        EditorUtility.RevealInFinder(Application.streamingAssetsPath);
    }
    [MenuItem("MiniFramework/Tool/打开PersisentData")]
    static void OpenPersistentDataFinder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
