using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThrowDice : MonoBehaviour
{
    [SerializeField] private Image face_img;
    [SerializeField] private Text username_txt;
    [SerializeField] private Image dice_img;
    [SerializeField] private Sprite[] diceArr;
    [SerializeField] private Sprite[] defaultFace;
    [SerializeField] private AudioSource audioSource;

    public CampEnum m_Camp { get; private set; }
    public long UserUID { get; private set; }

    private bool isWinner = false;//�����յ� isWinner = true

    public void Init(CampEnum camp, string userName, long userUID, Sprite userFace)
    {
        this.m_Camp = camp;
        this.UserUID = userUID;
        this.isWinner = false;

        if (userName.Length > 7)
        {
            userName = userName.Substring(0, 7) + "...";
        }
        this.username_txt.text = userName;

        if (userFace == null) userFace = defaultFace[(int)camp];
        this.face_img.sprite = userFace;


        EventSys.Instance.AddEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.AddEvt(EventSys.Winner, OnWinnerEvt);
    }


    /// <summary>
    /// �������¼�����
    /// </summary>
    /// <param name="obj">��Ӫ</param>
    void OnThrowDiceEvt(object obj)
    {
        int tmpCamp = (int)obj;
        if (tmpCamp == (int)m_Camp && isWinner == false)
        {
            //�ֵ��ҷ�������
            StartCoroutine(DoThrowDice());
        }
    }

    /// <summary>
    /// �����յ��¼�
    /// </summary>
    /// <param name="obj">��0�������յ���������Ӫ����1�������յ���UID</param>
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        if ((long)result[1] == UserUID)
        {
            isWinner = true;
        }
    }


    /// <summary>
    /// ������
    /// </summary>
    IEnumerator DoThrowDice()
    {
        yield return new WaitForSeconds(1f);

        audioSource?.Play();
        int oldIndex = -1;
        int randomIndex = -1;
        for (int i = 0; i < 10; i++)
        {
            randomIndex = Random.Range(0, diceArr.Length);
            if (randomIndex == oldIndex)
            {
                randomIndex = Random.Range(0, diceArr.Length);
            }
            dice_img.sprite = diceArr[randomIndex];
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        EventSys.Instance.CallEvt(EventSys.ThrowDice_OK, new object[] { m_Camp, UserUID, randomIndex });
    }


    private void OnDestroy()
    {
        EventSys.Instance.RemoveEvt(EventSys.ThrowDice, OnThrowDiceEvt);
        EventSys.Instance.RemoveEvt(EventSys.Winner, OnWinnerEvt);
    }
}
