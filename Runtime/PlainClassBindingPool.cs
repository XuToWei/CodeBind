using System;
using System.Collections.Generic;

namespace CodeBind
{
    /// <summary>
    /// Caches plain class binding instances.
    /// </summary>
    internal sealed class PlainClassBindingPool
    {
        private const int MaxPoolSize = 128;

        private readonly Dictionary<Type, Queue<IPlainClassBinding>> m_QueuesByType = new Dictionary<Type, Queue<IPlainClassBinding>>();

        internal T Acquire<T>(PlainClassBindingHost host) where T : class, IPlainClassBinding, new()
        {
            T binding;
            if (!m_QueuesByType.TryGetValue(typeof(T), out var bindings))
            {
                binding = Activator.CreateInstance<T>();
            }
            else
            {
                if (bindings.Count == 0)
                {
                    binding = Activator.CreateInstance<T>();
                }
                else
                {
                    binding = (T)bindings.Dequeue();
                }
            }
            binding.Bind(host);
            return binding;
        }

        internal void Release(IPlainClassBinding binding)
        {
            binding.Unbind();
            Type bindingType = binding.GetType();
            if (!m_QueuesByType.TryGetValue(bindingType, out var bindings))
            {
                bindings = new Queue<IPlainClassBinding>();
                m_QueuesByType.Add(bindingType, bindings);
            }

            if (bindings.Count > MaxPoolSize)
            {
                return;
            }

            bindings.Enqueue(binding);
        }
    }
}
