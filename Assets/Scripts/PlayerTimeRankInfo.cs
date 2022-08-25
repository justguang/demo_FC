using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTimeRankInfo : MonoBehaviour
{
    [SerializeField] private Image face_img;
    [SerializeField] private Text username_txt;
    [SerializeField] private Text time_txt;
    [SerializeField] private Sprite[] defaultFace;

    public CampEnum m_Camp { get; private set; }
    public long userUID { get; private set; }
    public float UserTime { get; private set; }//��ʱ����λ�롿


    /// <summary>
    /// 
    /// </summary>
    /// <param name="camp">��Ӫ</param>
    /// <param name="userName">�����</param>
    /// <param name="userUID">���UID</param>
    /// <param name="useTime">ͨ����ʱ����λ�롿</param>
    /// <param name="userFace">���ͷ��</param>
    public void Init(CampEnum camp, string userName, long userUID, float useTime, Sprite userFace)
    {
        this.m_Camp = camp;
        this.userUID = userUID;
        this.UserTime = useTime;

        if (userName.Length > 7)
        {
            this.username_txt.text = userName.Substring(0, 7) + "...";
        }
        else
        {
            this.username_txt.text = userName;
        }

        if (userFace != null)
        {
            face_img.sprite = userFace;
        }
        else
        {
            face_img.sprite = defaultFace[(int)camp];
        }

        TimeSpan ts = TimeSpan.FromSeconds(useTime);
        this.time_txt.text = ts.ToString(@"hh\:mm\:ss");
    }

    /// <summary>
    /// ����ͨ����ʱ
    /// </summary>
    /// <param name="useTime"></param>
    public void UpdateUseTime(float useTime)
    {
        this.UserTime = useTime;
        TimeSpan ts = TimeSpan.FromSeconds(useTime);
        this.time_txt.text = ts.ToString(@"hh\:mm\:ss");
    }
}
