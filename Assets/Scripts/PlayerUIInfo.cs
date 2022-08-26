using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField] private Image face_img;//玩家头像
    [SerializeField] private Sprite[] defaultFace;//默认头像
    [SerializeField] private Text username_txt;//玩家昵称
    [SerializeField] private AudioSource audioSource;//掷骰子声
    [SerializeField] private Image dice_img;//骰子img
    [SerializeField] private Sprite[] DiceArr;//骰子sprite
    [SerializeField] private Text realTime;//时间UI

    private float startTime;//开始时间
    private bool isWinner = false;

    public long UserUID { get; private set; }//玩家UID
    public CampEnum m_Camp { get; private set; }//阵营

    public void Init(CampEnum camp, string username, long userUID, Sprite userFace)
    {
        this.m_Camp = camp;
        this.UserUID = userUID;
        this.isWinner = false;

        if (username.Length > 7) username = username.Substring(0, 7) + "...";
        this.username_txt.text = username;

        if (userFace == null)
        {
            userFace = defaultFace[(int)m_Camp];
        }
        face_img.sprite = userFace;

        this.startTime = Time.realtimeSinceStartup;

        EventSys.Instance.AddEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.AddEvt(EventSys.Winner, OnWinnerEvt);
    }

    /// <summary>
    /// 到达终点事件
    /// </summary>
    /// <param name="obj"></param>
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        if ((long)result[1] == UserUID)
        {
            isWinner = true;
        }
    }

    /// <summary>
    /// 掷骰子事件触发
    /// </summary>
    /// <param name="obj"></param>
    void OnThrowDiceEvt(object obj)
    {
        int tmpCamp = (int)obj;
        if (tmpCamp == (int)m_Camp && isWinner == false)
        {
            //轮到我方掷骰子
            StartCoroutine(DoThrowDice());
        }
    }


    /// <summary>
    /// 掷骰子
    /// </summary>
    IEnumerator DoThrowDice()
    {
        audioSource?.Play();
        int oldIndex = -1;
        int randomIndex = -1;
        for (int i = 0; i < 10; i++)
        {
            randomIndex = UnityEngine.Random.Range(0, DiceArr.Length);
            if (randomIndex == oldIndex)
            {
                randomIndex = UnityEngine.Random.Range(0, DiceArr.Length);
            }
            dice_img.sprite = DiceArr[randomIndex];
            yield return new WaitForSeconds(0.1f);
        }

        EventSys.Instance.CallEvt(EventSys.ThrowDice_OK, new object[] { m_Camp, UserUID, randomIndex });

    }

    private void LateUpdate()
    {
        TimeSpan ts = TimeSpan.FromSeconds(Time.realtimeSinceStartup - startTime);
        if (realTime != null)
        {
            realTime.text = ts.ToString(@"hh\:mm\:ss");
        }

    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Winner, OnWinnerEvt);
    }

}
