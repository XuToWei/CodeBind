using System;
using UnityEngine;

namespace CodeBind.Editor
{
    public sealed class BindingDescriptor : IComparable<BindingDescriptor>
    {
        public string VariableName
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

        public BindingDescriptor(string variableName, Type targetType, string targetToken, Transform sourceTransform)
        {
            this.VariableName = variableName;
            this.TargetType = targetType;
            this.TargetToken = targetToken;
            this.SourceTransform = sourceTransform;
        }

        public int CompareTo(BindingDescriptor other)
        {
            int compare = String.CompareOrdinal(VariableName, other.VariableName);
            if (compare != 0)
            {
                return compare;
            }
            return String.CompareOrdinal(TargetToken, other.TargetToken);
        }
    }
}
