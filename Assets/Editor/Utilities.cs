using BallMaze;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityExtensionMethods;
using Debug = UnityEngine.Debug;


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
        

        /// <summary>
        /// Sorts all children in the selected game object by their name
        /// </summary>
        [MenuItem("Utilities/Sort Children By Name", false, 15)]
        public static void SortChildrenByName()
        {
            foreach (Transform t in Selection.transforms)
            {
                List<Transform> children = t.Cast<Transform>().ToList();
                children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
                for (int i = 0; i < children.Count; ++i)
                {
                    Undo.SetTransformParent(children[i], children[i].parent, "Sort Children");
                    children[i].SetSiblingIndex(i);
                }
            }

        }


        /// <summary>
        /// This method is only used to run code in the editor without having to play the game and its content doesn't matter
        /// </summary>
        [MenuItem("Utilities/Quick Run Code", false, 10)]
        public static void QuickRunCode()
        {
            var renderers = GameObject.Find("Maze").GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; ++i)
                bounds.Encapsulate(renderers[i].bounds);

            float radius = Mathf.Sqrt(Mathf.Pow(bounds.extents.x, 2) + Mathf.Pow(bounds.extents.z, 2));

            GameObject camera = GameObject.Find("Main Camera");
            Camera cameraComponent = camera.GetComponent<Camera>();
            camera.transform.position = bounds.center + new Vector3(0, 10, 0);
            float minDistance = radius / Mathf.Tan(Mathf.Deg2Rad * cameraComponent.fieldOfView / 2);
            Vector3 normVectorBoundsCenter2CurrentCamPos = (camera.transform.position - bounds.center) / Vector3.Magnitude(camera.transform.position - bounds.center);

            camera.transform.position = new Vector3(bounds.center.x, (minDistance * normVectorBoundsCenter2CurrentCamPos).y - 1f, bounds.center.z);
            camera.transform.RotateAround(bounds.center, new Vector3(1, 0, 0), 20);

            camera.transform.LookAt(bounds.center, new Vector3(0, -1, -1));
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z + 0.4f);
            cameraComponent.nearClipPlane = minDistance - radius;
        }
    }
}