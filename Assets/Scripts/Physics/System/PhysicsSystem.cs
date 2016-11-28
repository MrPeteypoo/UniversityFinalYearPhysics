namespace PSI
{
    /// <summary>
    /// An abstraction to the system management functionality of a Physics engine.
    /// </summary>
    public abstract class PhysicsSystem : Physics
    {

        #region General functionality

        /// <summary>
        /// Sets the current system as the default Physics system.
        /// </summary>
        public void SetAsDefault()
        {
            defaultPhysicsSystem = this;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Initialises the system, preparing it for use.
        /// </summary>
        /// <returns>Whether the initialisation was successful.</returns> 
        public abstract bool Initialise();

        /// <summary>
        /// Called at the start of the game loop when an update is about to be requested.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public abstract void PreUpdate (float deltaTime);

        /// <summary>
        /// Called after PreUpdate(), this is typically where the main simulation happens.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public abstract void MainUpdate (float deltaTime);

        /// <summary>
        /// Called after the initial Update() has been performed.
        /// </summary>
        /// <param name="deltaTime">How many second have passed since the last frame.</param>
        public abstract void PostUpdate (float deltaTime);

        #endregion
    }
}