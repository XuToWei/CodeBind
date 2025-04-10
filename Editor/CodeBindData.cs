using System;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class CodeBindData : IComparable<CodeBindData>
    {
        public string BindName
        {
            get;
        }

        public Type BindType
        {
            get;
        }

        public string BindPrefix
        {
            get;
        }

        public Transform BindTransform
        {
            get;
        }

        public CodeBindData(string bindName, Type bindType, string bindPrefix, Transform bindTransform)
        {
            BindName = bindName;
            BindType = bindType;
            BindPrefix = bindPrefix;
            BindTransform = bindTransform;
        }

        public int CompareTo(CodeBindData other)
        {
            int compare = String.CompareOrdinal(BindName, other.BindName);
            if (compare != 0)
            {
                return compare;
            }
            return String.CompareOrdinal(BindPrefix, other.BindPrefix);
        }
    }
}