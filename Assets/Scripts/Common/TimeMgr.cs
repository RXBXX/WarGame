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
        /// 获取Unix时间戳
        /// 和时区无关，不同时区在同一刻获取的时间戳一致
        /// </summary>
        /// <returns></returns>
        public long GetUnixTimestamp()
        {
            //获取当前时间
            DateTime now = DateTime.Now;

            return ((DateTimeOffset)now).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取指定unix时间戳的格式化时间字符串
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public string GetFormatDateTime(long unixTimestamp)
        {
            // 从Unix时间戳创建DateTimeOffset对象
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);

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
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);

            // 将DateTimeOffset对象转换为本地时间
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

        //获取游戏内当前时刻
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
