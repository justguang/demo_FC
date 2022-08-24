using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image face;//ͷ��UI���
    [SerializeField] private Sprite[] defaultFace_Sprites;//Ĭ��ͷ��
    [SerializeField] private Color[] faceOutlineColor;//ͷ����߿�
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClip;//��Ч

    public CampEnum m_Camp { get; private set; }//������Ӫ
    public long UserUID { get; private set; }//���UID
    public string UserName { get; private set; }//�����
    public Sprite UserFaceSprite { get; private set; }//���ͷ��
    public int PathIndex { get; private set; }//��ҵ�ǰ�ߵ��ڼ���������·�������±꡿
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
    /// �ص��ȴ����ȴ����
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
    /// �ƶ�ͣ�����󴥷�
    /// </summary>
    /// <param name="obj">��0��userUID����1����Ӫ����2��·���±�</param>
    void OnMoventStopEvt(object obj)
    {
        long uid = (long)((object[])obj)[0];

        if (uid == UserUID)
        {
            //�Լ�
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
                    if (PathIndex == GameManager.Instance.PathList.Count) PathIndex = 0;//�޶�·���±�
                }

                node = GameManager.Instance.PathList[PathIndex];//����·���±��ȡNode��Ϣ
                transform.position = node.pos;

                //�ɻ���ɴ�������ײ���¼�
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

                //���з��ɻ�ײ��
                ReturnWaitPoint();
            }
        }


    }

    /// <summary>
    /// ��������ײ���¼�
    /// </summary>
    /// <param name="obj">������������Ӫ</param>
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
    /// ��Ŀ���ƶ�һ����λ��
    /// </summary>
    /// <param name="obj">[0]=>userUID, [1]=>���ӵ���</param>
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

                    //���
                    IsFly = true;
                    Vector3 pos = GameManager.Instance.StartPoint[(int)m_Camp].position;
                    transform.position = pos;
                }
            }
            else
            {
                //��̬�ı���Hierarchy�е�UI˳��
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
                    //�����յ�
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
                    /// ͣ���¼� ��0��userUID����1����Ӫ����2��·���±�
                    EventSys.Instance.CallEvt(EventSys.MoventStop, new object[] { UserUID, m_Camp, PathIndex });
                }
            }

        }
    }

    /// <summary>
    /// �ƶ�һ��
    /// </summary>
    /// <param name="isSitDown">false�����ƶ���·��</param>
    void MoveOneToForward(int pathCount)
    {
        if (MaxMoveCount == 0)
        {
            //��������꣬�����ն�·��
            IsEndPath = true;
            //�����±�
            PathIndex = -1;
        }

        OverMoveCount += 1;//�ܲ���+1
        MaxMoveCount -= 1;//���������-1
        PathIndex += 1;//·���±�+1

        audioSource.clip = audioClip[0];
        audioSource.Play();

        if (IsEndPath)
        {
            Vector3 pos = GameManager.Instance.EndPath[(int)m_Camp].GetChild(PathIndex).position;
            transform.position = pos;
        }
        else
        {
            if (PathIndex == pathCount) PathIndex = 0;//�޶�·���±�
            Node node = GameManager.Instance.PathList[PathIndex];//����·���±��ȡNode��Ϣ
            transform.position = node.pos;
        }

    }

    //����
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
