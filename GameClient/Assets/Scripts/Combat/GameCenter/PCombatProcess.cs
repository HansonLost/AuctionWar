using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CombatGameCenter
{
    public class ProcessFactory
    {
        private List<ProcessBlock> m_Blocks = new List<ProcessBlock>();

        /// <summary>
        /// 在第[Int32]个加工块，更新信息为[ProcessBlock]，剩余材料为[Material]
        /// </summary>
        public Action<Int32, ProcessBlock, Material> onAddMaterial;
        /// <summary>
        /// 第[Int32]个加工块开始运行
        /// </summary>
        public Action<Int32> onBeginRun;

        public ProcessBlock GetProcessBlock(Int32 idx)
        {
            if (Utility.IsInRange(idx, 0, m_Blocks.Count - 1))
                return m_Blocks[idx];
            return null;
        }
        public void AddQuest(Quest quest)
        {
            if (m_Blocks.Count >= GameConst.COUNT_CLAIM_QUEST) return;
            var block = new ProcessBlock
            {
                quest = quest,
                matBuffer = new List<int>(),
                isRun = false,
                startSeq = 0,
            };
            for (int i = 0; i < quest.materials.Count; i++)
            {
                block.matBuffer.Add(0);
            }
            m_Blocks.Add(block);
        }
        public void AddMaterial(Int32 idx, Material mat)
        {
            if (!Utility.IsInRange(idx, 0, m_Blocks.Count - 1)) return;
            var block = m_Blocks[idx];

            Int32 questIdx = 0;
            Material leftMat = mat;
            bool isCanRun = false;
            if (!block.isRun)
            {
                foreach (var questMat in block.quest.materials)
                {
                    if (questMat.type == mat.type)
                    {
                        Int32 currCount = block.matBuffer[questIdx];
                        Int32 nextCount = currCount + mat.count;
                        isCanRun = (nextCount >= questMat.count);
                        Int32 leftCount = (isCanRun ? nextCount - questMat.count : 0);
                        block.matBuffer[questIdx] = nextCount - leftCount;

                        leftMat = new Material(mat.type, leftCount);
                        break;
                    }
                    questIdx++;
                }
            }
            onAddMaterial?.Invoke(idx, block, leftMat);

            if(isCanRun)
            {
                block.isRun = true;
                block.startSeq = CombatFrameManager.instance.seq;
                onBeginRun?.Invoke(idx);
            }
        }


        public class ProcessBlock
        {
            public Quest quest;
            public List<Int32> matBuffer;
            public bool isRun;
            public Int32 startSeq;
        }
    }
}
