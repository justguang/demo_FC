using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField] private Image face_img;//���ͷ��
    [SerializeField] private Sprite[] defaultFace;//Ĭ��ͷ��
    [SerializeField] private Text username_txt;//����ǳ�
    [SerializeField] private AudioSource audioSource;//��������
    [SerializeField] private Image dice_img;//����img
    [SerializeField] private Sprite[] DiceArr;//����sprite
    [SerializeField] private Text realTime;//ʱ��UI

    private float startTime;//��ʼʱ��
    private bool isWinner = false;

    public long UserUID { get; private set; }//���UID
    public CampEnum m_Camp { get; private set; }//��Ӫ

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
    /// �����յ��¼�
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
    /// �������¼�����
    /// </summary>
    /// <param name="obj"></param>
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
    /// ������
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
