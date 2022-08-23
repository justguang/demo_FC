using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image face;//头像UI组件
    [SerializeField] private Sprite[] defaultFace_Sprites;//默认头像
    [SerializeField] private Color[] faceOutlineColor;//头像外边框
    [SerializeField] private AudioSource stepAudio;

    public CampEnum m_Camp { get; private set; }//所属阵营
    public long UserUID { get; private set; }//玩家UID
    public string UserName { get; private set; }//玩家名
    public Sprite UserFaceSprite { get; private set; }//玩家头像
    public int PathIndex { get; private set; }//玩家当前走到第几步【环形路径数据下标】
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

        ReturnWaitPoint();
        gameObject.name = userName;

        face.transform.GetComponent<Outline>().effectColor = faceOutlineColor[(int)camp];
        if (userFace == null)
        {
            face.sprite = defaultFace_Sprites[(int)camp];
        }
        else
        {
            face.sprite = userFace;
        }


        //init Event
        EventSys.Instance.AddEvt(EventSys.ThrowDice_OK, MoveByDice);
        EventSys.Instance.AddEvt(EventSys.FlyToHit, OnFlyToHitEvt);

    }


    /// <summary>
    /// 回到等待区等待起飞
    /// </summary>
    void ReturnWaitPoint()
    {
        this.MaxMoveCount = 50;
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

        Vector3 tmpPos = GameManager.Instance.WaitPoint[(int)m_Camp].position;
        tmpPos.x += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        tmpPos.y += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        transform.position = tmpPos;
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
                //动态改变在Hierarchy中的UI顺序
                transform.SetSiblingIndex(transform.parent.childCount - 1);

                StartCoroutine(DoMoveOneToForward(dice));
            }

        }
    }

    IEnumerator DoMoveOneToForward(int moveNum)
    {
        bool isSitDown = false;
        int endPathCount = GameManager.Instance.EndPath[(int)m_Camp].childCount;
        int pathCount = GameManager.Instance.PathList.Count;
        for (int i = 0; i < moveNum; i++)
        {
            if (i == (moveNum - 1)) isSitDown = true;
            MoveOneToForward(pathCount, isSitDown);
            yield return new WaitForSeconds(0.415f);

            if (IsEndPath && PathIndex == endPathCount)
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
    void MoveOneToForward(int pathCount, bool isSitDown = true)
    {
        OverMoveCount++;//步数+1

        if (IsEndPath)
        {
            stepAudio?.Play();
            Vector3 pos = GameManager.Instance.EndPath[(int)m_Camp].GetChild(PathIndex).position;
            transform.position = pos;
        }
        else
        {
            stepAudio?.Play();
            if (PathIndex == pathCount) PathIndex = 0;//限定路径下标
            Node node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
            transform.position = node.pos;

            if (isSitDown && node.isFly && node.camp == (int)m_Camp)
            {//达到自己可飞行领地
                OverMoveCount += node.flyOver;
                MaxMoveCount -= node.flyOver;
                for (int i = 0; i < node.flyOver; i++)
                {
                    PathIndex += 1;
                    if (PathIndex == pathCount) PathIndex = 0;//限定路径下标
                }

                node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
                transform.position = node.pos;

                //飞机起飞触发意外撞机事件
                EventSys.Instance.CallEvt(EventSys.FlyToHit, m_Camp);
            }
            else
            {
                MaxMoveCount--;//正常最大步数-1
            }

            if (MaxMoveCount == 0)
            {
                //最大步数走完，进入终段路线
                IsEndPath = true;
                //重置下标
                PathIndex = -1;
            }
        }

        PathIndex++;//路径下标+1
    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice_OK, MoveByDice);
        EventSys.Instance.RemoveEvt(EventSys.FlyToHit, OnFlyToHitEvt);
    }

}
