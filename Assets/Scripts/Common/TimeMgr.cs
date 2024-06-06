using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeMgr : Singeton<TimeMgr>
{
    /// <summary>
    /// 获取Unix时间戳
    /// 和时区无关，不同时区在同一刻获取的时间戳一致
    /// </summary>
    /// <returns></returns>
    public long GetUnixTimestamp()
    {
        //获取当前时间
        DateTime now = DateTime.Now;

        return ((DateTimeOffset)now).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 获取指定unix时间戳的格式化时间字符串
    /// </summary>
    /// <param name="unixTimestamp"></param>
    /// <returns></returns>
    public string GetFormatDateTime(long unixTimestamp)
    {
        // 从Unix时间戳创建DateTimeOffset对象
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        // 将DateTimeOffset对象转换为本地时间
        DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;

        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 获取当前unix时间戳的格式化时间字符串
    /// </summary>
    /// <returns></returns>
    public string GetNowFormatDateTime()
    {
        long unixTimestamp = GetUnixTimestamp();
        // 从Unix时间戳创建DateTimeOffset对象
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        // 将DateTimeOffset对象转换为本地时间
        DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;

        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    //游戏开始时间
    public float GetTime()
    {
        return Time.time;
    }

    //当前场景被加载后的时间
    public float GetTimeSinceLevelLoad()
    {
        return Time.timeSinceLevelLoad;
    }
}
