using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image bgImg;//����
    [SerializeField] private Image face;//ͷ��UI���
    [SerializeField] private Sprite[] bgImg_Sprites;//ͷ�񱳾�
    [SerializeField] private Sprite[] defaultFace_Sprites;//Ĭ��ͷ��

    public CampEnum m_Camp { get; private set; }//������Ӫ
    public long UserUID { get; private set; }//���UID
    public string UserName { get; private set; }//�����
    public string UserFace { get; private set; }//���ͷ��
    public int PathIndex;// { get; private set; }//��ҵ�ǰ�ߵ��ڼ���������·�������±꡿
    public bool IsEndPath;// { get; private set; }//�Ƿ�����ն�·����Ĭ��false��
    public int OverMoveCount;// { get; private set; }//������˶��ٲ�
    public int MaxMoveCount;// { get; private set; }//�����Ҫ�߶��ٲ����ܽ����ն�
    public bool IsFly { get; private set; }//true����ɣ�false�ȴ����ȴ����

    /// <summary>
    /// ��ʼ����������Ӫ��������ȴ���
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
    /// �ص��ȴ����ȴ����
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
    /// ��Ŀ���ƶ�һ����λ��
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
        for (int i = 0; i < moveNum; i++)
        {
            MoveOneToForward();
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// �ƶ�һ��
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
