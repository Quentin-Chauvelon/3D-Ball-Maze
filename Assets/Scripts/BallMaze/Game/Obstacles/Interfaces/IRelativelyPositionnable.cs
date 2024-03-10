using Newtonsoft.Json;

namespace BallMaze.Obstacles
{
    /// <summary>
    /// Interface for obstacles that can only be positionned over other obstacles, relative to an obstacle's id
    /// </summary>
    public interface IRelativelyPositionnable
    {
        /// <summary>
        /// The id of the obstacle to position this obstacle over.
        /// This class should be used for all obstacles that can only be positionned over other obstacles.
        /// This is better than using an absolute position because if the obstacle's model changes, an absolute position might not be valid anymore.
        /// while using the id of the obstacle under it, we can simply change the offset to match the new model.
        /// </summary>
        [JsonProperty("oId")]
        int obstacleId { get; set; }
    }
}