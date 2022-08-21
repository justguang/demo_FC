using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image bgImg;//背景
    [SerializeField] private Image face;//头像UI组件
    [SerializeField] private Sprite[] bgImg_Sprites;//头像背景
    [SerializeField] private Sprite[] defaultFace_Sprites;//默认头像

    public CampEnum m_Camp { get; private set; }//所属阵营
    public long UserUID { get; private set; }//玩家UID
    public string UserName { get; private set; }//玩家名
    public string UserFace { get; private set; }//玩家头像
    public int pathIndex { get; private set; }//玩家当前走到第几步【环形路径数据下标】
    public bool isEndPath { get; private set; }//是否进入终段路径【默认false否】
    public int OverMoveCount { get; private set; }//玩家走了多少步
    public int MoveCount { get; private set; }//玩家需要走多少步才能进入终段

    private int index;//起飞区下标、阵营下标

    /// <summary>
    /// 初始化【加入阵营】，进入等待区
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="userName"></param>
    /// <param name="userFace"></param>
    /// <param name="camp"></param>
    public bool Init(long userUID, string userName, string userFace, CampEnum camp)
    {
        this.UserUID = userUID;
        this.UserName = userName;
        this.UserFace = userFace;
        this.m_Camp = camp;
        this.OverMoveCount = 0;
        this.MoveCount = 49;
        this.isEndPath = false;

        switch (camp)
        {
            case CampEnum.Yellow:
                index = 0;
                pathIndex = -1;
                break;
            case CampEnum.Blue:
                index = 1;
                pathIndex = 12;
                break;
            case CampEnum.Green:
                index = 2;
                pathIndex = 25;
                break;
            case CampEnum.Red:
                index = 3;
                pathIndex = 38;
                break;
            default:
                index = -1;
                break;
        }

        if (index != -1)
        {
            bgImg.sprite = bgImg_Sprites[index];
            if (string.IsNullOrEmpty(userFace))
            {
                face.sprite = defaultFace_Sprites[index];
            }

            Vector3 tmpPos = GameManager.Instance.WaitPoint[index].position;
            tmpPos.x += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
            tmpPos.y += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
            transform.position = tmpPos;

            return true;
        }
        return false;
    }

    /// <summary>
    /// 想目标移动一个单位格
    /// </summary>
    /// <param name="node">目标单位格</param>
    public void MoveByNode(Node node)
    {
        pathIndex += 1;
    }

    /// <summary>
    /// 移动一格
    /// </summary>
    public void MoveOneToForward()
    {
        OverMoveCount++;
        pathIndex++;
        if (isEndPath)
        {
            Vector3 pos = GameManager.Instance.EndPath[index].GetChild(pathIndex).position;
            transform.position = pos;
        }
        else
        {
            Node node = GameManager.Instance.PathList[pathIndex];
            transform.position = node.pos;
            if (node.isFly)
            {
                OverMoveCount += node.flyOver;
                pathIndex += node.flyOver;
                node = GameManager.Instance.PathList[pathIndex];
                transform.position = node.pos;
            }

            MoveCount--;
            if (MoveCount == 0)
            {
                isEndPath = true;
                pathIndex = -1;
            }
        }
    }


}
