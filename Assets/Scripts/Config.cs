using System;
using System.Linq;

public static class Config
{
    //true全屏
    public const bool isFullScreen = false;

    //屏幕分辨率
    public const int ScreenResolution_width = 1320;
    public const int ScreenResolution_height = 890;

    //帧率锁定
    public const int FrameRate = 60;
    //等待区位置偏移
    public const float OffsetWaitPos = 65f;
    //每个阵营最多人数限制
    public const int MaxPlayer = 4;
    //正常走环形路径步数
    public const int MaxMoveCount = 50;
    //每步之间的时间间隔【单位秒】
    public const float MoveWaitTime = 0.42f;
    //每次掷骰子时间间隔【单位秒】
    public const float ThrowDiceWaitTime = 6.5f;


    #region 弹幕命令
    public const string Join_Yellow = "加入黄";
    public const string Join_Blue = "加入蓝";
    public const string Join_Green = "加入绿";
    public const string Join_Red = "加入红";
    #endregion
}

