using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //所有顺序按照枚举 CampEnum 排序 0、1、2、3 =》yellow、blue、green、red
    [Header("玩家根节点")]
    public Transform PlayerRoot;
    [Header("等待区根节点")]
    public Transform[] WaitPoint;
    [Header("起飞区根节点")]
    public Transform[] StartPoint;

    [Header("环形路径根节点")]
    public Transform PathRoot;
    [Header("环形路径数据")]
    public List<Node> PathList;
    [Header("终段路径根节点")]
    public Transform[] EndPath;

    [Header("Player Prefab")]
    public GameObject PlayerPrefab;

    [Space(10)]

    #region UI
    public Transform UIRoot;
    public Transform LoginPanel;
    public Transform PlayPanel;
    #endregion

    //所有Player【key => 阵营 =>  value<userUID,Player>】
    private Dictionary<int, Dictionary<long, Player>> playerDic;

    public void Init()
    {
        Instance = this;

        if (UIRoot == null) UIRoot = GameObject.Find("Canvas").transform;
        if (LoginPanel == null) LoginPanel = GameObject.Find("LoginPanel").transform;
        if (PlayPanel == null) PlayPanel = GameObject.Find("PlayPanel").transform;

        //UIRoot.gameObject.SetActive(true);
        //PlayPanel.gameObject.SetActive(false);
        //LoginPanel.gameObject.SetActive(true);

        BilibiliLoginManager loginMgr = LoginPanel.Find("loginUI").GetComponent<BilibiliLoginManager>();
        loginMgr?.LinkFailedEvent.AddListener(OnLinkFailedEvt);
        loginMgr?.LinkSuccessEvent.AddListener(OnLinkSuccessEvt);

        playerDic = new Dictionary<int, Dictionary<long, Player>>();
        playerDic.Add((int)CampEnum.Yellow, new Dictionary<long, Player>());
        playerDic.Add((int)CampEnum.Blue, new Dictionary<long, Player>());
        playerDic.Add((int)CampEnum.Green, new Dictionary<long, Player>());
        playerDic.Add((int)CampEnum.Red, new Dictionary<long, Player>());
    }


    [ContextMenu("生成环形路径数据")]
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
        Debug.Log("链接成功");
    }

    void OnLinkFailedEvt()
    {
        PlayPanel.gameObject.SetActive(false);
        LoginPanel.gameObject.SetActive(true);
        Debug.LogWarning("链接失败");
    }

    void GameEnd()
    {
        ConnectViaCode.Instance?.LinkEnd();
    }

    void LoadPlayer(CampEnum camp, long userUID, string userName, string userFace)
    {
        if (playerDic[(int)camp].Count >= Config.MaxPlayer)
        {
            Debug.LogWarning($"[{userName}]加入失败， [{camp}] 阵营人数已满。");
            return;
        }

        Dictionary<long, Player> tmpDic = playerDic[(int)camp];
        if (!tmpDic.ContainsKey(userUID))
        {
            GameObject obj = Instantiate<GameObject>(PlayerPrefab);
            obj.transform.SetParent(PlayerRoot);
            Player player = obj.GetComponent<Player>();

            if (player.Init(userUID, userName, userFace, camp))
            {
                tmpDic.Add(userUID, player);
            }
            else
            {
                Destroy(obj);
                Debug.LogWarning($"玩家[{userName}] 加入阵营失败，错误的命令");
            }

        }
        else
        {
            Debug.LogWarning($"玩家[{userName}]，已存在");
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
        }

        #region  For Test 弹幕命令
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
