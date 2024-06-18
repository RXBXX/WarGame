using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeMgr : Singeton<TimeMgr>
{
    private long _startTime;

    public override bool Init()
    {
        _startTime = GetUnixTimestamp();
        return true;
    }

    public long GetStartTime()
    {
        return _startTime;
    }

    public long GetGameDuration()
    {
        return GetUnixTimestamp() - _startTime;
    }

    /// <summary>
    /// ��ȡUnixʱ���
    /// ��ʱ���޹أ���ͬʱ����ͬһ�̻�ȡ��ʱ���һ��
    /// </summary>
    /// <returns></returns>
    public long GetUnixTimestamp()
    {
        //��ȡ��ǰʱ��
        DateTime now = DateTime.Now;

        return ((DateTimeOffset)now).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// ��ȡָ��unixʱ����ĸ�ʽ��ʱ���ַ���
    /// </summary>
    /// <param name="unixTimestamp"></param>
    /// <returns></returns>
    public string GetFormatDateTime(long unixTimestamp)
    {
        // ��Unixʱ�������DateTimeOffset����
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);

        // ��DateTimeOffset����ת��Ϊ����ʱ��
        DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;

        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// ��ȡ��ǰunixʱ����ĸ�ʽ��ʱ���ַ���
    /// </summary>
    /// <returns></returns>
    public string GetNowFormatDateTime()
    {
        long unixTimestamp = GetUnixTimestamp();
        // ��Unixʱ�������DateTimeOffset����
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);

        // ��DateTimeOffset����ת��Ϊ����ʱ��
        DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;

        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string GetFormatLeftTime(long leftTime)
    {
        var hours = Mathf.Floor(leftTime / 3600000);
        var minus = Mathf.Floor((leftTime - hours * 3600000)/60000);
        var seconds = MathF.Floor((leftTime - hours * 3600000 - minus * 60000) / 1000);
        return string.Format("{0}:{1}:{2}", hours, minus, seconds);
    }

    //��Ϸ��ʼʱ��
    public float GetTime()
    {
        return Time.time;
    }

    //��ǰ���������غ��ʱ��
    public float GetTimeSinceLevelLoad()
    {
        return Time.timeSinceLevelLoad;
    }
}
