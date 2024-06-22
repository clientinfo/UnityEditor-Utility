using unity_editor_utils.Core.HookManager;
using UnityEditor;
using UnityEngine;

namespace unity_editor_utils
{
    [InitializeOnLoad]
    public static class Main
    {
        /// <summary>
        /// Static constructor that initializes hooks by calling HookManager.InitHooks.
        /// </summary>
        static Main()
        {
            HookManager.InitHooks();
            Debug.Log("Unity Hooks Applied :) ~clientinfo");
        }
    }
}