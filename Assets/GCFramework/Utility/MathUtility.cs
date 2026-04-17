using System;
using System.Collections.Generic;

namespace GCFramework.Utility
{
    public static class MathUtility
    {
        #region 快速排序

        public static void QuickSort<T>(ref T[] arr, Comparison<T> comparison)
        {
            Quick(ref arr, 0, arr.Length - 1, comparison);
        }

        public static void QuickSortByStack<T>(ref T[] arr, Comparison<T> comparison)
        {
            QuickByStack(ref arr, 0, arr.Length - 1, comparison);
        }

        #region 递归实现

        private static void Quick<T>(ref T[] arr, int left, int right, Comparison<T> comparison)
        {
            if (left < right)
            {
                int pivot = GetMinPivot(ref arr, left, right, comparison);
                Quick(ref arr, left, pivot - 1, comparison);
                Quick(ref arr, pivot + 1, right, comparison);
            }
        }

        #endregion

        #region 栈实现

        private static void QuickByStack<T>(ref T[] arr, int left, int right, Comparison<T> comparison)
        {
            Stack<int> st = new Stack<int>();
            st.Push(left);
            st.Push(right);
            while (st.Count > 0)
            {
                right = st.Pop();
                left = st.Pop();
                int pivot = Partition(ref arr, left, right, comparison);
                if (pivot - 1 > left)
                {
                    st.Push(left);
                    st.Push(pivot - 1);
                }

                if (pivot + 1 < right)
                {
                    st.Push(pivot + 1);
                    st.Push(right);
                }
                
                // string log = "";
                // foreach (var e in st)
                // {
                //     log += e + " ";
                // }
                // Debug.Log($"栈内 顶->底: {log}");
            }
        }

        #endregion

        private static int Partition<T>(ref T[] arr, int left, int right, Comparison<T> comparison)
        {
            T pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                if (comparison(arr[j], pivot) <= 0)
                {
                    i++;
                    Swap(ref arr, i, j);
                }
            }
            
            Swap(ref arr, i + 1, right);
            // string log = "";
            // foreach (var e in arr)
            // {
            //     log += e + " ";
            // }
            // Debug.Log($"每次排序后结果: {log}");
            return i + 1;
        }
        
        private static int GetMinPivot<T>(ref T[] arr, int left, int right, Comparison<T> comparison)
        {
            T temp = arr[left];
            while (left < right)
            {
                // 在右边找比基准小的
                while (left < right && comparison(arr[right], temp) >= 0)
                    right--;
                Swap(ref arr, left, right);
                // 在左边找比基准大的
                while (left < right && comparison(arr[left], temp) <= 0)
                    left++;
                Swap(ref arr, right, left);
            }

            arr[left] = temp;
            return left;
        }

        private static void Swap<T>(ref T[] arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        #endregion
        
    }
}