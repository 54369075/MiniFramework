﻿using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MiniFramework
{
    public class AssetBundleEditor : EditorWindow
    {
        private BuildTarget platform;
        private BuildAssetBundleOptions option;
        private string version;
        private string path;
        private Dictionary<string, AssetBundleBuild> assetBundleBuilds = new Dictionary<string, AssetBundleBuild>();
        private string[] bundles;
        private List<string> selectBunldes;
        private Vector2 scrollPos;
        [MenuItem("MiniFramework/AssetBundle")]
        public static void OpenWindow()
        {
            AssetBundleEditor window = (AssetBundleEditor)EditorWindow.GetWindow(typeof(AssetBundleEditor), false, "AssetBundle");
            window.Show();
        }
        private void Awake()
        {
            platform = (BuildTarget)PlayerPrefs.GetInt("Mini_Platform", 5);
            option = (BuildAssetBundleOptions)PlayerPrefs.GetInt("Mini_Option", 256);
            version = PlayerPrefs.GetString("Mini_Version", "1.0.0");
            path = PlayerPrefs.GetString("Mini_Path", Application.streamingAssetsPath);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            bundles = AssetDatabase.GetAllAssetBundleNames();
            selectBunldes = bundles.ToList();
        }
        private void OnGUI()
        {
            GUILayout.Label("保存路径");
            GUILayout.BeginHorizontal();
            path = GUILayout.TextField(path);
            if (GUILayout.Button("选择文件夹"))
            {
                string selectPath = EditorUtility.OpenFolderPanel("资源保持路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    path = selectPath;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("打包平台");
            platform = (BuildTarget)EditorGUILayout.EnumPopup(platform);

            GUILayout.Label("压缩方式");
            option = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup(option);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < bundles.Length; i++)
            {
                bool isBuild = selectBunldes.Contains(bundles[i]);
                bool isTrue = GUILayout.Toggle(isBuild, bundles[i]);

                if(isBuild!=isTrue)
                {
                    if(isTrue)
                    {
                        selectBunldes.Add(bundles[i]);
                    }
                    else
                    {
                        selectBunldes.Remove(bundles[i]);
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Label("版本信息");
            version = GUILayout.TextField(version);

            // if (GUILayout.Button("生成Hotfix字节文件"))
            // {
            //     GenHotfix();
            //     AssetDatabase.Refresh();
            // }

            if (GUILayout.Button("打包"))
            {
                AssetBundleBuild[] builds = new AssetBundleBuild[selectBunldes.Count];

                for (int i = 0; i < builds.Length; i++)
                {
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = selectBunldes[i];
                    build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(build.assetBundleName);
                    builds[i] = build;
                }
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(GetTargetPath(platform), builds, option, platform);
                Dictionary<string, Hash128> hash = FileUtil.LoadABManifest(manifest);
                CreateConfig(hash);
                AssetDatabase.Refresh();
            }
        }


        private void OnDestroy()
        {
            PlayerPrefs.SetInt("Mini_Platform", (int)platform);
            PlayerPrefs.SetInt("Mini_Option", (int)option);
            PlayerPrefs.SetString("Mini_Version", version);
            PlayerPrefs.SetString("Mini_Path", path);
        }
        /// <summary>
        /// 根据打包平台获取存放路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private string GetTargetPath(BuildTarget platform)
        {
            string outputPath = path + "/" + platform;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }

        // private void GenHotfix()
        // {
        //     string path = Application.streamingAssetsPath + "/Hotfix/Hotfix.dll";
        //     string newPath = Application.dataPath + "/Hotfix.bytes";
        //     if (File.Exists(path))
        //     {
        //         File.Copy(path, newPath, true);
        //     }
        // }
        /// <summary>
        /// 生成配置文件
        /// </summary>
        /// <param name="hash"></param>
        private void CreateConfig(Dictionary<string, Hash128> hash)
        {
            string configPath = GetTargetPath(platform) + "/config.txt";
            string content = "";
            content += "version:" + version + "\n";
            int index = 0;
            foreach (var item in hash)
            {
                index++;
                if (index < hash.Count)
                    content += item.Key + ":" + item.Value + "\n";
                else
                    content += item.Key + ":" + item.Value;
            }
            File.WriteAllText(configPath, content);
            Debug.Log("写入成功");
        }
    }
}