using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;


namespace AssetsEditor
{
    public class Utilities
    {
        /// <summary>
        /// Closes the currently focused window when pressing ctrl+w.
        /// </summary>
        /// <param name="args"></param>
        [Shortcut("Window/Close", KeyCode.W, ShortcutModifiers.Control)]
        private static void CloseWindow(ShortcutArguments args)
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Close();
            }
        }


        [MenuItem("Utilities/Quick Run Code")]
        public static void QuickRunCode()
        {
            string levelsJsonData = File.ReadAllText(Path.Combine(Application.persistentDataPath, "levels", "test2.json"));

            foreach (string line in levelsJsonData.Split('\n'))
            {
                if (line.Contains("\"id\":\"2\""))
                {
                    Debug.Log((line.Substring(line.Length - 10)).Trim(new char[] { ' ', ',' }));
                }
            }
        }
    }
}