using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node
{
    public int camp;
    public Vector3 pos;
    public bool isFly;//5 => 17 +=12
    public int flyOver = 12;//如果来到自己可飞行领地，则向前走12个单位格【环形路径数据下标 (+=12) 】

    /// <summary>
    /// 生成地图数据
    /// </summary>
    /// <param name="start">起飞点</param>
    /// <param name="dir">
    /// /********************************
    /// /* 0 |  方向1  |   走多少个单位  *
    /// /* 1 |  方向2  |   走多少个单位  *
    /// /* 2 |  方向3  |   走多少个单位  *
    /// /********************************
    /// </param>
    /// <param name="camp">起飞点所属领地</param>
    /// <param name="offset">每个单位之间的偏移量</param>
    /// <param name="nodeList"></param>
    public static void GenerateNode(Vector3 start, int[][] dir, int camp, float offset, ref List<Node> nodeList)
    {
        bool isFly = false;
        int[] tmpDir = null;
        int flyOver = 12;

        for (int i = 0; i < dir.Length; i++)
        {
            tmpDir = dir[i];
            for (int j = 0; j < tmpDir[1]; j++)
            {
                switch (tmpDir[0])
                {
                    case (int)MoveDirEnum.Up:
                        start.y += offset;
                        break;
                    case (int)MoveDirEnum.Down:
                        start.y -= offset;
                        break;
                    case (int)MoveDirEnum.Left:
                        start.x -= offset;
                        break;
                    case (int)MoveDirEnum.Right:
                        start.x += offset;
                        break;
                    default:
                        break;
                }

                if (i == 1 && j == 0)
                {
                    isFly = true;
                }
                else
                {
                    isFly = false;
                }

                camp += 1;
                if (camp == 4) camp = 0;

                nodeList.Add(new Node
                {
                    camp = camp,
                    pos = start,
                    isFly = isFly,
                    flyOver = flyOver,
                });
            }
        }
    }
}
