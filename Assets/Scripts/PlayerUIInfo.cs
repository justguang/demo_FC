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
    public Sprite[] DiceArr;//����sprite

    public long userUID;//���UID
    public CampEnum camp;//��Ӫ

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
    /// �������¼�����
    /// </summary>
    /// <param name="obj"></param>
    void OnThrowDiceEvt(object obj)
    {
        StartCoroutine(DoThrowDice());
    }


    /// <summary>
    /// ������
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
