using System;
using System.Collections.Generic;
using HamPig.Extension;

namespace HamPig
{
    public class Timer
    {
        private static readonly Int64 m_StartTick = DateTime.Now.Ticks;
        private static Dictionary<Handle, OnceEvent> m_OnceEvents = new Dictionary<Handle, OnceEvent>();
        private static Dictionary<Handle, IntervalEvent> m_IntervalEvent = new Dictionary<Handle, IntervalEvent>();

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

        public static Handle CallOnce(float interval, Action action)
        {
            Handle handle = new Handle();
            m_OnceEvents.Add(handle, new OnceEvent
            {
                action = action,
                timeCall = Timer.time + interval,
                isStop = false,
            });
            return handle;
        }

        public static Handle CallInterval(float interval, Action action)
        {
            Handle handle = new Handle();
            m_IntervalEvent.Add(handle, new IntervalEvent
            {
                action = action,
                timeCall = Timer.time + interval,
                interval = interval,
            });
            return handle;
        }

        public static void Remove(Handle handle)
        {
            RemoveOnce(handle);
            RemoveInterval(handle);
        }

        public static void RemoveOnce(Handle handle)
        {
            /* 
             * Once 有可能被委托含有 RemoveOnce 的函数，从而导致在 Once 进行 foreach 时进行 remove 操作。
             * 因此，需要需要延迟移除，在 Update 时才统一进行移除。
             * 为了能够移除也是同一时间触发的事件，采用在 Event 中加字段来禁止执行。
             */
            if (!m_OnceEvents.ContainsKey(handle)) return;
            m_OnceEvents[handle].isStop = true;
        }

        public static void RemoveInterval(Handle handle)
        {
            if (!m_IntervalEvent.ContainsKey(handle)) return;
            m_IntervalEvent[handle].isStop = true;
        }

        public static void Update()
        {
            float timeCurr = Timer.time;
            m_OnceEvents.RemoveJudge(delegate (Handle handle, OnceEvent onceEvent)
            {
                if (onceEvent.isStop) return true;

                bool isCall = (timeCurr >= onceEvent.timeCall);
                if (isCall)
                {
                    onceEvent.action.Invoke();
                }
                return isCall;
            });
            m_IntervalEvent.RemoveJudge((Handle handle, IntervalEvent intervalEvent) =>
            {
                if (intervalEvent.isStop) return true;

                if (timeCurr >= intervalEvent.timeCall)
                {
                    intervalEvent.action.Invoke();
                    intervalEvent.timeCall += intervalEvent.interval;
                }
                return false;
            });
        }

        public class Handle { }

        public class OnceEvent
        {
            public Action action;
            public float timeCall;
            public bool isStop;
        }

        public class IntervalEvent
        {
            public Action action;
            public float timeCall;
            public float interval;
            public bool isStop;
        }
    }
}
