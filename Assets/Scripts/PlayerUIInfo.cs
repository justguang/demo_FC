using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField]
    private Image face;//玩家头像
    [SerializeField]
    private Text username;//玩家昵称
    [SerializeField]
    private Image dice;//骰子img
    private CampEnum camp;//阵营

    public Sprite[] DiceArr;//骰子sprite


    public void Init(CampEnum camp, string username, string face = "")
    {
        this.camp = camp;
        this.username.text = username;

        switch (camp)
        {
            case CampEnum.Yellow:
                EventSys.Instance.AddEvt(EventSys.Left_Up_ThrowDice, OnThrowDiceEvt);
                break;
            case CampEnum.Blue:
                EventSys.Instance.AddEvt(EventSys.Right_Up_ThrowDice, OnThrowDiceEvt);
                break;
            case CampEnum.Green:
                EventSys.Instance.AddEvt(EventSys.Right_Bottom_ThrowDice, OnThrowDiceEvt);
                break;
            case CampEnum.Red:
                EventSys.Instance.AddEvt(EventSys.Left_Bottom_ThrowDice, OnThrowDiceEvt);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 掷骰子事件触发
    /// </summary>
    /// <param name="obj"></param>
    void OnThrowDiceEvt(object obj)
    {

    }

}
