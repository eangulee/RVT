using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace VirtualTexture
{
    public class PageTable : MonoBehaviour
	{
		/// <summary>
		/// 页表尺寸.
		/// </summary>
		[SerializeField]
		private int m_TableSize = default;

        /// <summary>
        /// 调试着色器.
        /// 用于在编辑器中显示贴图mipmap等级
        /// </summary>
        [SerializeField]
        private Shader m_DebugShader = default;

        [SerializeField]
        private Shader m_DrawLookup = default;

        /// <summary>
        /// 页表层级结构
        /// </summary>
        private PageLevelTable[] m_PageTable;

        /// <summary>
        /// 当前活跃的页表
        /// </summary>
        private Dictionary<Vector2Int, TableNodeCell> m_ActivePages = new Dictionary<Vector2Int, TableNodeCell>();

        /// <summary>
        /// 导出的页表寻址贴图
        /// </summary>
        private RenderTexture m_LookupTexture;

        /// <summary>
        /// RT Job对象
        /// </summary>
        private RenderTextureJob m_RenderTextureJob;

        /// <summary>
        /// 平铺贴图对象
        /// </summary>
        private TiledTexture m_TileTexture;

        /// <summary>
        /// 调试材质.
        /// 用于在编辑器中显示贴图mipmap等级
        /// </summary>
        private Material m_DebugMaterial;

        /// <summary>
        /// 调试贴图
        /// </summary>
        public RenderTexture DebugTexture { get; private set; }

        /// <summary>
        /// 页表尺寸.
        /// </summary>
        public int TableSize { get { return m_TableSize; } }

        public bool UseFeed { get; set; } = true;
		/// <summary>
		/// 最大mipmap等级
		/// </summary>
		public int MaxMipLevel { get { return (int)Mathf.Log(TableSize, 2); } }

        private Material drawLookupMat = null;
        private Mesh mQuad;

        public void Init(RenderTextureJob job,int tileCount)
        {
            m_RenderTextureJob = job;
            m_RenderTextureJob.StartRenderJob += OnRenderJob;
            m_RenderTextureJob.CancelRenderJob += OnRenderJobCancel;

            m_LookupTexture = new RenderTexture(TableSize, TableSize, 0);
            m_LookupTexture.filterMode = FilterMode.Point;
            m_LookupTexture.wrapMode = TextureWrapMode.Clamp;

            m_PageTable = new PageLevelTable[MaxMipLevel + 1];
            for (int i = 0; i <= MaxMipLevel; i++)
            {
                m_PageTable[i] = new PageLevelTable(i, TableSize);
            }
            drawLookupMat = new Material(m_DrawLookup);
            drawLookupMat.enableInstancing = true;

            Shader.SetGlobalTexture(
                "_VTLookupTex",
                m_LookupTexture);
            Shader.SetGlobalVector(
                "_VTPageParam",
                new Vector4(
                    TableSize,
                    1.0f / TableSize,
                    MaxMipLevel,
                    0));

            InitDebugTexture(TableSize, TableSize);
            InitializeQuadMesh();

            m_TileTexture = (TiledTexture)GetComponent(typeof(TiledTexture));
            m_TileTexture.OnTileUpdateComplete += InvalidatePage;
            ((FeedbackReader)GetComponent(typeof(FeedbackReader))).OnFeedbackReadComplete += ProcessFeedback;
            ActivatePage(0, 0, MaxMipLevel);
        }

        public void Reset()
        {
            for (int i = 0; i <= MaxMipLevel; i++)
            {
                for(int j=0;j<m_PageTable[i].NodeCellCount;j++)
                    for (int k = 0; k < m_PageTable[i].NodeCellCount; k++)
                    {
                        InvalidatePage(m_PageTable[i].Cell[j,k].Payload.TileIndex);
                    }
            }
            m_ActivePages.Clear();
        }

        public void ChangeViewRect(Vector2Int offset)
        {
            for (int i = 0; i <= MaxMipLevel; i++)
            {
                m_PageTable[i].ChangeViewRect(offset, this.InvalidatePage);
            }

            ActivatePage(0, 0, MaxMipLevel);
        }

        public void UpdatePage(Vector2Int center)
        {
            if(this.UseFeed)
            {
                return;
            }

            for (int i = 0; i < TableSize; i++)
                for (int j = 0; j < TableSize; j++)
                {
                    var thisPos = new Vector2Int(i, j);
                    Vector2Int ManhattanDistance = thisPos - center;
                    int absX = Mathf.Abs(ManhattanDistance.x);
                    int absY = Mathf.Abs(ManhattanDistance.y);
                    int absMax = Mathf.Max(absX, absY);
                    int tempMipLevel = (int)Mathf.Floor(Mathf.Sqrt(2 * absMax));
                    tempMipLevel = Mathf.Clamp(tempMipLevel, 0, MaxMipLevel);
                    ActivatePage(i, j, tempMipLevel);
                }

            this.UpdateLookup();
        }

        private void InitializeQuadMesh()
        {
            List<Vector3> quadVertexList = new List<Vector3>();
            List<int> quadTriangleList = new List<int>();
            List<Vector2> quadUVList = new List<Vector2>();

            quadVertexList.Add(new Vector3(0, 1, 0.1f));
            quadUVList.Add(new Vector2(0, 1));
            quadVertexList.Add(new Vector3(0, 0, 0.1f));
            quadUVList.Add(new Vector2(0, 0));
            quadVertexList.Add(new Vector3(1, 0, 0.1f));
            quadUVList.Add(new Vector2(1, 0));
            quadVertexList.Add(new Vector3(1, 1, 0.1f));
            quadUVList.Add(new Vector2(1, 1));

            quadTriangleList.Add(0);
            quadTriangleList.Add(1);
            quadTriangleList.Add(2);

            quadTriangleList.Add(2);
            quadTriangleList.Add(3);
            quadTriangleList.Add(0);

            mQuad = new Mesh();
            mQuad.SetVertices(quadVertexList);
            mQuad.SetUVs(0, quadUVList);
            mQuad.SetTriangles(quadTriangleList, 0);
        }

        /// <summary>
        /// 处理回读数据
        /// </summary>
        private void ProcessFeedback(Texture2D texture)
        {
            if (!this.UseFeed)
            {
                return;
            }

            // 激活对应页表
            foreach (var c in texture.GetRawTextureData<Color32>())
            {
                ActivatePage(c.r, c.g, c.b);
            }
            
            this.UpdateLookup();
        }

        private class DrawPageInfo
        {
            public Rect rect;
            public int mip;
            public Vector2 drawPos;
        }

        private void UpdateLookup()
        {
            // 将页表数据写入页表贴图
            var currentFrame = (byte)Time.frameCount;
            var drawList = new List<DrawPageInfo>();
            foreach (var kv in m_ActivePages)
            {
                var page = kv.Value;
                // 只写入当前帧活跃的页表
                if (page.Payload.ActiveFrame != Time.frameCount)
                    continue;

                var table = m_PageTable[page.MipLevel];
                var offset = table.pageOffset;
                var perSize = table.PerCellSize;
                var lb = new Vector2Int((page.Rect.xMin - offset.x * perSize),
                                          (page.Rect.yMin - offset.y * perSize));
                while (lb.x < 0)
                {
                    lb.x += TableSize;
                }
                while (lb.y < 0)
                {
                    lb.y += TableSize;
                }

                drawList.Add(new DrawPageInfo() {
                    rect = new Rect(lb.x, lb.y, page.Rect.width, page.Rect.height),
                    mip = page.MipLevel,
                    drawPos = new Vector2((float)page.Payload.TileIndex.x / 255,
                    (float)page.Payload.TileIndex.y / 255),
                });
            }
            drawList.Sort((a,b) => {
                return -(a.mip.CompareTo(b.mip));
            });
            if(drawList.Count == 0)
            {
                return;
            }

            var mats = new Matrix4x4[drawList.Count];
            var pageInfos = new Vector4[drawList.Count];
            for(int i=0;i<drawList.Count;i++)
            {
                float size = drawList[i].rect.width / TableSize;
                mats[i] = Matrix4x4.TRS(
                    new Vector3(drawList[i].rect.x / TableSize, drawList[i].rect.y / TableSize), 
                    Quaternion.identity,
                    new Vector3(size, size, size));

                pageInfos[i] = new Vector4(drawList[i].drawPos.x, drawList[i].drawPos.y, drawList[i].mip / 255f,0);
            }
            Graphics.SetRenderTarget(m_LookupTexture);
            var tempCB = new CommandBuffer();
            var block = new MaterialPropertyBlock();
            block.SetVectorArray("_PageInfo", pageInfos);
            block.SetMatrixArray("_ImageMVP", mats);
            tempCB.DrawMeshInstanced(mQuad, 0, drawLookupMat,0, mats, mats.Length, block);
            Graphics.ExecuteCommandBuffer(tempCB);
            UpdateDebugTexture();
        }

        /// <summary>
        /// 激活页表
        /// </summary>
        private TableNodeCell ActivatePage(int x, int y, int mip)
        {
            if (mip > MaxMipLevel || mip < 0 || x < 0 || y < 0 || x >= TableSize || y >= TableSize)
                return null;
            // 找到当前页表
            var page = m_PageTable[mip].Get(x, y);
            if(page == null)
            {
                return null;
            }
            if(!page.Payload.IsReady)
            {
                LoadPage(x, y, page);

                //向上找到最近的父节点
                while(mip < MaxMipLevel && !page.Payload.IsReady)
                {
                    mip++;
                    page = m_PageTable[mip].Get(x, y);
                }
            }

            if (page.Payload.IsReady)
            {
                // 激活对应的平铺贴图块
                m_TileTexture.SetActive(page.Payload.TileIndex);
                page.Payload.ActiveFrame = Time.frameCount;
                return page;
            }

            return null;
        }

        /// <summary>
        /// 加载页表
        /// </summary>
        private void LoadPage(int x, int y, TableNodeCell node)
        {
            if (node == null)
                return;

            // 正在加载中,不需要重复请求
			if(node.Payload.LoadRequest != null)
				return;

            // 新建加载请求
			node.Payload.LoadRequest = m_RenderTextureJob.Request(x, y, node.MipLevel);
        }

        /// <summary>
        /// 开始渲染
        /// </summary>
        private void OnRenderJob(RenderTextureRequest request)
        {
            // 找到对应页表
            var node = m_PageTable[request.MipLevel].Get(request.PageX, request.PageY);
			if (node == null || node.Payload.LoadRequest != request)
                return;

			node.Payload.LoadRequest = null;

            var id = m_TileTexture.RequestTile();
            m_TileTexture.UpdateTile(id, request);

            node.Payload.TileIndex = id;
            m_ActivePages[id] = node;
        }

        /// <summary>
        /// 取消渲染
        /// </summary>
        private void OnRenderJobCancel(RenderTextureRequest request)
        {
            // 找到对应页表
            var node = m_PageTable[request.MipLevel].Get(request.PageX, request.PageY);
            if (node == null || node.Payload.LoadRequest != request)
                return;

            node.Payload.LoadRequest = null;
        }

        /// <summary>
        /// 将页表置为非活跃状态
        /// </summary>
		private void InvalidatePage(Vector2Int id)
        {
            if (!m_ActivePages.TryGetValue(id, out var node))
                return;

            node.Payload.ResetTileIndex();
            m_ActivePages.Remove(id);
        }

        [Conditional("ENABLE_DEBUG_TEXTURE")]
        private void InitDebugTexture(int w, int h)
        {
#if UNITY_EDITOR
            DebugTexture = new RenderTexture(w, h, 0);
            DebugTexture.wrapMode = TextureWrapMode.Clamp;
            DebugTexture.filterMode = FilterMode.Point;
#endif
        }

        [Conditional("ENABLE_DEBUG_TEXTURE")]
        private void UpdateDebugTexture()
        {
#if UNITY_EDITOR
            if (m_LookupTexture == null || m_DebugShader == null)
                return;

            if (m_DebugMaterial == null)
                m_DebugMaterial = new Material(m_DebugShader);

            DebugTexture.DiscardContents();
            Graphics.Blit(m_LookupTexture, DebugTexture, m_DebugMaterial);
#endif
        }
    }
}
