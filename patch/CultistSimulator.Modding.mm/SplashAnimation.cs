﻿using System;
using MonoMod;
using System.IO;
using UnityEngine;

#pragma warning disable CS0626

namespace CultistSimulator.Modding.mm
{
    [MonoModPatch("global::SplashAnimation")]
    public class SplashAnimation
    {
        private extern void orig_Start();
        
        private void Start()
        {
            orig_Start();
            string path = Application.persistentDataPath + "/config.ini";
            string text = File.ReadAllText(path);
            if (!text.Contains("export=1"))
            {
                return;
            }
	        ExportAssetsToFileSystem();
        }
        
        private static void ExportAssetsToFileSystem()
		{	
			// Export art
			ExportAssetFolderToFileSystem<Sprite>("burnImages/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("cardBacks/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("elementArt/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("elementArt/anim/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("endingArt/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("icons40/aspects/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("icons100/legacies/", GetSpriteAsPng, "png");
			ExportAssetFolderToFileSystem<Sprite>("icons100/verbs/", GetSpriteAsPng, "png");
		}
	
		private static void ExportAssetFolderToFileSystem<T>(string sourceFolder, Func<T, byte[]> bytesFunc, string ext) where T : UnityEngine.Object
		{
			string exportFolder = Path.Combine(EXPORT_DIR, sourceFolder);
			Directory.CreateDirectory(exportFolder);
			foreach (var asset in Resources.LoadAll<T>(sourceFolder))
			{
				string exportPath = Path.Combine(exportFolder, asset.name + "." + ext);
				File.WriteAllBytes(exportPath, bytesFunc(asset));
				Resources.UnloadAsset(asset);
			}
		}
	
		private static byte[] GetSpriteAsPng(Sprite sprite)
		{
			// Copy the texture so that its data can be manipulated
			// Source: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
			RenderTexture tmp = RenderTexture.GetTemporary(
				sprite.texture.width,
				sprite.texture.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear);
			Graphics.Blit(sprite.texture, tmp);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = tmp;
			Texture2D spriteTexture = new Texture2D(sprite.texture.width, sprite.texture.height);
			spriteTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
			spriteTexture.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(tmp);
			
			// Crop the texture to the sprite
			Rect r = sprite.textureRect;
			Texture2D croppedTextured = spriteTexture.CropTexture((int) r.x, (int) r.y, (int) r.width, (int) r.height);
			return croppedTextured.EncodeToPNG();
		}
	    
	    private static readonly string EXPORT_DIR = Application.streamingAssetsPath;
    }
}