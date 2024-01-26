using System;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class CodeBindData
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
    }
}
