using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
	/// <summary>
	/// 预渲染器类.
	/// 预渲染器使用特定的着色器渲染场景，获取当前场景用到的的虚拟贴图相关信息(页表/mipmap等级等)
	/// </summary>
    public class FeedbackRenderer : MonoBehaviour
	{
		/// <summary>
		/// 渲染目标缩放比例
		/// </summary>
		[SerializeField]
		private ScaleFactor m_Scale = default;

        /// <summary>
        /// mipmap层级偏移
        /// </summary>
        [SerializeField]
        private int m_MipmapBias = default;

        /// <summary>
        /// 预渲染摄像机
        /// </summary>
        public Camera FeedbackCamera { get; set; }

		/// <summary>
		/// 获取预渲染的贴图
		/// </summary>
        public RenderTexture TargetTexture { get; private set; }

        private void Start()
		{
			InitCamera();
        }

		/// <summary>
		/// 初始化摄像机
		/// </summary>
		private void InitCamera()
		{
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            FeedbackCamera = GetComponent<Camera>();
            if (FeedbackCamera == null)
                FeedbackCamera = gameObject.AddComponent<Camera>();
            FeedbackCamera.enabled = false;
            // 处理屏幕尺寸变换
            var scale = m_Scale.ToFloat();
            var width = (int)(mainCamera.pixelWidth * scale);
            var height = (int)(mainCamera.pixelHeight * scale);
            if (TargetTexture == null || TargetTexture.width != width || TargetTexture.height != height)
            {
                TargetTexture = new RenderTexture(width, height, 0);
                TargetTexture.useMipMap = false;
                TargetTexture.wrapMode = TextureWrapMode.Clamp;
                TargetTexture.filterMode = FilterMode.Point;

                FeedbackCamera.targetTexture = TargetTexture;


                // 设置预渲染着色器参数
                // x: 页表大小(单位: 页)
                // y: 虚拟贴图大小(单位: 像素)
                // z: 最大mipmap等级
                var tileTexture = GetComponent(typeof(TiledTexture)) as TiledTexture;
                var virtualTable = GetComponent(typeof(PageTable)) as PageTable;
                Shader.SetGlobalVector(
                    "_VTFeedbackParam",
                    new Vector4(virtualTable.TableSize,
                                virtualTable.TableSize * tileTexture.TileSize * scale,
                                virtualTable.MaxMipLevel - 1,
                                m_MipmapBias));
            }

            // 渲染前先拷贝主摄像机的相关参数
            CopyCamera(mainCamera);
		}

		/// <summary>
		/// 拷贝摄像机参数
		/// </summary>
		private void CopyCamera(Camera camera)
		{
			if(camera == null)
				return;

			// Unity的Camera.CopyFrom方法会拷贝全部摄像机参数，这不是我们想要的，所以要自己写.
			//m_FeedbackCamera.transform.position = camera.transform.position;
			//m_FeedbackCamera.transform.rotation = camera.transform.rotation;
			//m_FeedbackCamera.cullingMask = camera.cullingMask;
			//m_FeedbackCamera.projectionMatrix = camera.projectionMatrix;
			FeedbackCamera.fieldOfView = camera.fieldOfView;
			FeedbackCamera.nearClipPlane = camera.nearClipPlane;
			FeedbackCamera.farClipPlane = camera.farClipPlane;
		}
    }
}
