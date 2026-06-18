using System;
using UnityEngine;

namespace CodeBind.Editor
{
    public sealed class CodeBindData : IComparable<CodeBindData>
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
            this.BindName = bindName;
            this.BindType = bindType;
            this.BindPrefix = bindPrefix;
            this.BindTransform = bindTransform;
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