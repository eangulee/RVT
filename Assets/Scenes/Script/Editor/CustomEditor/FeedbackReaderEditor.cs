using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VirtualTexture
{
    [CustomEditor(typeof(FeedbackReader))]
	public class FeedbackReaderEditor : EditorBase
	{
        protected override void OnPlayingInspectorGUI()
        {
			var reader = (FeedbackReader)target;
            
			DrawTexture(reader.DebugTexture, "Mipmap Level Debug Texture");
        }
    }
}