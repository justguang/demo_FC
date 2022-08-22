using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Player : MonoBehaviour
{
    [SerializeField] private Image bgImg;//����
    [SerializeField] private Image face;//ͷ��UI���
    [SerializeField] private Sprite[] bgImg_Sprites;//ͷ�񱳾�
    [SerializeField] private Sprite[] defaultFace_Sprites;//Ĭ��ͷ��
    [SerializeField] private AudioSource stepAudio;

    public CampEnum m_Camp { get; private set; }//������Ӫ
    public long UserUID { get; private set; }//���UID
    public string UserName { get; private set; }//�����
    public Sprite UserFaceSprite { get; private set; }//���ͷ��
    public int PathIndex { get; private set; }//��ҵ�ǰ�ߵ��ڼ���������·�������±꡿
    public int EndPathIndex { get; private set; }//��ҵ�ǰ�ߵ��նεڼ�����·�������±꡿
    public bool IsEndPath { get; private set; }//�Ƿ�����ն�·����Ĭ��false��
    public int OverMoveCount { get; private set; }//������˶��ٲ�
    public int MaxMoveCount { get; private set; }//�����Ҫ�߶��ٲ����ܽ����ն�
    public bool IsFly { get; private set; }//true����ɣ�false�ȴ����ȴ����

    /// <summary>
    /// ��ʼ����������Ӫ��������ȴ���
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
    /// �ص��ȴ����ȴ����
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
    /// �ɻ���������ײ���¼�
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
    /// ��Ŀ���ƶ�һ����λ��
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
                    //���
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
    /// �ƶ�һ��
    /// </summary>
    /// <param name="isSitDown">false�����ƶ���·��</param>
    void MoveOneToForward(bool isSitDown = true)
    {
        OverMoveCount++;//����+1

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
            if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//�޶�·���±�

            Node node = GameManager.Instance.PathList[PathIndex];//����·���±��ȡNode��Ϣ
            transform.position = node.pos;
            if (isSitDown && node.isFly && node.camp == (int)m_Camp)
            {//�ﵽ�Լ��ɷ������
                OverMoveCount += node.flyOver;
                for (int i = 0; i < node.flyOver; i++)
                {
                    PathIndex += 1;
                    if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//�޶�·���±�
                }

                node = GameManager.Instance.PathList[PathIndex];//����·���±��ȡNode��Ϣ
                transform.position = node.pos;

                MaxMoveCount -= node.flyOver;

                //�ɻ���ɴ�������ײ���¼�
                EventSys.Instance.CallEvt(EventSys.FlyToHit, m_Camp);
            }
            else
            {
                MaxMoveCount--;//���������-1
                PathIndex++;//·���±�+1
            }

            if (MaxMoveCount == 0)
            {
                //��������꣬�����ն�·��
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
