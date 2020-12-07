using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace VirtualTexture
{
    [CustomEditor(typeof(RVTTerrain))]
    public class RVTTerrainEditor : Editor
	{
        public override void OnInspectorGUI()
		{
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ShowTerrain"))
            {
                var terrain = (RVTTerrain)target;
                foreach (var ter in terrain.TerrainList)
                {
                    ter.materialTemplate.DisableKeyword("_SHOWRVTMIPMAP");
                }
            }
            if (GUILayout.Button("ShowMipMap"))
            {
                var terrain = (RVTTerrain)target;
                foreach(var ter in terrain.TerrainList)
                {
                    ter.materialTemplate.EnableKeyword("_SHOWRVTMIPMAP");
                }
            }
            if (GUILayout.Button("Rebuild"))
            {
                var terrain = (RVTTerrain)target;
                terrain.Rest();
            }
            GUILayout.EndHorizontal();
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                var terrain = (RVTTerrain)target;
                terrain.PageTable.UseFeed = terrain.UseFeed;
            }
        }
	}
}