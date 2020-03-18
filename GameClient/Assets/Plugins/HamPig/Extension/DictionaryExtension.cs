using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Extension
{
    public static class DictionaryExtension
    {
        public static void RemoveJudge<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> func)
        {
            if (dictionary.Count <= 0) return;

            List<TKey> removeSet = new List<TKey>();
            foreach (var pair in dictionary)
            {
                bool isRemove = func(pair.Key, pair.Value);
                if (isRemove) removeSet.Add(pair.Key);
            }

            foreach (var key in removeSet)
            {
                dictionary.Remove(key);
            }
        }
    }
}
