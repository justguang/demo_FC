using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    public int pathIndex { get; private set; }//��ҵ�ǰ�ߵ��ڼ���������·�������±꡿
    public bool isEndPath { get; private set; }//�Ƿ�����ն�·����Ĭ��false��
    public int OverMoveCount { get; private set; }//������˶��ٲ�
    public int MoveCount { get; private set; }//�����Ҫ�߶��ٲ����ܽ����ն�

    private int index;//������±ꡢ��Ӫ�±�

    /// <summary>
    /// ��ʼ����������Ӫ��������ȴ���
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
    /// ��Ŀ���ƶ�һ����λ��
    /// </summary>
    /// <param name="node">Ŀ�굥λ��</param>
    public void MoveByNode(Node node)
    {
        pathIndex += 1;
    }

    /// <summary>
    /// �ƶ�һ��
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
