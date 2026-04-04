using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExporter
{
    [MenuItem("Assets/Export Sliced Sprites")]
    static void ExportSprites()
    {
        // 获取选中的贴图
        Texture2D sourceTex = Selection.activeObject as Texture2D;
        if (sourceTex == null) return;

        // 找到该贴图下所有的子 Sprite
        string path = AssetDatabase.GetAssetPath(sourceTex);
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);

        // 创建输出目录
        string outDir = Path.Combine(Path.GetDirectoryName(path), sourceTex.name + "_Exported");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        foreach (var obj in sprites)
        {
            if (obj is Sprite sprite)
            {
                // 创建一个匹配 Sprite 大小的新纹理
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.textureRect.x, 
                    (int)sprite.textureRect.y, 
                    (int)sprite.textureRect.width, 
                    (int)sprite.textureRect.height);
                
                newText.SetPixels(pixels);
                newText.Apply();

                // 编码并保存
                byte[] bytes = newText.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(outDir, sprite.name + ".png"), bytes);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("导出完成！路径: " + outDir);
    }
}