using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image face;//头像UI组件
    [SerializeField] private Sprite[] defaultFace_Sprites;//默认头像
    [SerializeField] private Color[] faceOutlineColor;//头像外边框
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClip;//音效

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
        EventSys.Instance.AddEvt(EventSys.MoventStop, OnMoventStopEvt);

    }


    /// <summary>
    /// 回到等待区等待起飞
    /// </summary>
    void ReturnWaitPoint()
    {
        this.MaxMoveCount = Config.MaxMoveCount;
        this.IsEndPath = false;
        this.IsFly = false;

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

        Vector3 tmpPos = GameManager.Instance.WaitPoint[(int)m_Camp].position;
        tmpPos.x += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        tmpPos.y += Random.Range(-Config.OffsetWaitPos, Config.OffsetWaitPos);
        transform.position = tmpPos;
    }

    /// <summary>
    /// 移动停下来后触发
    /// </summary>
    /// <param name="obj">【0】userUID，【1】阵营，【2】路径下标</param>
    void OnMoventStopEvt(object obj)
    {
        long uid = (long)((object[])obj)[0];

        if (uid == UserUID)
        {
            //自己
            Node node = GameManager.Instance.PathList[PathIndex];
            if (node.isFly && node.camp == (int)m_Camp)
            {
                audioSource.clip = audioClip[1];
                audioSource.Play();

                OverMoveCount += node.flyOver;
                MaxMoveCount -= node.flyOver;
                for (int i = 0; i < node.flyOver; i++)
                {
                    PathIndex += 1;
                    if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//限定路径下标
                }

                node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
                transform.position = node.pos;

                //飞机起飞触发意外撞机事件
                EventSys.Instance.CallEvt(EventSys.FlyToHit, m_Camp);
            }
        }
        else
        {
            CampEnum camp = (CampEnum)((object[])obj)[1];
            int pathIdx = (int)((object[])obj)[2];

            if (!IsEndPath && !camp.Equals(m_Camp)
                && pathIdx == PathIndex
                && MaxMoveCount < Config.MaxMoveCount)
            {
                audioSource.clip = audioClip[3];
                audioSource.Play();

                //被敌方飞机撞回
                ReturnWaitPoint();
            }
        }


    }

    /// <summary>
    /// 飞行意外撞机事件
    /// </summary>
    /// <param name="obj">飞行者所属阵营</param>
    void OnFlyToHitEvt(object obj)
    {
        if (IsFly && IsEndPath && PathIndex == 2)
        {
            bool isHit = false;
            CampEnum otherCamp = (CampEnum)obj;
            switch (m_Camp)
            {
                case CampEnum.Yellow:
                    if (otherCamp.Equals(CampEnum.Green)) isHit = true;
                    break;
                case CampEnum.Blue:
                    if (otherCamp.Equals(CampEnum.Red)) isHit = true;
                    break;
                case CampEnum.Green:
                    if (otherCamp.Equals(CampEnum.Yellow)) isHit = true;
                    break;
                case CampEnum.Red:
                    if (otherCamp.Equals(CampEnum.Blue)) isHit = true;
                    break;
                default:
                    break;
            }


            if (isHit)
            {
                audioSource.clip = audioClip[3];
                audioSource.Play();

                StartCoroutine(DoScale());

                ReturnWaitPoint();
            }

        }
    }


    /// <summary>
    /// 向目标移动一个单位格
    /// </summary>
    /// <param name="obj">[0]=>userUID, [1]=>骰子点数</param>
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
                    audioSource.clip = audioClip[2];
                    audioSource.Play();

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

                StartCoroutine(DoMoveToForward(dice));
            }

        }
    }


    IEnumerator DoMoveToForward(int moveNum)
    {
        int endPathCount = GameManager.Instance.EndPath[(int)m_Camp].childCount;
        int pathCount = GameManager.Instance.PathList.Count;
        for (int i = 0; i < moveNum; i++)
        {
            MoveOneToForward(pathCount);
            yield return new WaitForSeconds(Config.MoveWaitTime);

            if (IsEndPath)
            {
                if (PathIndex == (endPathCount - 1))
                {
                    //到达终点
                    audioSource.clip = audioClip[1];
                    audioSource.Play();

                    StartCoroutine(DoScale());

                    //winner
                    EventSys.Instance.CallEvt(EventSys.Winner, new object[] { m_Camp, UserUID });
                    break;
                }
            }
            else
            {
                if (i == moveNum - 1)
                {
                    /// 停下事件 【0】userUID，【1】阵营，【2】路径下标
                    EventSys.Instance.CallEvt(EventSys.MoventStop, new object[] { UserUID, m_Camp, PathIndex });
                }
            }

        }
    }

    /// <summary>
    /// 移动一格
    /// </summary>
    /// <param name="isSitDown">false本次移动是路过</param>
    void MoveOneToForward(int pathCount)
    {
        if (MaxMoveCount == 0)
        {
            //最大步数走完，进入终段路线
            IsEndPath = true;
            //重置下标
            PathIndex = -1;
        }

        OverMoveCount += 1;//总步数+1
        MaxMoveCount -= 1;//正常最大步数-1
        PathIndex += 1;//路径下标+1

        audioSource.clip = audioClip[0];
        audioSource.Play();

        if (IsEndPath)
        {
            Vector3 pos = GameManager.Instance.EndPath[(int)m_Camp].GetChild(PathIndex).position;
            transform.position = pos;
        }
        else
        {
            if (PathIndex == pathCount) PathIndex = 0;//限定路径下标
            Node node = GameManager.Instance.PathList[PathIndex];//根据路径下标获取Node信息
            transform.position = node.pos;
        }

    }

    //缩放
    IEnumerator DoScale()
    {
        Vector3 scale = transform.localScale;
        while (scale.x < 1.5)
        {
            scale.x += Time.deltaTime;
            scale.y += Time.deltaTime;
            transform.localScale = scale;
            yield return new WaitForSeconds(0.1f);
        }

        while (scale.x > 1)
        {
            scale.x -= Time.deltaTime;
            scale.y -= Time.deltaTime;
            transform.localScale = scale;
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice_OK, MoveByDice);
        EventSys.Instance.RemoveEvt(EventSys.FlyToHit, OnFlyToHitEvt);
        EventSys.Instance.RemoveEvt(EventSys.MoventStop, OnMoventStopEvt);
    }

}
