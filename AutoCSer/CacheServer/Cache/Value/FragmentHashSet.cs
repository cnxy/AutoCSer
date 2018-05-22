﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoCSer.CacheServer.Cache.Value
{
    /// <summary>
    /// 256 基分片 哈希表节点
    /// </summary>
    /// <typeparam name="valueType">数据类型</typeparam>
    internal sealed class FragmentHashSet<valueType> : Node
        where valueType : IEquatable<valueType>
    {
        /// <summary>
        /// 哈希表
        /// </summary>
        private readonly System.Collections.Generic.HashSet<HashCodeKey<valueType>>[] hashSets = new System.Collections.Generic.HashSet<HashCodeKey<valueType>>[256];
        /// <summary>
        /// 有效数据数量
        /// </summary>
        private int count;
        /// <summary>
        /// 获取下一个节点
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        internal override Cache.Node GetOperationNext(ref OperationParameter.NodeParser parser)
        {
            parser.ReturnParameter.Type = ReturnType.OperationTypeError;
            return null;
        }
        /// <summary>
        /// 操作数据
        /// </summary>
        /// <param name="parser">参数解析</param>
        internal override void OperationEnd(ref OperationParameter.NodeParser parser)
        {
            switch (parser.OperationType)
            {
                case OperationParameter.OperationType.Remove: remove(ref parser); return;
                case OperationParameter.OperationType.SetValue: setValue(ref parser); return;
                case OperationParameter.OperationType.Clear:
                    if (count != 0)
                    {
                        Array.Clear(hashSets, 0, 256);
                        count = 0;
                        parser.IsOperation = true;
                    }
                    parser.ReturnParameter.Set(true);
                    return;
            }
            parser.ReturnParameter.Type = ReturnType.OperationTypeError;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="parser"></param>
        private void remove(ref OperationParameter.NodeParser parser)
        {
            HashCodeKey<valueType> key;
            if (HashCodeKey<valueType>.Get(ref parser, out key))
            {
                System.Collections.Generic.HashSet<HashCodeKey<valueType>> hashSet = hashSets[key.HashCode & 0xff];
                if (hashSet != null && hashSet.Remove(key))
                {
                    --count;
                    parser.IsOperation = true;
                    parser.ReturnParameter.Set(true);
                }
                else parser.ReturnParameter.Set(false);
            }
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="parser"></param>
        private void setValue(ref OperationParameter.NodeParser parser)
        {
            HashCodeKey<valueType> key;
            if (HashCodeKey<valueType>.Get(ref parser, out key))
            {
                System.Collections.Generic.HashSet<HashCodeKey<valueType>> hashSet = hashSets[key.HashCode & 0xff];
                if (hashSet == null) hashSets[key.HashCode & 0xff] = hashSet = AutoCSer.HashSetCreator<HashCodeKey<valueType>>.Create();
                int count = hashSet.Count;
                hashSet.Add(key);
                if (hashSet.Count != count)
                {
                    ++this.count;
                    parser.IsOperation = true;
                    parser.ReturnParameter.Set(true);
                }
                else parser.ReturnParameter.Set(true);
            }
        }
        /// <summary>
        /// 获取下一个节点
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        internal override Cache.Node GetQueryNext(ref OperationParameter.NodeParser parser)
        {
            parser.ReturnParameter.Type = ReturnType.OperationTypeError;
            return null;
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="parser">参数解析</param>
        internal override void QueryEnd(ref OperationParameter.NodeParser parser)
        {
            HashCodeKey<valueType> key;
            switch (parser.OperationType)
            {
                case OperationParameter.OperationType.GetCount: parser.ReturnParameter.Set(count); return;
                case OperationParameter.OperationType.ContainsKey:
                    if (HashCodeKey<valueType>.Get(ref parser, out key))
                    {
                        System.Collections.Generic.HashSet<HashCodeKey<valueType>> hashSet = hashSets[key.HashCode & 0xff];
                        parser.ReturnParameter.Set(hashSet != null && hashSet.Contains(key));
                    }
                    return;
            }
            parser.ReturnParameter.Type = ReturnType.OperationTypeError;
        }

        /// <summary>
        /// 创建缓存快照
        /// </summary>
        /// <returns></returns>
        internal override Snapshot.Node CreateSnapshot()
        {
            valueType[] array = new valueType[count];
            int index = 0;
            foreach (System.Collections.Generic.HashSet<HashCodeKey<valueType>> hashSet in hashSets)
            {
                if (hashSet != null)
                {
                    foreach (HashCodeKey<valueType> node in hashSet) array[index++] = node.Value;
                }
            }
            return new Snapshot.Value.HashSet<valueType>(array);
        }
#if NOJIT
        /// <summary>
        /// 创建哈希表 数据节点
        /// </summary>
        /// <returns></returns>
        [AutoCSer.IOS.Preserve(Conditional = true)]
        [MethodImpl(AutoCSer.MethodImpl.AggressiveInlining)]
        private static FragmentHashSet<valueType> create()
        {
            return new FragmentHashSet<valueType>();
        }
#endif
        /// <summary>
        /// 构造函数
        /// </summary>
        [AutoCSer.IOS.Preserve(Conditional = true)]
        private static readonly Func<FragmentHashSet<valueType>> constructor;
        static FragmentHashSet()
        {
#if NOJIT
            constructor = (Func<FragmentHashSet<valueType>>)Delegate.CreateDelegate(typeof(Func<FragmentHashSet<valueType>>), typeof(FragmentHashSet<valueType>).GetMethod(CreateMethodName, BindingFlags.Static | BindingFlags.NonPublic, null, NullValue<Type>.Array, null));
#else
            constructor = (Func<FragmentHashSet<valueType>>)AutoCSer.Emit.Constructor.Create(typeof(FragmentHashSet<valueType>));
#endif
        }
    }
}