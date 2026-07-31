using UnityEngine;

namespace CodeBind
{
    public static class PlainClassBindingExtensions
    {
        /// <summary>
        /// Gets the cached plain class binding instance.
        /// </summary>
        public static T GetPlainClassBinding<T>(this Transform transform) where T : class, IPlainClassBinding, new()
        {
            PlainClassBindingHost host = transform.GetComponent<PlainClassBindingHost>();
            if (host == null)
            {
                return null;
            }
            return host.GetPlainClassBinding<T>();
        }

        /// <summary>
        /// Gets the cached plain class binding instance.
        /// </summary>
        public static T GetPlainClassBinding<T>(this GameObject gameObject) where T : class, IPlainClassBinding, new()
        {
            PlainClassBindingHost host = gameObject.GetComponent<PlainClassBindingHost>();
            if (host == null)
            {
                return null;
            }
            return host.GetPlainClassBinding<T>();
        }

        /// <summary>
        /// Initializes a plain class binding without using the shared pool.
        /// </summary>
        public static void Initialize(this IPlainClassBinding binding, GameObject gameObject)
        {
            binding.Initialize(gameObject.GetComponent<PlainClassBindingHost>());
        }

        /// <summary>
        /// Initializes a plain class binding without using the shared pool.
        /// </summary>
        public static void Initialize(this IPlainClassBinding binding, Transform transform)
        {
            binding.Initialize(transform.GetComponent<PlainClassBindingHost>());
        }
    }
}
