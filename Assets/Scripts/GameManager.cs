using OpenBLive.Runtime.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

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
    [Header("Player UI 信息显示根节点")]
    public Transform PlayerUILayoutGroup;
    [Header("排行信息")]
    public Transform TimeRankLayoutGroup;

    [Header("掷骰子面板")]
    public Transform[] PlayerThrowDicePanel;
    [Header("掷骰子动画")]
    public Animator[] PlayerThrowDiceAni;

    [Header("环形路径根节点")]
    public Transform PathRoot;
    [Header("环形路径数据")]
    public List<Node> PathList;
    [Header("终段路径根节点")]
    public Transform[] EndPath;


    [Header("Player Prefab")]
    public GameObject Player_Prefab;
    public GameObject PlayerUI_Prefab;
    public GameObject PlayerThrowDice_Prefab;
    public GameObject PlayerTimeRankInfo_Prefab;

    [Space(10)]

    #region UI
    public Transform UIRoot;
    public Transform LoginPanel;
    public Transform PlayPanel;
    #endregion

    //所有Player【key => 阵营 =>  value<userUID,Player>】
    private Dictionary<int, Dictionary<long, Player>> playerDic;
    private Dictionary<int, Dictionary<long, PlayerUI>> playerUIDic;
    private Dictionary<int, Dictionary<long, PlayerThrowDice>> playerThrowDiceDic;
    //player排行信息
    private List<PlayerTimeRankInfo> playerTimeRankList;

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

        //init Collections
        {
            playerDic = new Dictionary<int, Dictionary<long, Player>>();
            playerDic.Add((int)CampEnum.Yellow, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Blue, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Green, new Dictionary<long, Player>());
            playerDic.Add((int)CampEnum.Red, new Dictionary<long, Player>());

            playerUIDic = new Dictionary<int, Dictionary<long, PlayerUI>>();
            playerUIDic.Add((int)CampEnum.Yellow, new Dictionary<long, PlayerUI>());
            playerUIDic.Add((int)CampEnum.Blue, new Dictionary<long, PlayerUI>());
            playerUIDic.Add((int)CampEnum.Green, new Dictionary<long, PlayerUI>());
            playerUIDic.Add((int)CampEnum.Red, new Dictionary<long, PlayerUI>());

            playerThrowDiceDic = new Dictionary<int, Dictionary<long, PlayerThrowDice>>();
            playerThrowDiceDic.Add((int)CampEnum.Yellow, new Dictionary<long, PlayerThrowDice>());
            playerThrowDiceDic.Add((int)CampEnum.Blue, new Dictionary<long, PlayerThrowDice>());
            playerThrowDiceDic.Add((int)CampEnum.Green, new Dictionary<long, PlayerThrowDice>());
            playerThrowDiceDic.Add((int)CampEnum.Red, new Dictionary<long, PlayerThrowDice>());

            playerTimeRankList = new List<PlayerTimeRankInfo>();
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
            EventSys.Instance.AddEvt(EventSys.ThrowDice_OK, OnThrowDice_OKEvt);
            EventSys.Instance.AddEvt(EventSys.Winner, OnWinnerEvt);
        }

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

    #region On Danmaku Event
    //弹幕
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
                    //已存在
                    Debug.LogWarning($"{dm.userName} 已存在，加入错误");
                }
                break;
            case Config.Join_Blue:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Blue, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //已存在
                    Debug.LogWarning($"{dm.userName} 已存在，加入错误");
                }
                break;
            case Config.Join_Green:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Green, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //已存在
                    Debug.LogWarning($"{dm.userName} 已存在，加入错误");
                }
                break;
            case Config.Join_Red:
                if (IsExistPlayer(dm.uid) == false)
                {
                    StartCoroutine(DoLoadPlayer(CampEnum.Red, dm.uid, dm.userName, dm.userFace));
                }
                else
                {
                    //已存在
                    Debug.LogWarning($"{dm.userName} 已存在，加入错误");
                }
                break;
            default:
                break;
        }

    }

    //送礼物
    void OnGiftEvt(SendGift sendGift)
    {

    }

    //收到大航海
    void OnGuard(Guard guard)
    {

    }

    //收到SC
    void OnSuperChat(SuperChat sc)
    {

    }
    #endregion


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

        Debug.Log("链接成功，游戏开始");
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

    #region EventSys Callback
    /// <summary>
    /// 掷骰子结束事件
    /// </summary>
    /// <param name="obj">【0】阵营，【1】userUID，【2】骰子点数</param>
    void OnThrowDice_OKEvt(object obj)
    {
        CampEnum camp = (CampEnum)((object[])obj)[0];
        PlayerThrowDiceAni[(int)camp]?.Play("Hide");
    }

    /// <summary>
    /// 有玩家到达终点
    /// </summary>
    /// <param name="obj">[0]=>玩家阵营，[1]=>玩家UID</param>
    void OnWinnerEvt(object obj)
    {
        object[] result = (object[])obj;
        CampEnum camp = (CampEnum)result[0];
        long userUID = (long)result[1];

        Player player = null;
        PlayerUI playerUI = null;
        PlayerThrowDice playerTD = null;

        //清除player信息
        if (playerDic.ContainsKey((int)camp))
        {
            if (playerDic[(int)camp].TryGetValue(userUID, out player))
            {
                playerDic[(int)camp].Remove(userUID);
            }
        }
        //清除playerUI信息
        if (playerUIDic.ContainsKey((int)camp))
        {
            if (playerUIDic[(int)camp].TryGetValue(userUID, out playerUI))
            {
                playerUIDic[(int)camp].Remove(userUID);
            }
        }
        //清除PlayerThrowDice
        if (playerThrowDiceDic.ContainsKey((int)camp))
        {
            if (playerThrowDiceDic[(int)camp].TryGetValue(userUID, out playerTD))
            {
                playerThrowDiceDic[(int)camp].Remove(userUID);
            }
        }

        //destory gameObject
        if (player != null) Destroy(player.gameObject);
        if (playerTD != null) Destroy(playerTD.gameObject);
        if (playerUI != null)
        {
            //计算通关用时
            float endTime = Time.realtimeSinceStartup;
            float useTime = endTime - playerUI.StartTime;

            //计算刷新排行信息
            CalcTimeRank(camp, playerUI.UserName, playerUI.UserUID, useTime, playerUI.UserFace);

            Destroy(playerUI.gameObject);
        }

        //如果该阵营人数=0，随机生成人机加入
        if (playerDic[(int)camp].Count == 0)
        {
            int random = Random.Range(111, 1000);
            LoadPlayer(camp, random, random.ToString(), null);
        }
    }
    #endregion

    //计算排行信息
    void CalcTimeRank(CampEnum camp, string userName, long userUID, float useTime, Sprite userFace)
    {
        PlayerTimeRankInfo playerTRI = null;
        if (playerTimeRankList.Count == 0)
        {
            playerTRI = LoadPlayerTimeRankInfo(camp, userName, userUID, useTime, userFace);
            playerTimeRankList.Add(playerTRI);
        }
        else
        {
            //playerTRI 不为空 =》 历史在榜
            playerTimeRankList.Find(p =>
            {
                if (p.userUID == userUID)
                {
                    playerTRI = p;
                    return p;
                }
                return false;
            });

            if (playerTRI != null)
            {
                playerTRI.UpdateUseTime(useTime);//刷新在榜
            }
            else
            {
                //上榜
                playerTRI = LoadPlayerTimeRankInfo(camp, userName, userUID, useTime, userFace);
                playerTimeRankList.Add(playerTRI);
                playerTimeRankList.Sort((p1, p2) =>
                {
                    return p1.UserTime.CompareTo(p2.UserTime);
                });
            }

            //清除4名开外
            PlayerTimeRankInfo tmp = null;
            for (int i = 4; i < playerTimeRankList.Count; i++)
            {
                tmp = playerTimeRankList[i];
                if (tmp != null)
                {
                    playerTimeRankList.Remove(tmp);
                    Destroy(tmp.gameObject);
                    tmp = null;
                }
            }

        }

    }

    #region Load Prefab
    //加载Player
    IEnumerator DoLoadPlayer(CampEnum camp, long userUID, string userName, string userFace)
    {

        Texture2D loadTexture = null;
        Sprite loadFace = null;
        using (UnityWebRequest webReq = new UnityWebRequest())
        {
            //加载头像
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

    //实例化Player prefab
    void LoadPlayer(CampEnum camp, long userUID, string userName, Sprite userFace)
    {
        if (playerDic[(int)camp].Count >= Config.MaxPlayer)
        {
            Debug.LogWarning($"[{userName}]加入失败， [{camp}] 阵营人数已满。");
            return;
        }

        if (!playerDic[(int)camp].ContainsKey(userUID))
        {
            GameObject obj = Instantiate<GameObject>(Player_Prefab);
            obj.name = userName;
            obj.transform.SetParent(PlayerRoot);
            obj.transform.localScale = Vector3.one;
            Player player = obj.GetComponent<Player>();

            player.Init(userUID, userName, userFace, camp);
            PlayerUI playerUI = LoadPlayerUI(camp, userName, userUID, userFace, out GameObject playerUIObj);
            PlayerThrowDice playerTD = LoadPlayerThrowDice(camp, userName, userUID, userFace, out GameObject playerTDObj);
            if (playerUI != null && playerTD != null)
            {
                playerDic[(int)camp].Add(userUID, player);
                playerUIDic[(int)camp].Add(userUID, playerUI);
                playerThrowDiceDic[(int)camp].Add(userUID, playerTD);
            }
            else
            {
                Destroy(obj);
                Destroy(playerUIObj);
                Destroy(playerTDObj);
                Debug.LogWarning($"玩家[{userName}] 加入阵营失败，请稍后重试");
            }

        }
        else
        {
            Debug.LogWarning($"玩家[{userName}]，已存在");
        }
    }

    //实例化Player UI Info Prefab
    PlayerUI LoadPlayerUI(CampEnum camp, string userName, long userUID, Sprite userface, out GameObject obj)
    {
        PlayerUI playerUI = null;
        obj = null;

        obj = GameObject.Instantiate<GameObject>(PlayerUI_Prefab);
        obj.name = userName;
        playerUI = obj.GetComponent<PlayerUI>();
        obj.transform.SetParent(PlayerUILayoutGroup);
        obj.transform.localScale = Vector3.one;
        playerUI?.Init(camp, userName, userUID, userface);
        return playerUI;

    }

    //实例化PlayerThrowDice Prefab
    PlayerThrowDice LoadPlayerThrowDice(CampEnum camp, string userName, long userUID, Sprite userFace, out GameObject obj)
    {
        PlayerThrowDice playerThrowDice = null;
        obj = null;
        obj = GameObject.Instantiate<GameObject>(PlayerThrowDice_Prefab);
        obj.name = userName;
        playerThrowDice = obj.GetComponent<PlayerThrowDice>();
        obj.transform.SetParent(PlayerThrowDicePanel[(int)camp]);
        obj.transform.localScale = Vector3.one;
        playerThrowDice?.Init(camp, userName, userUID, userFace);
        return playerThrowDice;
    }

    //实例化PlayerTimeRankInfo
    PlayerTimeRankInfo LoadPlayerTimeRankInfo(CampEnum camp, string userName, long userUID, float useTime, Sprite userFace)
    {
        GameObject obj = GameObject.Instantiate(PlayerTimeRankInfo_Prefab);
        obj.name = userName;
        PlayerTimeRankInfo playerTRI = obj.GetComponent<PlayerTimeRankInfo>();
        obj.transform.SetParent(TimeRankLayoutGroup);
        obj.transform.localScale = Vector3.one;
        playerTRI.Init(camp, userName, userUID, useTime, userFace);
        return playerTRI;
    }
    #endregion

    //检查指定UID是否存在
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


    //执行掷骰子事件
    bool isStartThrowDice = false;
    float throwTime = Config.ThrowDiceWaitTime;
    int campThrow = (int)CampEnum.Red;
    void ThrowDice_CallEvt()
    {
        campThrow += 1;
        if (campThrow == 4) campThrow = 0;
        PlayerThrowDiceAni[(int)campThrow]?.Play("Show");
        EventSys.Instance.CallEvt(EventSys.ThrowDice, campThrow);
    }
    #endregion


    #region Unity Func
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = Config.FrameRate;//帧率
        Screen.fullScreen = Config.isFullScreen;//是否全屏
        //屏幕分辨率
        Screen.SetResolution(Config.ScreenResolution_width, Config.ScreenResolution_height, Config.isFullScreen);

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (isStartThrowDice)
        {
            throwTime += Time.deltaTime;
            if (throwTime >= Config.ThrowDiceWaitTime)
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
