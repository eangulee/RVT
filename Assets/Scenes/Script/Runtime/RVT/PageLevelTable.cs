using System;
using UnityEngine;

namespace VirtualTexture
{
    /// <summary>
    /// 页表节点
    /// </summary>
    public class PageLevelTable
    {
        public TableNodeCell[,] Cell { get; set; }

        public int MipLevel { get; }

        public Vector2Int pageOffset;
        public int NodeCellCount;
        public int PerCellSize;

        public PageLevelTable(int mip, int tableSize)
        {
            pageOffset = Vector2Int.zero;
            MipLevel = mip;
            PerCellSize = (int)Mathf.Pow(2, mip);
            NodeCellCount = tableSize / PerCellSize;
            Cell = new TableNodeCell[NodeCellCount, NodeCellCount];
            for (int i = 0; i < NodeCellCount; i++)
            {
                for(int j = 0; j < NodeCellCount; j++)
                {
                    Cell[i,j] = new TableNodeCell(i * PerCellSize, 
                                                  j * PerCellSize, 
                                                  PerCellSize, PerCellSize, MipLevel);
                }
            }
        }

        public void ChangeViewRect(Vector2Int offset,Action<Vector2Int> InvalidatePage)
        {
            if (Mathf.Abs(offset.x) >= NodeCellCount || Mathf.Abs(offset.y) > NodeCellCount ||
                offset.x % PerCellSize != 0 || offset.y % PerCellSize != 0)
            {
                for (int i = 0; i < NodeCellCount; i++)
                    for (int j = 0; j < NodeCellCount; j++)
                    {
                        var transXY = GetTransXY(i, j);
                        Cell[transXY.x, transXY.y].Payload.LoadRequest = null;
                        InvalidatePage(Cell[transXY.x, transXY.y].Payload.TileIndex);
                    }
                pageOffset = Vector2Int.zero;
                return;
            }
            offset.x /= PerCellSize;
            offset.y /= PerCellSize;
            #region clip map
            if (offset.x > 0)
            {
                for(int i = 0;i < offset.x; i++)
                {
                    for (int j = 0;j < NodeCellCount;j++)
                    {
                        var transXY = GetTransXY(i, j);
                        Cell[transXY.x, transXY.y].Payload.LoadRequest = null;
                        InvalidatePage(Cell[transXY.x, transXY.y].Payload.TileIndex);
                    }
                }
            }
            else if(offset.x < 0)
            {
                for(int i = 1; i <= -offset.x; i++)
                {
                    for (int j = 0; j < NodeCellCount; j++)
                    {
                        var transXY = GetTransXY(NodeCellCount - i, j);
                        Cell[transXY.x, transXY.y].Payload.LoadRequest = null;
                        InvalidatePage(Cell[transXY.x, transXY.y].Payload.TileIndex);
                    }
                }
            }
            if (offset.y > 0)
            {
                for (int i = 0; i < offset.y; i++)
                {
                    for (int j = 0; j < NodeCellCount; j++)
                    {
                        var transXY = GetTransXY(j, i);
                        Cell[transXY.x, transXY.y].Payload.LoadRequest = null;
                        InvalidatePage(Cell[transXY.x, transXY.y].Payload.TileIndex);
                    }
                }
            }
            else if (offset.y < 0)
            {
                for (int i = 1; i <= -offset.y; i++)
                {
                    for (int j = 0; j < NodeCellCount; j++)
                    {
                        var transXY = GetTransXY(j, NodeCellCount - i);
                        Cell[transXY.x, transXY.y].Payload.LoadRequest = null;
                        InvalidatePage(Cell[transXY.x, transXY.y].Payload.TileIndex);
                    }
                }
            }
            #endregion
            pageOffset += offset;
            while(pageOffset.x < 0)
            {
                pageOffset.x += NodeCellCount;
            }
            while (pageOffset.y < 0)
            {
                pageOffset.y += NodeCellCount;
            }
            pageOffset.x %= NodeCellCount;
            pageOffset.y %= NodeCellCount;
        }

        // 取x/y/mip完全一致的node，没有就返回null
        public TableNodeCell Get(int x, int y)
        {
            x /= PerCellSize;
            y /= PerCellSize;

            x = (x + pageOffset.x) % NodeCellCount;
            y = (y + pageOffset.y) % NodeCellCount;

            return Cell[x, y];
        }

        public RectInt GetInverRect(RectInt rect)
        {
            return new RectInt( rect.xMin - pageOffset.x,
                                rect.yMin - pageOffset.y,
                                rect.width,
                                rect.height);
        }

        private Vector2Int GetTransXY(int x, int y)
        {
            return new Vector2Int((x + pageOffset.x) % NodeCellCount,
                                  (y + pageOffset.y) % NodeCellCount);
        }
    }

    public class TableNodeCell
    {
        public RectInt Rect { get; set; }

        public PagePayload Payload { get; set; }

        public int MipLevel { get; }

        public TableNodeCell(int x, int y, int width, int height,int mip)
        {
            Rect = new RectInt(x, y, width, height);
            MipLevel = mip;
            Payload = new PagePayload();
        }
    }
}
