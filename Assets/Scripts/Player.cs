using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Player : MonoBehaviour
{
    [SerializeField] private Image bgImg;//背景
    [SerializeField] private Image face;//头像UI组件
    [SerializeField] private Sprite[] bgImg_Sprites;//头像背景
    [SerializeField] private Sprite[] defaultFace_Sprites;//默认头像
    [SerializeField] private AudioSource stepAudio;

    public CampEnum m_Camp { get; private set; }//所属阵营
    public long UserUID { get; private set; }//玩家UID
    public string UserName { get; private set; }//玩家名
    public Sprite UserFaceSprite { get; private set; }//玩家头像
    public int PathIndex { get; private set; }//玩家当前走到第几步【环形路径数据下标】
    public int EndPathIndex { get; private set; }//玩家当前走到终段第几步【路径数据下标】
    public bool IsEndPath { get; private set; }//是否进入终段路径【默认false否】
    public int OverMoveCount { get; private set; }//玩家走了多少步
    public int MaxMoveCount { get; private set; }//玩家需要走多少步才能进入终段
    public bool IsFly { get; private set; }//true已起飞，false等待区等待起飞

    /// <summary>
    /// 初始化【加入阵营】，进入等待区
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="userName"></param>
    /// <param name="userFace"></param>
    /// <param name="camp"></param>
    public void Init(long userUID, string userName, Sprite userFace, CampEnum camp)
    {
        this.UserUID = userUID;
        this.UserName = userName;
        this.UserFaceSprite = userFace;
        this.m_Camp = camp;
        this.OverMoveCount = 0;

        gameObject.name = userName;

        bgImg.sprite = bgImg_Sprites[(int)camp];
        if (userFace == null)
        {
            face.sprite = defaultFace_Sprites[(int)camp];
        }
        else
        {
            face.sprite = userFace;
        }

        ReturnWaitPoint();

        //init Event
        EventSys.Instance.AddEvt(EventSys.ThrowDice_OK, MoveByDice);
        EventSys.Instance.AddEvt(EventSys.FlyToHit, OnFlyToHitEvt);

    }


    /// <summary>
    /// 回到等待区等待起飞
    /// </summary>
    void ReturnWaitPoint()
    {
        Vector3 tmpPos = GameManager.Instance.WaitPoint[(int)m_Camp].position;
        tmpPos.x += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        tmpPos.y += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        transform.position = tmpPos;

        this.EndPathIndex = 0;
        this.MaxMoveCount = 49;
        this.IsEndPath = false;
        this.IsFly = false;

        switch (m_Camp)
        {
            case CampEnum.Yellow:
                PathIndex = 0;
                break;
            case CampEnum.Blue:
                PathIndex = 13;
                break;
            case CampEnum.Green:
                PathIndex = 26;
                break;
            case CampEnum.Red:
                PathIndex = 39;
                break;
            default:
                PathIndex = 99;
                break;
        }

    }

    /// <summary>
    /// 飞机飞行意外撞机事件
    /// </summary>
    void OnFlyToHitEvt(object obj)
    {
        if (IsFly && IsEndPath && PathIndex == 2)
        {
            CampEnum otherCamp = (CampEnum)obj;
            switch (m_Camp)
            {
                case CampEnum.Yellow:
                    if (otherCamp.Equals(CampEnum.Green)) ReturnWaitPoint();
                    break;
                case CampEnum.Blue:
                    if (otherCamp.Equals(CampEnum.Red)) ReturnWaitPoint();
                    break;
                case CampEnum.Green:
                    if (otherCamp.Equals(CampEnum.Yellow)) ReturnWaitPoint();
                    break;
                case CampEnum.Red:
                    if (otherCamp.Equals(CampEnum.Blue)) ReturnWaitPoint();
                    break;
                default:
                    break;
            }

        }
    }


    /// <summary>
    /// 向目标移动一个单位格
    /// </summary>
    /// <param name="obj">[0]=>userUID, [1]=>dice</param>
    void MoveByDice(object obj)
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
        bool isSitDown = false;
        for (int i = 0; i < moveNum; i++)
        {
            if (i == (moveNum - 1)) isSitDown = true;
            MoveOneToForward(isSitDown);
            yield return new WaitForSeconds(0.42f);

            if (IsEndPath && EndPathIndex == GameManager.Instance.EndPath[(int)m_Camp].childCount - 1)
            {
                //winner
                EventSys.Instance.CallEvt(EventSys.Winner, new object[] { m_Camp, UserUID });
                break;
            }
        }
    }

    /// <summary>
    /// 移动一格
    /// </summary>
    /// <param name="isSitDown">false本次移动是路过</param>
    void MoveOneToForward(bool isSitDown = true)
    {
        OverMoveCount++;//步数+1

        if (IsEndPath)
        {
            stepAudio?.Play();
            Vector3 pos = GameManager.Instance.EndPath[(int)m_Camp].GetChild(EndPathIndex).position;
            transform.position = pos;

            EndPathIndex++;
        }
        else
        {
            stepAudio?.Play();
            if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//限定路径下标

            Node node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
            transform.position = node.pos;
            if (isSitDown && node.isFly && node.camp == (int)m_Camp)
            {//达到自己可飞行领地
                OverMoveCount += node.flyOver;
                for (int i = 0; i < node.flyOver; i++)
                {
                    PathIndex += 1;
                    if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//限定路径下标
                }

                node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
                transform.position = node.pos;

                MaxMoveCount -= node.flyOver;

                //飞机起飞触发意外撞机事件
                EventSys.Instance.CallEvt(EventSys.FlyToHit, m_Camp);
            }
            else
            {
                MaxMoveCount--;//正常最大步数-1
                PathIndex++;//路径下标+1
            }

            if (MaxMoveCount == 0)
            {
                //最大步数走完，进入终段路线
                IsEndPath = true;
            }

        }

    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice_OK, MoveByDice);
        EventSys.Instance.RemoveEvt(EventSys.FlyToHit, OnFlyToHitEvt);
    }

}
