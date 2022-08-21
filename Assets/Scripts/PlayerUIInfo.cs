using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField]
    private Image face;//���ͷ��
    [SerializeField]
    private Text username;//����ǳ�
    [SerializeField]
    private Image dice;//����img
    private CampEnum camp;//��Ӫ

    public Sprite[] DiceArr;//����sprite


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
    /// �������¼�����
    /// </summary>
    /// <param name="obj"></param>
    void OnThrowDiceEvt(object obj)
    {

    }

}
