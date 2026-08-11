using System;
using UnityEngine;

namespace CodeBind.Editor
{
    public sealed class BindingDescriptor : IComparable<BindingDescriptor>
    {
        public string MemberNamePrefix
        {
            get;
        }

        public Type TargetType
        {
            get;
        }

        public string TargetToken
        {
            get;
        }

        public Transform SourceTransform
        {
            get;
        }

        public BindingDescriptor(string memberNamePrefix, Type targetType, string targetToken, Transform sourceTransform)
        {
            this.MemberNamePrefix = memberNamePrefix;
            this.TargetType = targetType;
            this.TargetToken = targetToken;
            this.SourceTransform = sourceTransform;
        }

        public int CompareTo(BindingDescriptor other)
        {
            int compare = String.CompareOrdinal(MemberNamePrefix, other.MemberNamePrefix);
            if (compare != 0)
            {
                return compare;
            }
            return String.CompareOrdinal(TargetToken, other.TargetToken);
        }
    }
}
