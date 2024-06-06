using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeMgr : Singeton<TimeMgr>
{
    /// <summary>
    /// ��ȡUnixʱ���
    /// ��ʱ���޹أ���ͬʱ����ͬһ�̻�ȡ��ʱ���һ��
    /// </summary>
    /// <returns></returns>
    public long GetUnixTimestamp()
    {
        //��ȡ��ǰʱ��
        DateTime now = DateTime.Now;

        return ((DateTimeOffset)now).ToUnixTimeSeconds();
    }

    /// <summary>
    /// ��ȡָ��unixʱ����ĸ�ʽ��ʱ���ַ���
    /// </summary>
    /// <param name="unixTimestamp"></param>
    /// <returns></returns>
    public string GetFormatDateTime(long unixTimestamp)
    {
        // ��Unixʱ�������DateTimeOffset����
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

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
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        // ��DateTimeOffset����ת��Ϊ����ʱ��
        DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;

        return localDateTime.ToString("yyyy-MM-dd HH:mm:ss");
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
