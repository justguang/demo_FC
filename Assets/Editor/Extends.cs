using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Extends
{

    /// <summary>
    /// Sprite 转换格式成 Texture2D
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public static Texture2D Sprite_2_Texture2D(this Sprite sprite)
    {
        //sprite为图集中的某个子Sprite对象
        var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);
        targetTex.SetPixels(pixels);
        targetTex.Apply();


        return targetTex;
    }

    /// <summary>
    /// Texture2D 转换格式成 Sprite
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static Sprite Texture2D_2_Sprite(this Texture2D texture)
    {

        //t2d为待转换的Texture2D对象
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

    }

    /// <summary>
    /// 将Texture2D 保存到 savePath 文件中
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="savePath"></param>
    public static void SaveTexture2D(this Texture2D texture, string savePath)
    {
        byte[] dataBytes = texture.EncodeToPNG();
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);
        fileStream.Write(dataBytes, 0, dataBytes.Length);
        fileStream.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
