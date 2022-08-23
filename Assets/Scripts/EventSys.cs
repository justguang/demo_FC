using System;
using System.Collections.Generic;

public class EventSys
{
    static EventSys _ins;
    public static EventSys Instance
    {
        get
        {
            if (_ins == null)
            {
                _ins = new EventSys();
            }
            return _ins;
        }
    }
    private EventSys() { }


    private Dictionary<string, Action<object>> evtDic = new Dictionary<string, Action<object>>();


    public void AddEvt(string evtCode, Action<object> callback)
    {
        if (!evtDic.ContainsKey(evtCode))
        {
            evtDic.Add(evtCode, callback);
        }
        else
        {
            RemoveEvt(evtCode, callback);
            evtDic[evtCode] += callback;
        }
    }

    public void RemoveEvt(string evtCode, Action<object> callback)
    {
        if (evtDic.ContainsKey(evtCode))
        {
            evtDic[evtCode] -= callback;
        }
    }

    public void CallEvt(string evtCode, object param)
    {
        if (evtDic.ContainsKey(evtCode))
        {
            evtDic[evtCode]?.Invoke(param);
        }
    }








    #region Event Code
    public static readonly string test = "test";
    public static readonly string ThrowDice = "掷骰子";
    public static readonly string ThrowDice_OK = "掷骰子结束";


    public static readonly string FlyToHit = "飞机飞行意外撞击";

    public static readonly string Winner = "到达终点";
    #endregion
}
