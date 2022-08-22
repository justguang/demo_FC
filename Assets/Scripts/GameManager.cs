using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

            //UIRoot.gameObject.SetActive(true);
            //PlayPanel.gameObject.SetActive(false);
            //LoginPanel.gameObject.SetActive(true);

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
    void OnLinkSuccessEvt()
    {
        LoginPanel.gameObject.SetActive(false);
        PlayPanel.gameObject.SetActive(true);
        Debug.Log("���ӳɹ�");
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

    //ʵ����Player prefab
    void LoadPlayer(CampEnum camp, long userUID, string userName, string userFace)
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
    PlayerUIInfo LoadPlayerUIIngo(CampEnum camp, string username, long userUID, string userface, out GameObject obj)
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
    #endregion


    #region Unity Func
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Key Down [A]");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Key Down [S]");
            EventSys.Instance.CallEvt(EventSys.Left_Up_ThrowDice, null);
            EventSys.Instance.CallEvt(EventSys.Right_Up_ThrowDice, null);
            EventSys.Instance.CallEvt(EventSys.Right_Bottom_ThrowDice, null);
            EventSys.Instance.CallEvt(EventSys.Left_Bottom_ThrowDice, null);
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
            LoadPlayer(CampEnum.Green, random, random.ToString(), "");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Key Down [4]");
            int random = Random.Range(1, 1000);
            LoadPlayer(CampEnum.Red, random, random.ToString(), "");
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
