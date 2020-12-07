using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    /// <summary>
    /// 预渲染贴图回读类.
    /// 负责将预渲染RT从GPU端回读到CPU端
    /// </summary>
    public class FeedbackReader : MonoBehaviour
	{
		/// <summary>
		/// 回读完成的事件回调.
		/// </summary>
		public event Action<Texture2D> OnFeedbackReadComplete;

		/// <summary>
		/// 回读目标缩放比例
		/// </summary>
		[SerializeField]
		private ScaleFactor m_ReadbackScale = default;

		/// <summary>
		/// 缩放着色器.
		/// Feedback目标有特定的缩放逻辑，必须要通过自定义着色器来实现.
		/// 具体逻辑为:找到区域中mipmap等级最小的像素作为最终像素，其余像素抛弃.
		/// </summary>
		[SerializeField]
		private Shader m_DownScaleShader = default;

		/// <summary>
		/// 调试着色器.
		/// 用于在编辑器中显示贴图mipmap等级
		/// </summary>
		[SerializeField]
		private Shader m_DebugShader = default;

		/// <summary>
		/// 缩放材质
		/// </summary>
		private Material m_DownScaleMaterial;

		/// <summary>
		/// 缩放材质使用的Pass
		/// </summary>
		private int m_DownScaleMaterialPass;

		/// <summary>
		/// 缩小后的RT
		/// </summary>
		private RenderTexture m_DownScaleTexture;

		/// <summary>
		/// 调试材质.
		/// 用于在编辑器中显示贴图mipmap等级
		/// </summary>
		private Material m_DebugMaterial;

		/// <summary>
		/// 处理中的回读请求
		/// </summary>
		private AsyncGPUReadbackRequest m_ReadbackRequest;

		/// <summary>
		/// 回读到cpu端的贴图
		/// </summary>
		private Texture2D m_ReadbackTexture;

		/// <summary>
		/// 调试用的贴图(用于显示mipmap等级)
		/// </summary>
		public RenderTexture DebugTexture { get; private set; }

        public bool CanRead
        {
            get
            {
                return m_ReadbackRequest.done || m_ReadbackRequest.hasError;
            }
        }


        private void Start()
		{
			if (m_ReadbackScale != ScaleFactor.One)
			{
				m_DownScaleMaterial = new Material(m_DownScaleShader);

				switch(m_ReadbackScale)
				{
				case ScaleFactor.Half:
					m_DownScaleMaterialPass = 0;
					break;
				case ScaleFactor.Quarter:
					m_DownScaleMaterialPass = 1;
					break;
				case ScaleFactor.Eighth:
					m_DownScaleMaterialPass = 2;
					break;
				}
			}
        }

		/// <summary>
		/// 发起回读请求
		/// </summary>
		public void NewRequest(RenderTexture texture,bool forceRequestAndWaitComplete = false)
		{
            if (!m_ReadbackRequest.done && !m_ReadbackRequest.hasError && !forceRequestAndWaitComplete)
                return;

			// 缩放后的尺寸
			var width = (int)(texture.width * m_ReadbackScale.ToFloat());
			var height = (int)(texture.height * m_ReadbackScale.ToFloat());

			// 先进行缩放
			if (m_ReadbackScale != ScaleFactor.One)
            {
                if (m_DownScaleTexture == null || m_DownScaleTexture.width != width || m_DownScaleTexture.height != height)
                {
                    m_DownScaleTexture = new RenderTexture(width, height, 0);
                }

                m_DownScaleTexture.DiscardContents();
                Graphics.Blit(texture, m_DownScaleTexture, m_DownScaleMaterial, m_DownScaleMaterialPass);
                texture = m_DownScaleTexture;
            }

			// 贴图尺寸检测
            if (m_ReadbackTexture == null || m_ReadbackTexture.width != width || m_ReadbackTexture.height != height)
            {
                m_ReadbackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
				m_ReadbackTexture.filterMode = FilterMode.Point;
				m_ReadbackTexture.wrapMode = TextureWrapMode.Clamp;

				InitDebugTexture(width, height);
            }

            // 发起异步回读请求
            m_ReadbackRequest = AsyncGPUReadback.Request(texture);
            if(forceRequestAndWaitComplete)
            {
                m_ReadbackRequest.WaitForCompletion();
            }
        }

		/// <summary>
		/// 检测回读请求状态
		/// </summary>
		public void UpdateRequest()
		{
			if(m_ReadbackRequest.done && !m_ReadbackRequest.hasError)
			{
                // 更新数据并分发事件
                m_ReadbackTexture.GetRawTextureData<Color32>().CopyFrom(m_ReadbackRequest.GetData<Color32>());
                OnFeedbackReadComplete?.Invoke(m_ReadbackTexture);
                UpdateDebugTexture();
            }
		}

		[Conditional("ENABLE_DEBUG_TEXTURE")]
		private void InitDebugTexture(int width, int height)
		{
#if UNITY_EDITOR
            DebugTexture = new RenderTexture(width, height, 0);
			DebugTexture.filterMode = FilterMode.Point;
			DebugTexture.wrapMode = TextureWrapMode.Clamp;
#endif
        }

		[Conditional("ENABLE_DEBUG_TEXTURE")]
		protected void UpdateDebugTexture()
		{
#if UNITY_EDITOR
            if (m_ReadbackTexture == null || m_DebugShader == null)
				return;

			if(m_DebugMaterial == null)
				m_DebugMaterial = new Material(m_DebugShader);

			m_ReadbackTexture.Apply(false);

            DebugTexture.DiscardContents();
            Graphics.Blit(m_ReadbackTexture, DebugTexture, m_DebugMaterial);
#endif
        }
    }
}