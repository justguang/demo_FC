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
    public float UserTime { get; private set; }//用时【单位秒】


    /// <summary>
    /// 
    /// </summary>
    /// <param name="camp">阵营</param>
    /// <param name="userName">玩家名</param>
    /// <param name="userUID">玩家UID</param>
    /// <param name="useTime">通关用时【单位秒】</param>
    /// <param name="userFace">玩家头像</param>
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
    /// 更新通关用时
    /// </summary>
    /// <param name="useTime"></param>
    public void UpdateUseTime(float useTime)
    {
        this.UserTime = useTime;
        TimeSpan ts = TimeSpan.FromSeconds(useTime);
        this.time_txt.text = ts.ToString(@"hh\:mm\:ss");
    }
}
