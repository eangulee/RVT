using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace VirtualTexture
{
	/// <summary>
	/// 渲染器类.
	/// </summary>
	public class RenderTextureJob 
	{
        /// <summary>
        /// 渲染完成的事件回调.
        /// </summary>
        public event Action<RenderTextureRequest> StartRenderJob;

        /// <summary>
        /// 渲染取消的事件回调.
        /// </summary>
        public event Action<RenderTextureRequest> CancelRenderJob;

        /// <summary>
        /// 一帧最多处理几个
        /// </summary>
        [SerializeField]
		private int m_Limit = 2;

		/// <summary>
		/// 等待处理的请求.
		/// </summary>
		private List<RenderTextureRequest> m_PendingRequests = new List<RenderTextureRequest>();

        public void Update()
        {
            if (m_PendingRequests.Count <= 0)
                return;

            // 优先处理mipmap等级高的请求
            m_PendingRequests.Sort((x, y) => { return x.MipLevel.CompareTo(y.MipLevel); });

            int count = m_Limit;
            while (count > 0 && m_PendingRequests.Count > 0)
            {
                count--;
                // 将第一个请求从等待队列移到运行队列
                var req = m_PendingRequests[m_PendingRequests.Count - 1];
                m_PendingRequests.RemoveAt(m_PendingRequests.Count - 1);

                // 开始渲染
                StartRenderJob?.Invoke(req);
            }
        }

        /// <summary>
        /// 新建渲染请求
        /// </summary>
        public RenderTextureRequest Request(int x, int y, int mip)
		{
			// 是否已经在请求队列中
			foreach(var r in m_PendingRequests)
			{
				if(r.PageX == x && r.PageY == y && r.MipLevel == mip)
					return null;
			}

			// 加入待处理列表
			var request = new RenderTextureRequest(x, y, mip);
			m_PendingRequests.Add(request);

			return request;
		}

        public void ClearJob()
        {
            foreach (var r in m_PendingRequests)
            {
                CancelRenderJob?.Invoke(r);
            }

            m_PendingRequests.Clear();
        }
    }
}
