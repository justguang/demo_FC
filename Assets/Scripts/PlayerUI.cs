using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示在UI上的玩家信息
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image face_img;
    [SerializeField] private Text username_txt;
    [SerializeField] private Outline faceOutline;
    [SerializeField] private Color[] outlineColor;
    [SerializeField] private Sprite[] defaultFace;

    public Sprite UserFace { get; private set; }
    public string UserName { get; private set; }
    public long UserUID { get; private set; }
    public CampEnum m_Camp { get; private set; }//阵营
    public float StartTime { get; private set; }//开始时间

    public void Init(CampEnum camp, string userName, long userUID, Sprite userFace)
    {
        this.m_Camp = camp;
        this.UserName = userName;
        this.UserUID = userUID;
        this.faceOutline.effectColor = outlineColor[(int)camp];

        if (userName.Length > 7)
        {
            userName = userName.Substring(0, 7) + "...";
        }
        this.username_txt.text = userName;

        if (userFace == null) userFace = defaultFace[(int)camp];
        this.face_img.sprite = userFace;
        this.UserFace = userFace;

        this.StartTime = Time.realtimeSinceStartup;
    }
}
