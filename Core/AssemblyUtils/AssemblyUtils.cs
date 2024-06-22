using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace unity_editor_utils.Core.AssemblyUtils
{
    /// <summary>
    /// Provides utility methods for interacting with Unity assemblies.
    /// </summary>
    public static class AssemblyUtils
    {
        /// <summary>
        /// Retrieves an assembly by its name from the current application domain.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to retrieve.</param>
        /// <returns>The assembly with the specified name, or null if not found.</returns>
        public static Assembly GetAssemblyByName(string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);
        }

        /// <summary>
        /// Retrieves the type for LayerControllerView from the UnityEditor.Graphs assembly.
        /// </summary>
        /// <returns>
        /// The LayerControllerView type if found; otherwise, null. 
        /// Logs an error if the UnityEditor.Graphs assembly or LayerControllerView type could not be found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the UnityEditor.Graphs assembly could not be found.
        /// </exception>
        public static Type GetLayerControllerViewType()
        {
            var unityEditorGraphAssembly = GetAssemblyByName("UnityEditor.Graphs");

            if (unityEditorGraphAssembly == null)
            {
                Debug.LogError("UnityEditor.Graphs assembly could not be found.");
                return null;
            }

            var layerControllerView = unityEditorGraphAssembly.GetType("UnityEditor.Graphs.LayerControllerView");

            if (layerControllerView == null)
            {
                Debug.LogError("LayerControllerView type could not be found in UnityEditor.Graphs assembly.");
                return null;
            }

            return layerControllerView;
        }

        /// <summary>
        /// Retrieves the type for ParameterControllerView from the UnityEditor.Graphs assembly.
        /// </summary>
        /// <returns>
        /// The ParameterControllerView type if found; otherwise, null. 
        /// Logs an error if the UnityEditor.Graphs assembly or ParameterControllerView type could not be found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the UnityEditor.Graphs assembly could not be found.
        /// </exception>
        public static Type GetParameterControllerViewType()
        {
            var unityEditorGraphAssembly = GetAssemblyByName("UnityEditor.Graphs");

            if (unityEditorGraphAssembly == null)
            {
                Debug.LogError("UnityEditor.Graphs assembly could not be found.");
                return null;
            }

            var parameterControllerView = unityEditorGraphAssembly.GetType("UnityEditor.Graphs.ParameterControllerView");

            if (parameterControllerView == null)
            {
                Debug.LogError("parameterControllerView type could not be found in UnityEditor.Graphs assembly.");
                return null;
            }

            return parameterControllerView;
        }

        /// <summary>
        /// Retrieves the type for AnimatorControllerLayer from the UnityEditor assembly.
        /// </summary>
        /// <returns>
        /// The AnimatorControllerLayer type if found; otherwise, null. 
        /// Logs an error if the UnityEditor assembly or AnimatorControllerLayer type could not be found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the UnityEditor assembly could not be found.
        /// </exception>
        public static Type GetAnimatorControllerLayerType()
        {
            var unityEditorAssembly = GetAssemblyByName("UnityEditor");

            if (unityEditorAssembly == null)
            {
                Debug.LogError("UnityEditor assembly could not be found.");
                return null;
            }

            var animatorControllerLayer = unityEditorAssembly.GetType("UnityEditor.Animations.AnimatorControllerLayer");

            if (animatorControllerLayer == null)
            {
                Debug.LogError("AnimatorControllerLayer type could not be found in UnityEditor assembly.");
                return null;
            }

            return animatorControllerLayer;
        }

        /// <summary>
        /// Retrieves the type for AnimatorController from the UnityEditor assembly.
        /// </summary>
        /// <returns>
        /// The AnimatorController type if found; otherwise, null. 
        /// Logs an error if the UnityEditor assembly or AnimatorController type could not be found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the UnityEditor assembly could not be found.
        /// </exception>
        public static Type GetAnimatorControllerType()
        {
            var unityEditorAssembly = GetAssemblyByName("UnityEditor");

            if (unityEditorAssembly == null)
            {
                Debug.LogError("UnityEditor assembly could not be found.");
                return null;
            }

            var animatorControllerType = unityEditorAssembly.GetType("UnityEditor.Animations.AnimatorController");

            if (animatorControllerType == null)
            {
                Debug.LogError("AnimatorController type could not be found in UnityEditor assembly.");
                return null;
            }

            return animatorControllerType;
        }
    }
}
