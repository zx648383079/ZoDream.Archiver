using System.Collections.Generic;

namespace ZoDream.Shared.Collections
{
    public static class ListExtension
    {
        /// <summary>
        /// 添加元素并返回元素的序号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int AddWithIndex<T>(this IList<T> items, T item)
        {
            var index = items.Count;
            items.Add(item);
            return index;
        }

        public static void AddRange<T>(this IList<T> items, IEnumerable<T> data)
        {
            foreach (var item in data)
            {
                items.Add(item);
            }
        }

        public static void AddRange<T>(this IList<T> items, params T[] data)
        {
            foreach (var item in data)
            {
                items.Add(item);
            }
        }
    }
}
