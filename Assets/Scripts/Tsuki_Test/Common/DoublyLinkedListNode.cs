// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/24 13:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Tsuki_Test.Common
{
    /// <summary>
    /// 双向链表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DoublyLinkedListNode<T>
    {
        [CanBeNull] public DoublyLinkedListNode<T> Prev { get; private set; }
        [CanBeNull] public DoublyLinkedListNode<T> Next { get; private set; }
        public T Data { get; private set; }

        // 构造函数
        public DoublyLinkedListNode(T data)
        {
            Prev = null;
            Next = null;
            Data = data;
        }

        /// <summary>
        /// 在给定的头部添加节点
        /// </summary>
        /// <param name="newNode"></param>
        public void InsertHead(DoublyLinkedListNode<T> newNode)
        {
            // 如果链表为空，直接添加
            if (Prev == null)
            {
                Prev = newNode;
                newNode.Next = this;
            }
            else
            {
                newNode.Next = this;
                newNode.Prev = Prev;
                Prev.Next = newNode;
                Prev = newNode;
            }
        }

        /// <summary>
        /// 在给定的节点尾部添加节点
        /// </summary>
        /// <param name="newNode"></param>
        public void InsertTail(DoublyLinkedListNode<T> newNode)
        {
            // 如果链表为空，直接添加
            if (Next == null)
            {
                Next = newNode;
                newNode.Prev = this;
            }
            else
            {
                newNode.Next = Next;
                newNode.Prev = this;
                Next.Prev = newNode;
                Next = newNode;
            }
        }

        /// <summary>
        /// 删除节点，不影响链表结构
        /// </summary>
        public void RemoveNode()
        {
            if (Prev != null)
            {
                Prev.Next = Next;
            }
            if (Next != null) Next.Prev = Prev;
        }

        /// <summary>
        /// 删除节点并分离产生子链表
        /// </summary>
        public void RemoveNodeAndSeparate()
        {
            if (Prev != null) Prev.Next = null;
            if (Next != null) Next.Prev = null;
            Prev = null;
            Next = null;
        }

        /// <summary>
        /// 正向遍历当前节点后的迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DoublyLinkedListNode<T>> BianLiForward()
        {
            var current = Next;

            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        /// <summary>
        /// 反向遍历当前节点前的迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DoublyLinkedListNode<T>> BianLiBackward()
        {
            var current = Prev;

            while (current != null)
            {
                yield return current;
                current = current.Prev;
            }
        }
    }
}
