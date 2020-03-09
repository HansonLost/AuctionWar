using System.Collections.Generic;
using System;

namespace HamPig.Extension
{
    public static class LinkedListExtension
    {
        public static void RemoveJudge<T>(this LinkedList<T> list, Func<T, bool> func)
        {
            if (list.Count <= 0) return;

            var nodeNow = list.First;
            var nodeLast = list.Last;
            bool isEntire = false;
            while (!isEntire)
            {
                isEntire = (nodeNow == nodeLast);

                bool isRemove = func(nodeNow.Value);
                if (isRemove)
                {
                    var nodeRemove = nodeNow;
                    nodeNow = nodeNow.Next;
                    list.Remove(nodeRemove);
                }
                else
                {
                    nodeNow = nodeNow.Next;
                }
            }
        }
    }
}
