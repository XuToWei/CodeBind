using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Default binding target token configuration.
    /// </summary>
    internal sealed class DefaultBindingTargetTokenConfig : IBindingTargetTokenConfig
    {
        public int Priority => 0;

        public IReadOnlyDictionary<string, Type> TargetTypesByToken => s_TargetTypesByToken;

        private static readonly Dictionary<string, Type> s_TargetTypesByToken = new Dictionary<string, Type>()
        {
            { "Animation", typeof (UnityEngine.Animation) },
            { "Animator", typeof (UnityEngine.Animator) },
            { "Button", typeof (UnityEngine.UI.Button) },
            { "Canvas", typeof (UnityEngine.Canvas) },
            { "CanvasGroup", typeof (UnityEngine.CanvasGroup) },
            { "Dropdown", typeof (UnityEngine.UI.Dropdown) },
            { "GameObject", typeof (UnityEngine.GameObject) },
            { "GridLayoutGroup", typeof (UnityEngine.UI.GridLayoutGroup) },
            { "HorizontalLayoutGroup", typeof (UnityEngine.UI.HorizontalLayoutGroup) },
            { "Image", typeof (UnityEngine.UI.Image) },
            { "InputField", typeof (UnityEngine.UI.InputField) },
            { "Mask", typeof (UnityEngine.UI.Mask) },
            { "RawImage", typeof (UnityEngine.UI.RawImage) },
            { "RectMask2D", typeof (UnityEngine.UI.RectMask2D) },
            { "RectTransform", typeof (UnityEngine.RectTransform) },
            { "Scrollbar", typeof (UnityEngine.UI.Scrollbar) },
            { "ScrollRect", typeof (UnityEngine.UI.ScrollRect) },
            { "Slider", typeof (UnityEngine.UI.Slider) },
            { "SpriteRenderer", typeof (UnityEngine.SpriteRenderer) },
            { "Text", typeof (UnityEngine.UI.Text) },
            { "Transform", typeof (UnityEngine.Transform) },
            { "Toggle", typeof (UnityEngine.UI.Toggle) },
            { "ToggleGroup", typeof (UnityEngine.UI.ToggleGroup) },
            { "VerticalLayoutGroup", typeof (UnityEngine.UI.VerticalLayoutGroup) },
        };
    }
}
