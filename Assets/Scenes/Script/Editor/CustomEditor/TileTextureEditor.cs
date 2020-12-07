using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace VirtualTexture
{
    [CustomEditor(typeof(TiledTexture))]
	public class TileTextureEditor : EditorBase
	{
		protected override void OnPlayingInspectorGUI()
        {
            var tileTexture = (TiledTexture)target;
            
            DrawTexture(tileTexture.VTRTs[0], "Diffuse tex");
        }
    }
}