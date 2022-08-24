using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField]
    private Image face;//玩家头像
    [SerializeField]
    private Sprite[] defaultFace;//默认头像
    private Sprite userFaceSprite;//玩家头像sprite
    [SerializeField]
    private Text username;//玩家昵称
    [SerializeField]
    private AudioSource diceAudio;//掷骰子声
    [SerializeField]
    private Image dice;//骰子img
    [SerializeField]
    private Sprite[] DiceArr;//骰子sprite

    private bool isWinner = false;

    public long userUID;//玩家UID
    public CampEnum m_Camp;//阵营

    public void Init(CampEnum camp, string username, long userUID, Sprite userFace)
    {
        this.m_Camp = camp;
        this.userUID = userUID;
        this.userFaceSprite = userFace;
        this.isWinner = false;

        if (username.Length > 5)
        {
            this.username.text = username.Substring(0, 5) + ".....";
        }
        else
        {
            this.username.text = username;
        }
        gameObject.name = username;

        if (userFace == null)
        {
            face.sprite = defaultFace[(int)m_Camp];
        }
        else
        {
            face.sprite = userFaceSprite;
        }

        transform.localScale = Vector3.one;
        EventSys.Instance.AddEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.AddEvt(EventSys.Winner, OnWinnerEvt);
    }

    /// <summary>
    /// 到达终点事件
    /// </summary>
    /// <param name="obj">【0】到达终点者所属阵营，【1】到达终点者UID</param>
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        if ((long)result[1] == userUID)
        {
            isWinner = true;
        }
    }

    /// <summary>
    /// 掷骰子事件触发
    /// </summary>
    /// <param name="obj">阵营</param>
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
        diceAudio?.Play();
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
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Winner, OnWinnerEvt);
    }

}
