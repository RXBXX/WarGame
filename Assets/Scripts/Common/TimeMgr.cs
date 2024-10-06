using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public class TimeMgr : Singeton<TimeMgr>
    {
        private long _startTime;
        private float day = 2400f;

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
            int hours = Mathf.FloorToInt(leftTime / 3600000);
            int minus = Mathf.FloorToInt((leftTime - hours * 3600000) / 60000);
            int seconds = Mathf.FloorToInt((leftTime - hours * 3600000 - minus * 60000) / 1000);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minus, seconds);
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

        //��ȡ��Ϸ�ڵ�ǰʱ��
        public float GetGameTime()
        {
            return DatasMgr.Instance.GetGameTime();
        }

        public float GetGameTimePercent()
        {
            var time = TimeMgr.Instance.GetGameTime();
            return time % day / day;
        }

        public Enum.DayType GetDayType()
        {
            var timePer = GetGameTimePercent();
            return timePer <= 0.5F ? Enum.DayType.Day : Enum.DayType.Night;
        }
    }
}
