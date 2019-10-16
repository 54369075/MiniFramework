﻿using UnityEditor;
using UnityEngine;
namespace MiniFramework
{
    public class SpriteFont
    {
        [MenuItem("Assets/SpriteFont")]
        static void CreateFont()
        {
            if (Selection.assetGUIDs == null) return;
            string assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            string assetDir = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Texture2D tex = sprites[0] as Texture2D;
            string fontPath = assetDir + "/" + Selection.objects[0].name + ".fontsettings";
            string matPath = assetDir + "/" + Selection.objects[0].name + ".mat";

            Material mat = new Material(Shader.Find("GUI/Text Shader"));
            mat.SetTexture("_MainTex", tex);
            AssetDatabase.CreateAsset(mat, matPath);

            Font myFont = new Font();
            myFont.material = mat;
            CharacterInfo[] characterInfo = new CharacterInfo[sprites.Length - 1];
            for (int i = 1; i < sprites.Length; i++)
            {
                Sprite spr = sprites[i] as Sprite;
                CharacterInfo info = new CharacterInfo();
                //设置ascii码，使用切分sprite的最后一个字母
                info.index = (int)spr.name[spr.name.Length - 1];
                Rect rect = spr.rect;
                //设置字符映射到材质上的坐标
                info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                //设置字符顶点的偏移位置和宽高
                info.minX = 0;
                info.maxX = (int)rect.width;
                info.minY = -(int)rect.height + (int)rect.height / 2;
                info.maxY = (int)rect.height / 2;
                //设置字符的宽度
                info.advance = (int)rect.width;
                characterInfo[i - 1] = info;
            }
            myFont.characterInfo = characterInfo;
            AssetDatabase.CreateAsset(myFont, fontPath);
            Debug.Log("创建字体成功");
        }
    }
}