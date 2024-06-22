using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace unity_editor_utils.Core.HookMethods
{
    /// <summary>
    /// Contains methods which replace the original Unity Methods.
    /// </summary>
    public static class HookMethods
    {
        /// <summary>
        /// Cache for the layer scroll position.
        /// </summary>
        private static Vector2 _layerScrollCache;

        private static Vector2 _parameterScrollCache;

        /// <summary>
        /// Replace Method for LayerControllerView::ResetUi.
        /// </summary>
        /// <param name="instance">The instance of the LayerControllerView object.</param>
        public static void Hk_LayerControllerView_ResetUi(object instance)
        {
            // Retrieve the LayerControllerView type.
            var layerControllerViewType = AssemblyUtils.AssemblyUtils.GetLayerControllerViewType();

            if (layerControllerViewType == null)
            {
                Debug.LogError("Failed to retrieve the layerControllerView Type.");
                return;
            }

            // Retrieve the m_LayerScroll field.
            var mLayerScrollField = layerControllerViewType?.GetField("m_LayerScroll", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mLayerScrollField == null)
            {
                Debug.LogError("Failed to retrieve the m_LayerScroll field.");
                return;
            }

            // Retrieve the m_LayerList field.
            var mLayerListField = layerControllerViewType?.GetField("m_LayerList", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mLayerListField == null)
            {
                Debug.LogError("Failed to retrieve the m_LayerList field.");
                return;
            }

            // Cache the current scroll position.
            _layerScrollCache = (Vector2)mLayerScrollField.GetValue(instance);

            // Call the original ResetUI method.
            HookManager.HookManager.HkLayerControllerViewResetUi.CallOriginalFunction(instance, new object[] { });

            // Retrieve the ReorderableList instance from the m_LayerList field.
            if (!(mLayerListField.GetValue(instance) is ReorderableList layerList))
            {
                return;
            }

            // Adjust the scroll position.
            var scrollPosition = (Vector2)mLayerScrollField.GetValue(instance);
            scrollPosition.y = Mathf.Max(0, layerList.count * layerList.elementHeight - layerList.elementHeight);
            mLayerScrollField.SetValue(instance, scrollPosition);
        }

        /// <summary>
        /// Replace Method of GetDefaultWeight.
        /// </summary>
        /// <param name="instance">The instance of the AnimatorControllerLayer object.</param>
        /// <returns>Returns the default weight as 1.0.</returns>
        public static float HkGetDefaultWeight(object instance)
        {
            return 1.0f; 
        }

        public static void Hk_ParameterControllerView_ResetUi(object instance)
        {
            // Retrieve the ParameterControllerView type.
            var parameterControllerView = AssemblyUtils.AssemblyUtils.GetParameterControllerViewType();

            if (parameterControllerView == null)
            {
                Debug.LogError("Failed to retrieve the parameterControllerView Type.");
                return;
            }

            // Retrieve the m_LayerScroll field.
            var mScrollPosition = parameterControllerView?.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mScrollPosition == null)
            {
                Debug.LogError("Failed to retrieve the m_ScrollPosition field.");
                return;
            }

            // Retrieve the m_LayerList field.
            var mParameterList = parameterControllerView?.GetField("m_ParameterList", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mParameterList == null)
            {
                Debug.LogError("Failed to retrieve the m_ParameterList field.");
                return;
            }

            // Cache the current scroll position.
            _parameterScrollCache = (Vector2)mScrollPosition.GetValue(instance);

            // Call the original ResetUI method.
            HookManager.HookManager.HkParameterControllerViewResetUi.CallOriginalFunction(instance, new object[] { });

            // Retrieve the ReorderableList instance from the m_ParameterList field.
            if (!(mParameterList.GetValue(instance) is ReorderableList parameterList))
            {
                return;
            }

            // Adjust the scroll position.
            var scrollPosition = (Vector2)mScrollPosition.GetValue(instance);
            scrollPosition.y = Mathf.Max(0, parameterList.count * parameterList.elementHeight - parameterList.elementHeight);
            mScrollPosition.SetValue(instance, scrollPosition);
        }
    }
}
