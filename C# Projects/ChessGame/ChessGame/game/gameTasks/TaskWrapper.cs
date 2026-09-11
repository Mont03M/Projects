namespace ChessGame.Game.GameTasks
{
    /// <summary>
    /// TaskWrapper class provides utility methods to wrap synchronous functions and actions into asynchronous tasks, allowing them to be executed on a thread pool thread. 
    /// This is useful for running CPU-bound work in an asynchronous context.
    /// </summary>
    public static class TaskWrapper
    {
        /// <summary>
        /// Wraps a synchronous function into an asynchronous task, allowing it to be executed on a thread pool thread. 
        /// This is useful for running CPU-bound work in an asynchronous context.
        /// </summary>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <param name="func">The synchronous function to wrap.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task<T> TaskGenericWrapper<T>(Func<T> func)
        {
            // Run the CPU-bound work on a thread-pool thread so it can be canceled via the token.
            return await Task.Run(func);
        }

        /// <summary>
        /// Wraps a synchronous action into an asynchronous task, allowing it to be executed on a thread pool thread. 
        /// This is useful for running CPU-bound work in an asynchronous context.
        /// </summary>
        /// <param name="action">The synchronous action to wrap.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task TaskVoidWrapper(Action action)
        {

            await Task.Run(action);
            
        }

    }
}
