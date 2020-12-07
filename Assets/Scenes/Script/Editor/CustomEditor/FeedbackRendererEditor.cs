using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace VirtualTexture
{
    [CustomEditor(typeof(FeedbackRenderer))]
	public class FeedbackRendererEditor : EditorBase
    {
        protected override void OnPlayingInspectorGUI()
		{
			var renderer = (FeedbackRenderer)target;
			DrawTexture(renderer.TargetTexture, "Feedback Texture");
        }
    }
}