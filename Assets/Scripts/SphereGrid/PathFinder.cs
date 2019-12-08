using UnityEngine;
using System.Collections.Generic;
using System;

public class PathFinder<T> where T : IEquatable<T>
{

    #region FindPath

    internal class PathNode
    {
        public T t { get; private set; }
        public int h; //从指定的方格移动到终点的估算成本。
        public int g; //从起点移动到指定方格的移动代价
        public int f
        {
            get { return h + g; }
        }
        public T parent { get; private set; }
        public bool hasParent { get; private set; }

        public PathNode(T t)
        {
            this.t = t;
            Clear();
        }

        public void SetParent(T parent)
        {
            this.parent = parent;
            hasParent = true;
        }
        public void Clear()
        {
            h = 0;
            g = 0;
            parent = default(T);
            hasParent = false;
        }
    }

    private List<T> mOpenList = new List<T>();
    private List<T> mCloseList = new List<T>();
    private Dictionary<T, PathNode> mNodeDic = new Dictionary<T, PathNode>();

    private PathNode GetPathNode(T t, bool create = false)
    {
        PathNode node;
        if(mNodeDic.TryGetValue(t, out node) == false)
        {
            if(create)
            {
                node = new PathNode(t);

                mNodeDic.Add(t, node);
            }
        }

        return node;
      
    }

    public bool FindPath(ref List<T> result, T from, T to, Func<T, bool> isValid, Func<T, IEnumerator<T>> getNeihbors, Func<T, T, int> getCostValue)
    {
        if (from.Equals(to) || isValid == null || getNeihbors == null || getCostValue == null)
        {
            Debug.LogError("参数不能为空");
            return false;
        }

        result.Clear();

        mOpenList.Clear();
        mCloseList.Clear();

        var it = mNodeDic.GetEnumerator();
        while (it.MoveNext())
        {
            it.Current.Value.Clear();
        }


        //将起点作为待处理的点放入开启列表，
        mOpenList.Add(from);

        //如果开启列表没有待处理点表示寻路失败，此路不通
        while (mOpenList.Count > 0)
        {
            //遍历开启列表，找到消费最小的点作为检查点
            T cur = mOpenList[0];

            PathNode curNode = GetPathNode(cur, true);

            for (int i = 0; i < mOpenList.Count; i++)
            {
                T t = mOpenList[i];

                PathNode node = GetPathNode(t, true);

                if (node.f < curNode.f && node.h < curNode.h)
                {
                    cur = mOpenList[i];
                    curNode = node;
                }
            }


            //从开启列表中删除检查点，把它加入到一个“关闭列表”，列表中保存所有不需要再次检查的方格。
            mOpenList.Remove(cur);
            mCloseList.Add(cur);

            //检查是否找到终点
            if (cur.Equals(to))
            {
                T t = cur;
                while (true)
                {
                    result.Insert(0, t);
                    var node = GetPathNode(t);
                    if (node != null && node.hasParent)
                    {
                        t = node.parent;
                    }
                    else
                    {
                        break;
                    }
                }

                break;
            }

            ////根据检查点来找到周围可行走的点
            //1.如果是墙或者在关闭列表中则跳过
            //2.如果点不在开启列表中则添加
            //3.如果点在开启列表中且当前的总花费比之前的总花费小，则更新该点信息

            var getNeihbor = getNeihbors(cur);

            while(getNeihbor.MoveNext())
            {
                T neighbour = getNeihbor.Current;

                if (isValid(neighbour) == false || mCloseList.Contains(neighbour))
                    continue;

                int cost = curNode.g + getCostValue(neighbour, cur);

                PathNode neighborNode = GetPathNode(neighbour, true);

                if (cost < neighborNode.g || mOpenList.Contains(neighbour) == false)
                {
                    neighborNode.g = cost;
                    neighborNode.h = getCostValue(neighbour, to);
                    neighborNode.SetParent(cur);

                    if (mOpenList.Contains(neighbour) == false)
                    {
                        mOpenList.Add(neighbour);
                    }
                }
            }
        }

        return result.Count > 0;
    }
    #endregion
}
