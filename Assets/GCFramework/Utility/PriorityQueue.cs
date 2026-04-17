using System;
using System.Collections.Generic;
using System.Linq;

namespace GCFramework.Utility
{
    /// <summary>
    /// 优先队列
    /// </summary>
    public class PriorityQueue<T>
    { 
        private List<T> _heap;
        private Comparison<T> _comparison;

        /// <summary>
        /// 创建优先队列，值越大的在堆顶
        /// </summary>
        /// <param itemName="comparison">(x , y) x 在左边, y在有右边: x > y</param>
        public PriorityQueue(Comparison<T> comparison)
        {
            _heap = new List<T>();
            _comparison = comparison;
        }

        /// <summary>
        /// 创建优先队列，将数组堆化
        /// </summary>
        /// <param itemName="arr"></param>
        /// <param itemName="comparison"></param>
        public PriorityQueue(T[] arr, Comparison<T> comparison)
        {
            _heap = new List<T>(arr);
            _comparison = comparison;
            for (int i = (GetParent(arr.Length - 1)); i >= 0; i--)
                MoveDown(i);
        }

        #region 公开方法

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(T element)
        {
            _heap.Add(element);
            MoveUp(_heap.Count - 1);
        }

        /// <summary>
        /// 移除优先队列的队首元素
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T DeleteHeapTop()
        {
            if (_heap.Count == 0)
                throw new ArgumentNullException($"优先队列里没有元素可移除！");
            return MoveDown();
        }

        /// <summary>
        /// 将优先队列的队首进行替换
        /// </summary>
        /// <param itemName="element"></param>
        /// <returns></returns>
        public T Replace(T element)
        {
            T top = _heap.First();
            _heap[0] = element;
            MoveDown();
            return top;
        }

        public T GetHeapTop()
        {
            if (_heap.Count == 0) return default;
            return _heap.First();
        }

        public int GetSize() => _heap.Count;

        public T[] GetArray() => _heap.ToArray();

        #endregion
        

        /// <summary>
        /// 调整堆结构，让插入元素上移
        /// </summary>
        /// <param itemName="eIndex">插入元素的索引</param>
        private void MoveUp(int eIndex)
        {
            int curIndex = eIndex;
            while (curIndex > 0)
            {
                int parentIndex = GetParent(curIndex);
                if(_comparison(_heap[parentIndex], _heap[curIndex]) >= 0)
                    break;
                SwapElement(parentIndex, curIndex);
                curIndex = parentIndex;
            }
        }

        /// <summary>
        /// 调整堆结构，让堆顶元素下移，会删除堆顶元素
        /// </summary>
        private T MoveDown()
        {
            T result = _heap.First();
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            MoveDownElement(0);

            return result;
        }

        /// <summary>
        /// 调整堆结构，让指定元素下移，不会删除元素
        /// </summary>
        /// <param itemName="eIndex"></param>
        private void MoveDown(int eIndex)
        {
            int curIndex = eIndex;
            if(curIndex < 0 || curIndex > _heap.Count - 1) return;

            MoveDownElement(curIndex);
        }

        /// <summary>
        /// 下移指定元素
        /// </summary>
        /// <param itemName="eIndex"></param>
        private void MoveDownElement(int eIndex)
        {
            int curIndex = eIndex;
            int leftChildIdx;
            int rightChildIdx;
            int minIdx;
            while (true)
            {
                // 数组映射二叉树
                leftChildIdx = 2 * curIndex + 1; // 它的左子节点索引 2 * i + 1
                rightChildIdx = 2 * curIndex + 2; // 它的右子节点索引 2 * i + 2
                minIdx = curIndex;

                if (leftChildIdx < _heap.Count && _comparison(_heap[leftChildIdx], _heap[minIdx]) > 0)
                    minIdx = leftChildIdx;
                if (rightChildIdx < _heap.Count && _comparison(_heap[rightChildIdx], _heap[minIdx]) > 0)
                    minIdx = rightChildIdx;
                
                if(minIdx == curIndex)
                    break;
                
                SwapElement(curIndex, minIdx);
                curIndex = minIdx;
            }
        }

        private void SwapElement(int idx1, int idx2)
        {
            (_heap[idx1], _heap[idx2]) = (_heap[idx2], _heap[idx1]);
        }

        /// <summary>
        /// 数组映射二叉树，父节点索引 (i - 1) / 2
        /// </summary>
        /// <param itemName="eIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int GetParent(int eIndex)
        {
            int parentIndex = (eIndex - 1) / 2;
            if (parentIndex < 0 || parentIndex >= _heap.Count) 
                throw new ArgumentOutOfRangeException($"下标: {eIndex}获取父亲元素失败！");
            return parentIndex;
        }
        
    }
}