using System.Collections;
using System.Collections.Generic;
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
    public int PathIndex;// { get; private set; }//玩家当前走到第几步【环形路径数据下标】
    public bool IsEndPath;// { get; private set; }//是否进入终段路径【默认false否】
    public int OverMoveCount;// { get; private set; }//玩家走了多少步
    public int MaxMoveCount;// { get; private set; }//玩家需要走多少步才能进入终段
    public bool IsFly { get; private set; }//true已起飞，false等待区等待起飞

    /// <summary>
    /// 初始化【加入阵营】，进入等待区
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="userName"></param>
    /// <param name="userFace"></param>
    /// <param name="camp"></param>
    public void Init(long userUID, string userName, string userFace, CampEnum camp)
    {
        this.UserUID = userUID;
        this.UserName = userName;
        this.UserFace = userFace;
        this.m_Camp = camp;
        this.OverMoveCount = 0;

        bgImg.sprite = bgImg_Sprites[(int)camp];
        if (string.IsNullOrEmpty(userFace))
        {
            face.sprite = defaultFace_Sprites[(int)camp];
        }

        ReturnWaitPoint();
        EventSys.Instance.AddEvt(EventSys.ThrowDice_OK, MoveByDice);
    }


    /// <summary>
    /// 回到等待区等待起飞
    /// </summary>
    public void ReturnWaitPoint()
    {
        Vector3 tmpPos = GameManager.Instance.WaitPoint[(int)m_Camp].position;
        tmpPos.x += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        tmpPos.y += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        transform.position = tmpPos;

        this.MaxMoveCount = 49;
        this.IsEndPath = false;

        switch (m_Camp)
        {
            case CampEnum.Yellow:
                PathIndex = -1;
                break;
            case CampEnum.Blue:
                PathIndex = 12;
                break;
            case CampEnum.Green:
                PathIndex = 25;
                break;
            case CampEnum.Red:
                PathIndex = 38;
                break;
            default:
                PathIndex = 99;
                break;
        }

    }




    /// <summary>
    /// 想目标移动一个单位格
    /// </summary>
    /// <param name="obj">[0]=>userUID, [1]=>dice</param>
    public void MoveByDice(object obj)
    {
        long[] param = (long[])obj;


        if (param[0] == UserUID)
        {
            int dice = (int)param[1] + 1;
            if (!IsFly)
            {
                if (dice == 1 || dice == 6)
                {
                    //起飞
                    IsFly = true;
                    Vector3 pos = GameManager.Instance.StartPoint[(int)m_Camp].position;
                    transform.position = pos;
                }
            }
            else
            {
                StartCoroutine(DoMoveOneToForward(dice));
            }

        }
    }

    IEnumerator DoMoveOneToForward(int moveNum)
    {
        for (int i = 0; i < moveNum; i++)
        {
            MoveOneToForward();
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 移动一格
    /// </summary>
    public void MoveOneToForward()
    {
        OverMoveCount++;
        PathIndex++;
        if (IsEndPath)
        {
            Vector3 pos = GameManager.Instance.EndPath[(int)m_Camp].GetChild(PathIndex).position;
            transform.position = pos;
        }
        else
        {
            Node node = GameManager.Instance.PathList[PathIndex];
            transform.localPosition = node.pos;
            if (node.isFly)
            {
                //OverMoveCount += node.flyOver;
                //PathIndex += node.flyOver;
                //node = GameManager.Instance.PathList[PathIndex];
                //transform.position = node.pos;
            }

            MaxMoveCount--;
            if (MaxMoveCount == 0)
            {
                IsEndPath = true;
                PathIndex = -1;
            }
        }
    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice_OK, MoveByDice);
    }

}
