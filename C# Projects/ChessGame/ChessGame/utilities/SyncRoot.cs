namespace ChessGame.Utilities
{
    /// <summary>
    /// Represents a synchronization root that provides a shared lock for synchronizing access to move and board collections in a chess game.
    /// </summary>
    public static class SyncRoot
    {
        /// <summary>
        /// Shared lock used to synchronize access to move/board collections.
        /// </summary>
        public static readonly Lock MovesLock = new();
    }
}
