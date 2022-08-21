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
    public Sprite[] DiceArr;//骰子sprite

    public long userUID;//玩家UID
    public CampEnum camp;//阵营

    public void Init(CampEnum camp, string username, long userUID, string face = "")
    {
        this.camp = camp;
        this.username.text = username;
        this.userUID = userUID;

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
        StartCoroutine(DoThrowDice());
    }


    /// <summary>
    /// 掷骰子
    /// </summary>
    IEnumerator DoThrowDice()
    {
        int oldIndex = -1;
        int randomIndex = -1;
        for (int i = 0; i < 10; i++)
        {
            randomIndex = Random.Range(0, DiceArr.Length);
            if (randomIndex == oldIndex)
            {
                randomIndex = Random.Range(0, DiceArr.Length);
            }
            dice.sprite = DiceArr[randomIndex];
            yield return new WaitForSeconds(0.1f);
        }

        EventSys.Instance.CallEvt(EventSys.ThrowDice_OK, new long[] { userUID, randomIndex });

    }



    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.Left_Up_ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Left_Bottom_ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Right_Up_ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Right_Bottom_ThrowDice, OnThrowDiceEvt);
    }

}
