using UnityEditor;
using UnityEngine;
using BallMaze.Newtonsoft.Helpers;


namespace BallMaze.Editor.Newtonsoft
{
    /// <summary>
    /// Class to configure the JSON settings for the application in the editor.
    /// see: https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/#post-9490090
    /// </summary>
    public static class EditorJsonSettings
    {
        [InitializeOnLoadMethod]
        public static void ApplyCustomConverters()
        {
            CustomJsonSettings.ConfigureJsonInternal();
        }
    }

    /// <summary>
    /// Class to configure the JSON settings for the application at runtime.
    /// see: https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/#post-9490090
    /// </summary>
    public static class RuntimeJsonSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplyCustomConverters()
        {
            CustomJsonSettings.ConfigureJsonInternal();
        }
    }
}