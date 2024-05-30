using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class Spikes : Obstacle, IRelativelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.Spikes;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => true;

        public int obstacleId { get; set; }


        public Spikes(int id, int obstacleId) : base(id)
        {
            this.obstacleId = obstacleId;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject spikes;

            // If the game is in play mode, laod the spikes from the asset bundle. Otherwise, outside of play mode,
            // the asset bundle is not loaded yet, so instantiate the model directly from its assets path
            if (Application.isPlaying)
            {
                spikes = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/spikes.fbx");
            }
            else
            {
#if UNITY_EDITOR
                spikes = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Spikes.fbx", typeof(GameObject)));
#else
            return null;
#endif
            }

            spikes.name = "Spikes";

            PositionObstacleOverObstacleFromId(obstacles, spikes.transform, obstacleId, Vector3.zero);

            if (Application.isPlaying)
            {
                spikes.transform.Find("Spikes_SpikesBases").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/spikes/spikesbase.mat");
                spikes.transform.Find("Spikes_Spikes").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/spikes/spikes.mat");
            }

            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.transform.localScale = new Vector3(0.75f, 0.3f, 0.75f);
            trigger.transform.position = spikes.transform.position + new Vector3(0, 0.275f, 0);
            trigger.GetComponent<MeshRenderer>().enabled = false;
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.transform.parent = spikes.transform;

            return spikes;
        }
    }
}