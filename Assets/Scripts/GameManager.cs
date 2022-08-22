using OpenBLive.Runtime.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static Unity.Burst.Intrinsics.X86.Avx;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //����˳����ö�� CampEnum ���� 0��1��2��3 =��yellow��blue��green��red
    [Header("��Ҹ��ڵ�")]
    public Transform PlayerRoot;
    [Header("�ȴ������ڵ�")]
    public Transform[] WaitPoint;
    [Header("��������ڵ�")]
    public Transform[] StartPoint;
    [Header("Player UI info ��Ϣ��ʾ���ڵ�")]
    public Transform[] PlayerUIInfoRoot;

    [Header("����·�����ڵ�")]
    public Transform PathRoot;
    [Header("����·������")]
    public List<Node> PathList;
    [Header("�ն�·�����ڵ�")]
    public Transform[] EndPath;

    [Header("Player Prefab")]
    public GameObject PlayerPrefab;
    public GameObject PlayerUIInfo_L_Prefab;
    public GameObject PlayerUIInfo_R_Prefab;

    [Space(10)]

    #region UI
    public Transform UIRoot;
    public Transform LoginPanel;
    public Transform PlayPanel;
    #endregion

    //����Player��key => ��Ӫ =>  value<userUID,Player>��
    private Dictionary<int, Dictionary<long, Player>> playerDic;
    private Dictionary<int, Dictionary<long, PlayerUIInfo>> playerUiInfoDic;

    public void Init()
    {
        Instance = this;

        //int Component
        {
            if (UIRoot == null) UIRoot = GameObject.Find("Canvas").transform;
            if (LoginPanel == null) LoginPanel = GameObject.Find("LoginPanel").transform;
            if (PlayPanel == null) PlayPanel = GameObject.Find("PlayPanel").transform;

            UIRoot.gameObject.SetActive(true);
            PlayPanel.gameObject.SetActive(false);
            LoginPanel.gameObject.SetActive(true);

            BilibiliLoginManager loginMgr = LoginPanel.Find("loginUI").GetComponent<BilibiliLoginManager>();
            loginMgr?.LinkFailedEvent.AddListener(OnLinkFailedEvt);
            loginMgr?.LinkSuccessEvent.AddListener(OnLinkSuccessEvt);
        }

        //init Dictionary
        {
            playerDic = new Dictionary<int, Dictionary<long, Player>>();
            playerDic.Add((int)CampEnum.Yellow, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Blue, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Green, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Red, new Dictionary<long, Player>());

            playerUiInfoDic = new Dictionary<int, Dictionary<long, PlayerUIInfo>>();
            playerUiInfoDic.Add((int)CampEnum.Yellow, new Dictionary<long, PlayerUIInfo>());
            playerUiInfoDic.Add((int)CampEnum.Blue, new Dictionary<long, PlayerUIInfo>());
            playerUiInfoDic.Add((int)CampEnum.Green, new Dictionary<long, PlayerUIInfo>());
            playerUiInfoDic.Add((int)CampEnum.Red, new Dictionary<long, PlayerUIInfo>());
        }

        //init danmaku event
        {
            ConnectViaCode.Instance.OnDanmaku += OnDanmakuEvt;
            ConnectViaCode.Instance.OnGift += OnGiftEvt;
            ConnectViaCode.Instance.OnGuardBuy += OnGuard;
            ConnectViaCode.Instance.OnSuperChat += OnSuperChat;
        }

        //init EventSys
        {
            EventSys.Instance.AddEvt(EventSys.Winner, OnWinnerEvt);
        }

    }



    [ContextMenu("���ɻ���·������")]
    public void BackupNodePath()
    {
        PathList = new List<Node>();
        bool isFly = false;
        int tmpCamp = (int)CampEnum.Red;

        int count = PathRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform chilTrans = PathRoot.GetChild(i);
            isFly = chilTrans.name.Contains("fly");
            PathList.Add(new Node
            {
                isFly = isFly,
                pos = chilTrans.position,
                camp = tmpCamp
            });

            tmpCamp += 1;
            if (tmpCamp == 4) tmpCamp = 0;
        }
    }


    #region Private Func

    #region On Danmaku Event
    //��Ļ
    void OnDanmakuEvt(Dm dm)
    {
        switch (dm.msg)
        {
            case Config.Join_Yellow:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Yellow, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //�Ѵ���
                    Debug.LogWarning($"{dm.userName} �Ѵ��ڣ��������");
                }
                break;
            case Config.Join_Blue:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Blue, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //�Ѵ���
                    Debug.LogWarning($"{dm.userName} �Ѵ��ڣ��������");
                }
                break;
            case Config.Join_Green:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Green, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //�Ѵ���
                    Debug.LogWarning($"{dm.userName} �Ѵ��ڣ��������");
                }
                break;
            case Config.Join_Red:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Red, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //�Ѵ���
                    Debug.LogWarning($"{dm.userName} �Ѵ��ڣ��������");
                }
                break;
            default:
                break;
        }

    }

    //������
    void OnGiftEvt(SendGift sendGift)
    {

    }

    //�յ��󺽺�
    void OnGuard(Guard guard)
    {

    }

    //�յ�SC
    void OnSuperChat(SuperChat sc)
    {

    }
    #endregion


    //����Player
    IEnumerator DoLoadPlayer(CampEnum camp, long userUID, string userName, string userFace)
    {

        Texture2D loadTexture = null;
        Sprite loadFace = null;
        using (UnityWebRequest webReq = new UnityWebRequest())
        {
            webReq.url = userFace;
            webReq.method = UnityWebRequest.kHttpVerbGET;
            webReq.downloadHandler = new DownloadHandlerTexture();
            yield return webReq.SendWebRequest();
            if (webReq.result == UnityWebRequest.Result.Success)
            {
                loadTexture = DownloadHandlerTexture.GetContent(webReq);
                loadFace = Sprite.Create(loadTexture, new Rect(0, 0, loadTexture.width, loadTexture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                //error
                loadTexture = null;
                loadFace = null;
            }
        }


        LoadPlayer(camp, userUID, userName, loadFace);
    }

    void OnLinkSuccessEvt()
    {
        LoginPanel.gameObject.SetActive(false);
        PlayPanel.gameObject.SetActive(true);

        int random = Random.Range(111, 1000);
        LoadPlayer(CampEnum.Yellow, random, random.ToString(), null);
        random = Random.Range(111, 1000);
        LoadPlayer(CampEnum.Blue, random, random.ToString(), null);
        random = Random.Range(111, 1000);
        LoadPlayer(CampEnum.Green, random, random.ToString(), null);
        random = Random.Range(111, 1000);
        LoadPlayer(CampEnum.Red, random, random.ToString(), null);

        isStartThrowDice = true;

        Debug.Log("���ӳɹ�����Ϸ��ʼ");
    }

    void OnLinkFailedEvt()
    {
        PlayPanel.gameObject.SetActive(false);
        LoginPanel.gameObject.SetActive(true);
        Debug.LogWarning("����ʧ��");
    }

    void GameEnd()
    {
        ConnectViaCode.Instance?.LinkEnd();
    }

    //����ҵ����յ�
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        CampEnum camp = (CampEnum)result[0];
        long userUID = (long)result[1];


        //�����Ϣ
        if (playerDic.ContainsKey((int)camp))
        {
            if (playerDic[(int)camp].TryGetValue(userUID, out Player player))
            {
                playerDic[(int)camp].Remove(userUID);
                Destroy(player.gameObject);

            }
        }

        if (playerUiInfoDic.ContainsKey((int)camp))
        {
            if (playerUiInfoDic[(int)camp].TryGetValue(userUID, out PlayerUIInfo playerUIInfo))
            {
                playerUiInfoDic[(int)camp].Remove(userUID);
                Destroy(playerUIInfo.gameObject);
            }
        }


        if (playerDic[(int)camp].Count == 0)
        {
            int random = Random.Range(111, 1000);
            LoadPlayer(camp, random, random.ToString(), null);
        }
    }

    //ʵ����Player prefab
    void LoadPlayer(CampEnum camp, long userUID, string userName, Sprite userFace)
    {
        if (playerDic[(int)camp].Count >= Config.MaxPlayer)
        {
            Debug.LogWarning($"[{userName}]����ʧ�ܣ� [{camp}] ��Ӫ����������");
            return;
        }

        Dictionary<long, Player> tmpPDic = playerDic[(int)camp];
        Dictionary<long, PlayerUIInfo> tmpPUIDic = playerUiInfoDic[(int)camp];
        if (!tmpPDic.ContainsKey(userUID))
        {
            GameObject obj = Instantiate<GameObject>(PlayerPrefab);
            obj.transform.SetParent(PlayerRoot);
            Player player = obj.GetComponent<Player>();

            player.Init(userUID, userName, userFace, camp);
            PlayerUIInfo playerUIInfo = LoadPlayerUIIngo(camp, userName, userUID, userFace, out GameObject objUI);
            if (playerUIInfo != null)
            {
                tmpPDic.Add(userUID, player);
                tmpPUIDic.Add(userUID, playerUIInfo);

            }
            else
            {
                Destroy(obj);
                Destroy(objUI);
                Debug.LogWarning($"���[{userName}] ������Ӫʧ�ܣ����Ժ�����");
            }

        }
        else
        {
            Debug.LogWarning($"���[{userName}]���Ѵ���");
        }
    }

    //ʵ����Player UI Info Prefab
    PlayerUIInfo LoadPlayerUIIngo(CampEnum camp, string username, long userUID, Sprite userface, out GameObject obj)
    {
        PlayerUIInfo playerUIInfo = null;
        Transform parent = null;
        switch (camp)
        {
            case CampEnum.Yellow:
            case CampEnum.Red:
                obj = GameObject.Instantiate<GameObject>(PlayerUIInfo_L_Prefab);
                playerUIInfo = obj.GetComponent<PlayerUIInfo>();
                parent = PlayerUIInfoRoot[(int)camp];
                if (parent != null) obj.transform.SetParent(parent);
                playerUIInfo?.Init(camp, username, userUID, userface);
                return playerUIInfo;
            case CampEnum.Blue:
            case CampEnum.Green:
                obj = GameObject.Instantiate<GameObject>(PlayerUIInfo_R_Prefab);
                playerUIInfo = obj.GetComponent<PlayerUIInfo>();
                parent = PlayerUIInfoRoot[(int)camp];
                if (parent != null) obj.transform.SetParent(parent);
                playerUIInfo?.Init(camp, username, userUID, userface);
                return playerUIInfo;
            default:
                obj = null;
                return null;
        }
    }


    bool IsExistPlayer(long userUID)
    {
        for (int i = 0; i < playerDic.Count; i++)
        {
            if (playerDic[i].ContainsKey(userUID))
            {
                return true;
            }
        }
        return false;
    }

    //ִ���������¼�
    bool isStartThrowDice = false;
    float throwTime = 2.0f;
    int campThrow = (int)CampEnum.Red;
    void ThrowDice_CallEvt()
    {
        campThrow += 1;
        if (campThrow == 4) campThrow = 0;
        EventSys.Instance.CallEvt(EventSys.ThrowDice, campThrow);
    }
    #endregion


    #region Unity Func
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (isStartThrowDice)
        {
            throwTime += Time.deltaTime;
            if (throwTime >= 2.0f)
            {
                throwTime = 0.0f;
                ThrowDice_CallEvt();
            }
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Key Down [A]");
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && audio.enabled)
            {
                if (audio.isPlaying)
                {
                    audio.Stop();
                }
                else
                {
                    audio.Play();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Key Down [S]");
            isStartThrowDice = !isStartThrowDice;

        }

        #region  For Test ��Ļ����
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Key Down [1]");
            int random = Random.Range(1, 1000);
            LoadPlayer(CampEnum.Yellow, random, random.ToString(), null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Key Down [2]");
            int random = Random.Range(1, 1000);
            LoadPlayer(CampEnum.Blue, random, random.ToString(), null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Key Down [3]");
            int random = Random.Range(1, 1000);
            LoadPlayer(CampEnum.Green, random, random.ToString(), null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Key Down [4]");
            int random = Random.Range(1, 1000);
            LoadPlayer(CampEnum.Red, random, random.ToString(), null);
        }
        #endregion

    }

    private void OnDestroy()
    {
        GameEnd();
    }

    private void OnApplicationQuit()
    {
        GameEnd();
    }
    #endregion
}
