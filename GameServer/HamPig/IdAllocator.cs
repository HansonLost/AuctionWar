using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig
{
    public class IdAllocator
    {
        private HashSet<UInt32> m_IdSet = new HashSet<UInt32>();
        private UInt32 m_MinId;
        private UInt32 m_MaxId;
        private UInt32 m_MaxCount;
        private UInt32 m_NextId;

        public IdAllocator(UInt32 min, UInt32 max)
        {
            this.m_MinId = min;
            this.m_MaxId = max;
            this.m_MaxCount = max - min + 1;
            this.m_NextId = min;
        }

        public bool IsFull() { return m_IdSet.Count >= m_MaxCount; }

        public UInt32 GetId()
        {
            if (this.IsFull()) return 0;
            while (m_IdSet.Contains(m_NextId))
            {
                m_NextId = (m_NextId - m_MinId + 1) % m_MaxCount + m_MinId;
            }
            m_IdSet.Add(m_NextId);
            return m_NextId;
        }
        public void RestoreId(UInt32 id)
        {
            if (m_IdSet.Contains(id))
            {
                m_IdSet.Remove(id);
            }
        }
    }
}
