using ChessGame.Enums;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ChessGame.Strategies.Tools.StockfishTools
{
    /// <summary>
    /// Represents a wrapper for interacting with the Stockfish chess engine. 
    /// This class allows you to evaluate chess positions, get the best move, and analyze positions using the Stockfish engine.
    /// </summary>
    public sealed class StockFishEvaluator : IDisposable
    {
        public record StockfishSearchResult(string BestMove, double Evaluation);

        #region -- PROPERTIES --
        private readonly Process stockfishProcess;
        private readonly StreamWriter stockfishInput;
        private readonly StreamReader stockfishOutput;
        private bool disposed;
        // Set while Dispose has started; prevents new operations from entering while we wait for active operations to finish
        private bool isDisposing;
        // Signaled when Dispose has completed (either normally or forced). Waiting callers block on this while disposal is in progress.
        private readonly System.Threading.ManualResetEventSlim disposalCompleted = new(false);
        private readonly object syncRoot = new();
        private int activeOperations = 0;
        private readonly System.Threading.ManualResetEventSlim noActiveOperations = new(true);
        private readonly TimeSpan responseTimeout;

        public bool IsDisposed
        {
            get => disposed;
        }

        #endregion

        #region -- CONSTRUCTOR --

        /// <summary>
        /// Initializes a new instance of the StockFishEvaluator class, starting the Stockfish process and preparing it for use.
        /// </summary>
        /// <param name="stockfishPath">The path to the Stockfish executable.</param>
        /// <param name="responseTimeoutMs">The response timeout in milliseconds.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown when the project directory cannot be found.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the Stockfish executable cannot be found.</exception>
        public StockFishEvaluator(string stockfishPath = @"Strategies\Stockfish\src\stockfish.exe", int responseTimeoutMs = 15000) 
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null &&
                  directory.GetFiles("*.csproj").Length == 0)
            {
                directory = directory.Parent;
            }

        
            if(directory is null)
            {
                throw new DirectoryNotFoundException("Could not find the project directory.");
            }

            stockfishPath = System.IO.Path.Combine(directory.FullName,"Strategies","Stockfish","src", "stockfish.exe");

            
            if (!File.Exists(stockfishPath))
            {
                throw new FileNotFoundException("Stockfish executable was not found"
                    ,stockfishPath);
            }

            // Start the Stockfish process
            ProcessStartInfo processInfo = new()
            {
                FileName = stockfishPath,
                WorkingDirectory = System.IO.Path.GetDirectoryName(stockfishPath),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Redirect the error output to the console for debugging purposes
            stockfishProcess = new Process
            {
                StartInfo = processInfo
            };

            // Redirect the error output to the console for debugging purposes
            stockfishProcess.Start();

            // Redirect the error output to the console for debugging purposes
            stockfishInput = stockfishProcess.StandardInput;
            stockfishOutput = stockfishProcess.StandardOutput;

            responseTimeout = TimeSpan.FromMilliseconds(responseTimeoutMs);

            InitializeStockfish();

        }

        #endregion

        #region -- INITIALIZE STOCKFISH --

        /// <summary>
        /// Initializes the Stockfish engine by sending the "uci" command and waiting for the "uciok" response, 
        /// followed by the "isready" command and waiting for the "readyok" response.
        /// </summary>
        public void InitializeStockfish()
        {
            // Called from constructor; no need for operation counting here
            SendCommand("uci");

            WaitFor("uciok");

            SendCommand("isready");

            WaitFor("readyok");
        }

        #endregion

        #region -- ENTER AND EXIT OPERATION, READOUT-TIMEOUT, STOP SEARCH --

        /// <summary>
        /// Enters an operation, incrementing the active operation count. If disposal is in progress, this method will wait until disposal is complete before proceeding.
        /// This ensures that no new operations can start while the object is being disposed.
        /// </summary>
        private void EnterOperation()
        {
            while (true)
            {
                try
                {
                    // Check if the object is disposed or disposal is in progress. If so, throw an exception to prevent new operations from starting.
                    lock (syncRoot)
                    {
                        if (disposed) throw new ObjectDisposedException(nameof(StockFishEvaluator));
                        if (!isDisposing)
                        {
                            // Increment the active operation count and reset the noActiveOperations event if this is the first active operation.
                            activeOperations++;
                            if (activeOperations == 1) noActiveOperations.Reset();
                            return;
                        }
                        // if disposal is in progress, release the lock and wait for it to complete
                    }

                    // Wait until Dispose completes. When signaled, loop and re-check disposed/isDisposing.
                    disposalCompleted.Wait();

                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine($"ObjectDisposedException: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Reads a line from the Stockfish output stream with a timeout. If the read operation does not complete within the specified timeout, a TimeoutException is thrown.
        /// </summary>
        /// <returns>The line read from the Stockfish output stream.</returns>
        /// <exception cref="TimeoutException">Thrown if the read operation does not complete within the specified timeout.</exception>
        private string? ReadLineWithTimeout()
        {
            try
            {
                // Use ReadLineAsync and Wait with a timeout to avoid blocking indefinitely
                var readTask = stockfishOutput.ReadLineAsync();

                // Wait for the read operation to complete within the specified timeout
                if (readTask.Wait(responseTimeout))
                {
                    return readTask.Result;
                }
                else
                {
                    throw new TimeoutException($"Timed out waiting for Stockfish response after {responseTimeout.TotalMilliseconds}ms.");
                }
            }
            catch (AggregateException ae) when (ae.InnerException is TimeoutException te)
            {
                throw te;
            }
        }

        /// <summary>
        /// Exits an operation, decrementing the active operation count. If there are no more active operations, 
        /// the noActiveOperations event is set to signal that all operations have completed.
        /// </summary>
        private void ExitOperation()
        {
            lock (syncRoot)
            {
                activeOperations--;
                if (activeOperations == 0) noActiveOperations.Set();
            }
        }

        /// <summary>
        /// Stops any ongoing search operation in the Stockfish engine. 
        /// This method sends a "stop" command to the Stockfish process, which will halt any current search and return the best move found so far.
        /// </summary>
        public void StopSearch()
        {
            // Short operation: acquire lock to check disposed and write
            lock (syncRoot)
            {
                try
                {
                    if (disposed) return;
                    stockfishInput.WriteLine("stop");
                    stockfishInput.Flush();
                }
                catch
                {
                    // ignore any errors when trying to stop
                }
            }
        }

        #endregion

        #region -- GET BEST MOVE --

        /// <summary>
        /// Gets the best move for a given chess position in FEN format, searching to the specified depth.
        /// </summary>
        /// <param name="fen">The FEN string representing the chess position.</param>
        /// <param name="depth">The search depth for the engine.</param>
        /// <returns>The best move in UCI format, or null if no move is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the FEN string is null or empty.</exception>
        public string? GetBestMove(string fen, int depth = 12)
        {
            // Validate input and ensure the object is not disposed
            EnterOperation();
            try
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                if (string.IsNullOrWhiteSpace(fen))
                    throw new ArgumentNullException(nameof(fen), "FEN cannot be null or empty.");

                SendCommand($"position fen {fen}");

                SendCommand($"go depth {depth}");

                return ReadBestMove();
            }
            finally
            {
                ExitOperation();
            }
        }

        /// <summary>
        /// Reads the best move from the Stockfish output stream. This method continuously reads lines from the output until it finds a line that starts with "bestmove ".
        /// </summary>
        /// <returns>The best move as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Stockfish process closes the output stream before returning a best move.</exception>
        private string ReadBestMove()
        {
            while (true)
            {
                string? line = ReadLineWithTimeout() ?? throw new InvalidOperationException(
                        "Stockfish closed the output stream before returning a best move.");

                if (line.StartsWith("bestmove "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length < 2)
                        throw new InvalidOperationException(
                            $"Invalid Stockfish bestmove response: {line}");

                    return parts[1];
                }
            }
        }

        #endregion

        #region -- DETERMINE GAME STATE --

        /// <summary>
        /// Determines the game state (Continuing, Check, Checkmate, Stalemate) for a given chess position in FEN format and whether the side to move is in check.
        /// </summary>
        /// <param name="fen">The FEN string representing the chess position.</param>
        /// <param name="IsSideToMoveInCheck">Indicates whether the side to move is in check.</param>
        /// <returns>The game state.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Stockfish process closes the output stream before returning a result.</exception>
        public GameState DetermineGameState(string fen, bool IsSideToMoveInCheck)
        {
            EnterOperation();
            try
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                ValidateFen(fen);

                SendCommand($"position fen {fen}");

                SendCommand($"go depth 1");

                while (true)
                {
                    string? line = stockfishOutput.ReadLine() ??
                        throw new InvalidOperationException("Stockfish closed the output stream before returning a result.");

                    Debug.WriteLine($"[Stockfish] {line}");

                    if (!line.StartsWith("bestmove "))
                        continue;

                    // If we reach here, we have a bestmove line. Parse it to determine the game state.
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    // If the best move is "(none)", it indicates that there are no legal moves available, which means the game is either in checkmate or stalemate.
                    if (parts.Length < 2)
                    {
                        throw new InvalidOperationException($"Invalid Stockfish bestmove reponse: {line}");
                    }

                    // If the best move is "(none)", it indicates that there are no legal moves available, which means the game is either in checkmate or stalemate.
                    string bestMove = parts[1];

                    // If the best move is "(none)", it indicates that there are no legal moves available, which means the game is either in checkmate or stalemate.
                    if (bestMove == "(none)")
                    {
                        return IsSideToMoveInCheck ? GameState.Checkmate : GameState.Stalemate;
                    }

                    // If the best move is not "(none)", it indicates that there are legal moves available, which means the game is continuing.
                    // However, we still need to check if the side to move is in check.
                    return IsSideToMoveInCheck ? GameState.Check : GameState.Continuing;
                }
            }
            finally
            {
                ExitOperation();
            }
        }

        #endregion

        #region -- ANALYZE POSITION --

        /// <summary>
        /// Analyzes a chess position given in FEN format to a specified depth and returns the best move along with its evaluation score.
        /// </summary>
        /// <param name="fen">The FEN string representing the chess position.</param>
        /// <param name="depth">The search depth for the engine.</param>
        /// <returns>The best move along with its evaluation score.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the FEN string is null or empty.</exception>
        public StockfishSearchResult Analyze(string fen, int depth)
        {
            EnterOperation();
            try
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                if (string.IsNullOrWhiteSpace(fen))
                    throw new ArgumentNullException(
                        nameof(fen),
                        "FEN cannot be null or empty.");

                ValidateFen(fen);

                SendCommand($"position fen {fen}");

                SendCommand($"go depth {depth}");

                return ReadSearchResult();
            }
            finally
            {
                ExitOperation();
            }
        }

        /// <summary>
        /// Reads the search result from Stockfish, including the best move and evaluation.
        /// </summary>
        /// <returns>A <see cref="StockfishSearchResult"/> containing the best move and evaluation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Stockfish process closes the output stream before returning a result.</exception>
        private StockfishSearchResult ReadSearchResult()
        {
            double evaluation = 0;

            string bestMove;
            while (true)
            {
                // Read a line from the Stockfish output stream with a timeout
                string? line = ReadLineWithTimeout();

                if (line is null)
                {
                    Console.WriteLine($"[Stockfish] {line}");

                    throw new InvalidOperationException(
                        "Stockfish closed the output stream before returning a result." + $"Process running: {!stockfishProcess.HasExited}");
                }

                // Log the line for debugging purposes
                if (line.StartsWith("info "))
                {
                    // Parse the evaluation score from the "info" line
                    string[] parts = line.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        // Check if the current part is "score" and the next part is either "cp" or "mate"
                        if (parts[i] == "score")
                        {
                            string scoreType = parts[i + 1];

                            // If the score type is "cp" (centipawns) or "mate", parse the score value and convert it to a double
                            if (i + 2 < parts.Length &&
                                double.TryParse(
                                    parts[i + 2],
                                    out double score))
                            {
                                // Convert the score to a double based on the score type
                                if (scoreType == "cp")
                                {
                                    evaluation = score / 100.0;
                                }
                                // If the score type is "mate", set the evaluation to positive or negative infinity based on the score value
                                else if (scoreType == "mate")
                                {
                                    evaluation = score > 0
                                        ? double.PositiveInfinity
                                        : double.NegativeInfinity;
                                }
                            }
                        }
                    }
                }

                // If the line starts with "bestmove ", parse the best move from the line and break out of the loop
                if (line.StartsWith("bestmove "))
                {
                    // Log the line for debugging purposes
                    string[] parts = line.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

                    // If the line does not contain at least two parts, throw an exception indicating that the best move response is invalid
                    if (parts.Length < 2)
                        throw new InvalidOperationException(
                            $"Invalid Stockfish bestmove response: {line}");

                    // If the line contains at least two parts, set the best move to the second part of the line
                    bestMove = parts[1];

                    break;
                }
            }

            // Return a new StockfishSearchResult object containing the best move and evaluation
            return new StockfishSearchResult(bestMove, evaluation);
        }

        #endregion

        #region -- READ STATIC EVALUATION --

        /// <summary>
        /// Evaluates a chess position given in FEN format and returns the evaluation score.
        /// </summary>
        /// <param name="fen">The FEN string representing the chess position.</param>
        /// <returns>The evaluation score of the position.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the FEN string is null or empty.</exception>
        public double Evaluate(string fen)
        {
            EnterOperation();
            try
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                if (string.IsNullOrWhiteSpace(fen))
                    throw new ArgumentNullException(nameof(fen), "FEN cannot be null or empty.");

                SendCommand($"position fen {fen}");

                SendCommand("eval");

                return ReadEvaluation();
            }
            finally
            {
                ExitOperation();
            }
        }

        /// <summary>
        /// Reads the static evaluation from Stockfish after sending the "eval" command. 
        /// This method continuously reads lines from the output until it finds a line that starts with "Final evaluation".
        /// </summary>
        /// <returns>The static evaluation as a double.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private double ReadEvaluation()
        {
            string? line;

            while((line = ReadLineWithTimeout()) is not null)
            {
                Debug.WriteLine($"[Stockfish] {line}");

                if (!line.StartsWith("Final evaluation", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Use a regular expression to extract the evaluation score from the line
                Match match = Regex.Match(line,
                    @"Final evaluation\s+([+-]?\d+(?:\.\d+)?)");

                // If the regex match is not successful, throw an exception indicating that the evaluation could not be parsed
                if (!double.TryParse(match.Groups[1].Value,
                    NumberStyles.Float, CultureInfo.InvariantCulture, out double evaluation))
                {
                    throw new InvalidOperationException($"Unable to convert Stockfish evaluation: {line}");
                }

                return evaluation;
            }

            throw new InvalidOperationException("Stockfish terminated before returning an evaluation.");
        }

        #endregion

        #region -- SEND COMMAND AND VALIDATION HELPER --

        /// <summary>
        /// Sends a command to the Stockfish process. This method writes the command to the standard input of the Stockfish process and 
        /// flushes the input stream to ensure that the command is sent immediately.
        /// </summary>
        /// <param name="command"></param>
        private void SendCommand(string command)
        {
            lock (syncRoot)
            {
                if (disposed) return;
                stockfishInput.WriteLine(command);
                stockfishInput.Flush();
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Validates the FEN string to ensure it is not null or empty. If the FEN string is invalid, an ArgumentNullException is thrown.
        /// </summary>
        /// <param name="fen">The FEN string to validate.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidateFen(string fen)
        {
            if (string.IsNullOrWhiteSpace(fen))
            {
                throw new ArgumentNullException(
                    nameof(fen),
                    "FEN cannot be null or empty.");
            }
        }

        // ============================================================
        // WAIT FOR
        // ============================================================
        
        /// <summary>
        /// Waits for a specific line of output from the Stockfish process. This method continuously reads lines from the output until it finds a line that matches the expected value.
        /// </summary>
        /// <param name="expected">The expected line of output.</param>
        /// <exception cref="InvalidOperationException">Thrown if the expected line is not found.</exception>
        private void WaitFor(string expected)
        {
            string? line;

            while((line = ReadLineWithTimeout()) is not null)
            {
                if(line.Equals(expected, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Stockfish did not return '{expected}'.");
        }

        #endregion

        #region -- IDisposable Implementation --

        /// <summary>
        /// Dispose the evaluator. This will prevent new operations from starting, request any running
        /// operations to stop, wait for active operations to finish (with a bounded timeout), then
        /// release native resources.
        /// </summary>
        public void Dispose()
        {
            // Fast path: if already disposed return
            lock (syncRoot)
            {
                if (disposed) return;
                if (isDisposing) return;
                // Mark that disposal has started so EnterOperation will reject new callers
                isDisposing = true;
            }

            // Ask stockfish to stop any ongoing search; ignore failures
            try
            {
                lock (syncRoot)
                {
                    if (!disposed)
                    {
                        try
                        {
                            stockfishInput.WriteLine("stop");
                            stockfishInput.Flush();
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Wait for active operations to complete. Use a bounded wait to avoid deadlocks.
            // Timeout: use max(5s, responseTimeout * 2)
            TimeSpan waitTimeout;
            try
            {
                var candidate = TimeSpan.FromMilliseconds(Math.Max(5000, responseTimeout.TotalMilliseconds * 2));
                waitTimeout = candidate;
            }
            catch
            {
                waitTimeout = TimeSpan.FromSeconds(5);
            }

            try
            {
                noActiveOperations.Wait(waitTimeout);
            }
            catch { }

            // Finalize disposal and release resources
            lock (syncRoot)
            {
                if (!disposed)
                {
                    disposed = true;
                }
            }

            try
            {
                try
                {
                    if (!stockfishProcess.HasExited)
                    {
                        // request exit and wait briefly
                        try { stockfishInput.WriteLine("quit"); stockfishInput.Flush(); } catch { }
                        if (!stockfishProcess.WaitForExit((int)waitTimeout.TotalMilliseconds))
                        {
                            try { stockfishProcess.Kill(); } catch { }
                        }
                    }
                }
                catch { }
                try { stockfishInput.Close(); } catch { }
                try { stockfishOutput.Close(); } catch { }
                try { stockfishProcess.Dispose(); } catch { }
            }
            finally
            {
                // Signal any threads waiting for disposal to complete
                try { disposalCompleted.Set(); } catch { }
                try { noActiveOperations.Dispose(); } catch { }
                try { disposalCompleted.Dispose(); } catch { }
            }
        }

        #endregion
    }
}
