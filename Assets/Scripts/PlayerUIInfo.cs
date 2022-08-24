using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIInfo : MonoBehaviour
{
    [SerializeField]
    private Image face;//���ͷ��
    [SerializeField]
    private Sprite[] defaultFace;//Ĭ��ͷ��
    private Sprite userFaceSprite;//���ͷ��sprite
    [SerializeField]
    private Text username;//����ǳ�
    [SerializeField]
    private AudioSource diceAudio;//��������
    [SerializeField]
    private Image dice;//����img
    [SerializeField]
    private Sprite[] DiceArr;//����sprite

    private bool isWinner = false;

    public long userUID;//���UID
    public CampEnum m_Camp;//��Ӫ

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
    /// �����յ��¼�
    /// </summary>
    /// <param name="obj">��0�������յ���������Ӫ����1�������յ���UID</param>
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        if ((long)result[1] == userUID)
        {
            isWinner = true;
        }
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
    /// ������
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
