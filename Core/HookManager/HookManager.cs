using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;
using UnityEngine;

namespace unity_editor_utils.Core.HookManager
{
    /// <summary>
    /// Manages hooks for Unity Editor methods.
    /// </summary>
    public static class HookManager
    {
        /// <summary>
        /// Detour object for the AnimatorController::AddLayer (string) hook.
        /// </summary>
        internal static Detour.Detour HkAnimatorControllerAddLayerString;

        /// <summary>
        /// Detour object for the AnimatorController::AddLayer (ControllerLayer) hook.
        /// </summary>
        internal static Detour.Detour HkAnimatorControllerAddLayerControllerLayer;

        /// <summary>
        /// Detour object for the LayerControllerView::ResetUi hook.
        /// </summary>
        internal static Detour.Detour HkLayerControllerViewResetUi;

        /// <summary>
        /// Detour object for the ParameterControllerView::ResetUi hook.
        /// </summary>
        internal static Detour.Detour HkParameterControllerViewResetUi;

        /// <summary>
        /// Initializes the hooks for the Unity Methods
        /// </summary>
        public static void InitHooks()
        {
           
            HkAnimatorControllerAddLayerString = InitHook(
                AssemblyUtils.AssemblyUtils.GetAnimatorControllerType(),
                "AddLayer",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.HkAddLayerString),
                isStatic: false,
                parameterTypes: new Type[] { typeof(string) }
            );

            if (HkAnimatorControllerAddLayerString == null)
            {
                Debug.LogError("Failed to initialize HkAnimatorControllerAddLayerString hook.");
            }

            HkAnimatorControllerAddLayerControllerLayer = InitHook(
                AssemblyUtils.AssemblyUtils.GetAnimatorControllerType(),
                "AddLayer",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.HkAddLayerAnimatorControllerLayer),
                isStatic: false,
                parameterTypes: new Type[] { typeof(AnimatorControllerLayer) }
            );

            if (HkAnimatorControllerAddLayerControllerLayer == null)
            {
                Debug.LogError("Failed to initialize HkAnimatorControllerAddLayerControllerLayer hook.");
            }

            HkLayerControllerViewResetUi = InitHook(
                AssemblyUtils.AssemblyUtils.GetLayerControllerViewType(),
                "ResetUI",
                typeof(HookMethods.HookMethods),
                nameof(HookMethods.HookMethods.HkLayerControllerViewResetUi),
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
                nameof(HookMethods.HookMethods.HkParameterControllerViewResetUi),
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
        /// <param name="parameterTypes">The types of the parameters for the method to hook. Default is null.</param>
        /// <returns>A Detour object representing the hook, or null if the hook could not be created.</returns>
        private static Detour.Detour InitHook(Type targetType, string methodName, Type hookType, string hookMethodName, bool isStatic = true, Type[] parameterTypes = null)
        {
            if (targetType == null)
            {
                Debug.LogError($"{targetType?.Name} type is null");
                return null;
            }

            var originalMethod = isStatic
                ? targetType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, parameterTypes, null)
                : targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, parameterTypes, null);

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
