using System;
using System.Reflection;
using UnityEngine;

namespace unity_editor_utils.Core.HookManager
{
    /// <summary>
    /// Manages hooks for Unity Editor methods.
    /// </summary>
    public static class HookManager
    {
        /// <summary>
        /// Detour object for the get_defaultWeight hook.
        /// </summary>
        internal static Detour.Detour HkGetDefaultWeight;

        /// <summary>
        /// Detour object for the ResetUI hook.
        /// </summary>
        internal static Detour.Detour HkLayerControllerViewResetUi;

        internal static Detour.Detour HkParameterControllerViewResetUi;


        /// <summary>
        /// Initializes the hooks for the Unity Methods
        /// </summary>
        public static void InitHooks()
        {
            HkGetDefaultWeight = InitHook(
                AssemblyUtils.AssemblyUtils.GetAnimatorControllerLayerType(),
                "defaultWeight",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.HkGetDefaultWeight)
            );

            if (HkGetDefaultWeight == null)
            {
                Debug.LogError("Failed to initialize HkGetDefaultWeight hook.");
            }

            HkLayerControllerViewResetUi = InitHook(
                AssemblyUtils.AssemblyUtils.GetLayerControllerViewType(),
                "ResetUI",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.Hk_LayerControllerView_ResetUi),
                isStatic: false
            );

            if (HkLayerControllerViewResetUi == null)
            {
                Debug.LogError("Failed to initialize HkLayerControllerViewResetUi hook.");
            }

            HkParameterControllerViewResetUi = InitHook(
                AssemblyUtils.AssemblyUtils.GetParameterControllerViewType(),
                "ResetUI",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.Hk_ParameterControllerView_ResetUi),
                isStatic: false
                );

            if (HkParameterControllerViewResetUi == null)
            {
                Debug.LogError("Failed to initialize HkParameterControllerViewResetUi hook.");
            }
        }

        /// <summary>
        /// Initializes a hook by setting up a detour for the specified method.
        /// </summary>
        /// <param name="targetType">The type containing the method to hook.</param>
        /// <param name="methodName">The name of the method to hook.</param>
        /// <param name="hookType">The type containing the hook method.</param>
        /// <param name="hookMethodName">The name of the hook method.</param>
        /// <param name="isStatic">Indicates whether the method to hook is static. Default is true.</param>
        /// <returns>A Detour object representing the hook, or null if the hook could not be created.</returns>
        private static Detour.Detour InitHook(Type targetType, string methodName, Type hookType, string hookMethodName, bool isStatic = true)
        {
            if (targetType == null)
            {
                Debug.LogError($"{targetType?.Name} type is null");
                return null;
            }

            var originalMethod = isStatic ? targetType.GetProperty(methodName)?.GetGetMethod(true) : targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);

            if (originalMethod == null)
            {
                Debug.LogError($"Failed to retrieve the {methodName} method.");
                return null;
            }

            var hookMethod = hookType.GetMethod(hookMethodName, BindingFlags.Static | BindingFlags.Public);

            var detour = new Detour.Detour(originalMethod, hookMethod);
            detour.ApplyHook();

            return detour;
        }
    }
}
