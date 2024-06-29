using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;

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

        /// <summary>
        /// Cache for the parameter scroll position.
        /// </summary>
        private static Vector2 _parameterScrollCache;

        /// <summary>
        /// Hook method for AnimatorController.AddLayer(string name).
        /// After calling the original method, this sets the weight of the newly added layer to 1.0f.
        /// </summary>
        /// <param name="instance">The instance of the AnimatorController object.</param>
        /// <param name="name">The name of the new layer.</param>
        public static void HkAddLayerString(object instance, string name)
        {
            // Call the original AddLayer method
            HookManager.HookManager.HkAnimatorControllerAddLayerString.CallOriginalFunction<object>(instance, name);

            // Retrieve the AnimatorController instance
            var controller = instance as AnimatorController;
            if (controller == null)
            {
                Debug.LogError("Failed to cast instance to AnimatorController.");
                return;
            }

            // Set the weight of the last added layer to 1.0f
            var layers = controller.layers;
            if (layers.Length > 0)
            {
                layers[layers.Length - 1].defaultWeight = 1.0f;
                controller.layers = layers; // Apply the modified layers back to the controller
            }
        }

        /// <summary>
        /// Hook method for AnimatorController.AddLayer(AnimatorControllerLayer layer).
        /// After calling the original method, this sets the weight of the newly added layer to 1.0f.
        /// </summary>
        /// <param name="instance">The instance of the AnimatorController object.</param>
        /// <param name="layer">The new layer to be added.</param>
        public static void HkAddLayerAnimatorControllerLayer(object instance, AnimatorControllerLayer layer)
        {
            // Call the original AddLayer method
            HookManager.HookManager.HkAnimatorControllerAddLayerControllerLayer.CallOriginalFunction<object>(instance, layer);
            // Set the weight of the added layer to 1.0f
            layer.defaultWeight = 1.0f;
        }

        /// <summary>
        /// Replace Method for LayerControllerView::ResetUi.
        /// </summary>
        /// <param name="instance">The instance of the LayerControllerView object.</param>
        public static void HkLayerControllerViewResetUi(object instance)
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
            HookManager.HookManager.HkLayerControllerViewResetUi.CallOriginalFunction<object>(instance, new object[] { });

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
        /// Replace Method for ParameterControllerView::ResetUi.
        /// </summary>
        /// <param name="instance">The instance of the LayerControllerView object.</param>
        public static void HkParameterControllerViewResetUi(object instance)
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
            HookManager.HookManager.HkParameterControllerViewResetUi.CallOriginalFunction<object>(instance, new object[] { });

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
