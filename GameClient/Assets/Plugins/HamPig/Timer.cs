using System;
using System.Collections.Generic;
using HamPig.Extension;

namespace HamPig
{
    public class Timer
    {
        private static readonly Int64 m_StartTick = DateTime.Now.Ticks;
        private static LinkedList<OnceEvent> m_OnceEvents = new LinkedList<OnceEvent>();
        private static LinkedList<IntervalEvent> m_IntervalEvent = new LinkedList<IntervalEvent>();

        /// <summary>
        /// 从程序启动到当前所经过的秒数
        /// </summary>
        public static float time
        {
            get
            {
                Int64 currTick = DateTime.Now.Ticks;
                return (currTick - m_StartTick) / 10000 / 1000.0f;
            }
        }

        public static void CallOnce(float interval, Action action)
        {
            m_OnceEvents.AddLast(new OnceEvent
            {
                action = action,
                timeCall = Timer.time + interval,
            });
        }

        public static void CallInterval(float interval, Action action)
        {
            m_IntervalEvent.AddLast(new IntervalEvent
            {
                action = action,
                timeCall = Timer.time + interval,
                interval = interval,
            });
        }

        public static void RemoveOnce(Action action)
        {
            m_OnceEvents.RemoveJudge((OnceEvent item) => { return (item.action == action); });
        }

        public static void RemoveInterval(Action action)
        {
            m_IntervalEvent.RemoveJudge((IntervalEvent item) => { return (item.action == action); });
        }

        public static void Update()
        {
            float timeCurr = Timer.time;
            m_OnceEvents.RemoveJudge(delegate (OnceEvent item)
            {
                bool isCall = (timeCurr >= item.timeCall);
                if (isCall)
                {
                    item.action.Invoke();
                }
                return isCall;
            });
            foreach (var item in m_IntervalEvent)
            {
                if (timeCurr >= item.timeCall)
                {
                    item.action.Invoke();
                    item.timeCall += item.interval;
                }
            }
        }

        public class OnceEvent
        {
            public Action action;
            public float timeCall;
        }

        public class IntervalEvent
        {
            public Action action;
            public float timeCall;
            public float interval;
        }
    }
}
