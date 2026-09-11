using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.SpecialMoves;
using ChessGame.ChessPieces;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.ChessPlayers;
using ChessGame.Game.chessSoundPlayer;
using ChessGame.Controls;
using ChessGame.Enums;
using ChessGame.Game.GameTasks;
using ChessGame.Game.MoveQueue;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Square;
using ChessGame.Strategies.Tools.StockfishTools;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ChessGame.Game
{
    /// <summary>
    /// The Chess_Game class represents the main logic and state management for a chess game. It handles the game loop, player interactions, special moves, timers, 
    /// and game state transitions. The class integrates with various services and components to facilitate gameplay between human players and AI opponents.
    /// </summary>
    public class ChessGame : ObservableObject
    {
        #region -- Properties --

        private IIObjectService MainView { get; set; }
        private IIChessGameService ChessGameService { get; set; }
        private AppSettingsService AppSettingsService { get; set; }
        public StockFishEvaluator? StockFishEval { get; set; } = null;
        public StockfishMoveClock StockfishMoveClock { get; set; }
        public MovesQueue MovesQueue { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private TaskCompletionSource PlayerTurnPause { get; set; } = new();
        private TaskCompletionSource PausePawnSelection { get; set; } = new();
        private TaskCompletionSource TimerExpiredPause { get; set; } = new();
        private TaskCompletionSource PauseGameIsOver { get; set; } = new();
        public Board ChessBoard { get; set; }
        public GameControls GameControls { get; set; }
        public Player Player { get; set; }
        public StockfishAI [] StockfishAIs { get; set; }
        private int MoveCount { get; set; }
        private bool OneCompleteTurn { get; set; } = false;
        private bool GenerateMoves { get; set; }
        private ChessPieceColors TurnEvaluated { get; set; } = ChessPieceColors.NONE;
        private King? BlackKing { get; set; }
        private King? WhiteKing { get; set; }
        private Task? _mainGameLoopTask;
        private Task? _monitorTask;

        #endregion

        #region -- Constructor --
        /// <summary>
        /// Initializes a new instance of the Chess_Game class with the specified services. Sets up the chess board, game controls, players, and starts the game processes.
        /// </summary>
        /// <param name="mainViewModel">The main view model service.</param>
        /// <param name="chessGameService">The chess game service.</param>
        /// <param name="appSettingsService">The application settings service.</param>
        public ChessGame(IIObjectService mainViewModel, IIChessGameService chessGameService, 
            AppSettingsService appSettingsService)
        {
            // <----------------------- Services ------------------------------------>
            this.MainView = mainViewModel;
            this.ChessGameService = chessGameService;
            this.AppSettingsService = appSettingsService;
           
            StockFishEval = new StockFishEvaluator();
            StockfishMoveClock = new StockfishMoveClock();
            ArgumentNullException.ThrowIfNull(StockFishEval);
            ArgumentNullException.ThrowIfNull(StockfishMoveClock);

            // // <----------------------- Chess Board ------------------------------------>
            Application.Current.Dispatcher.Invoke(async () =>
            {
                this.ChessBoard = new Board(chessGameService, appSettingsService).DrawBoard();
            });

            // <----------------------- Controls ------------------------------------>
            ArgumentNullException.ThrowIfNull(ChessBoard);
            GameControls = new GameControls(MainView, appSettingsService, ChessBoard, chessGameService);

            // <----------------------- Chess board messages  ------------------------------------>
            MovesQueue = new MovesQueue(); // <-- message board object // moves to show on message board

            // <----------------------- Players ------------------------------------>
            StockfishAIs = new StockfishAI[2];
            Player = new Player(GameControls);

            ArgumentNullException.ThrowIfNull(ChessGameService.selectedGameMode, "Game mode must be set!");
            Debug.WriteLine($"Selected game mode: {ChessGameService.selectedGameMode}");
            if (ChessGameService.selectedGameMode.Equals("AI-AI"))
            {
                StockfishAIs[0] = new StockfishAI(GameControls);
                StockfishAIs[1] = new StockfishAI(GameControls);
            }
            else
            {
                StockfishAIs[0] = new StockfishAI(GameControls);

                // <----------------------- commands ----------------------------->
                this.MainView.mainViewModel.ExecuteMovePieceCommand = new AsyncRelayCommand(ExecutePlayerMove);
                this.MainView.mainViewModel.ExecutePawnPromotionCommand = new AsyncRelayCommand(ExecutePawnPromotion);
            }

            // <----------------------- Tokens ------------------------------------>
            PausePawnSelection = new TaskCompletionSource();
            PlayerTurnPause = new TaskCompletionSource();
            CancellationTokenSource = new CancellationTokenSource();
            
            // <----------------------- Game ------------------------------------>
            GenerateMoves = true;
            MoveCount = 0;

            // <----------------------- Player kings ------------------------------------>
            BlackKing = GameControls.FindKingChessPiece(ChessPieceColors.BLACK, ChessBoard);
            WhiteKing = GameControls.FindKingChessPiece(ChessPieceColors.WHITE, ChessBoard);
            ArgumentNullException.ThrowIfNull(BlackKing); // error
            ArgumentNullException.ThrowIfNull(WhiteKing); // error

            // Tasks for chess game
            StartGameProcesses();
        }

        #endregion

        #region -- Game Processes --

        /// <summary>
        /// Run two independent background tasks: one for the main game loop and one for periodic checks/timer/special moves monitoring.
        /// Both tasks observe the same cancellation token and the chessGameService.IsGameOver flag.
        /// </summary>
        public void StartGameProcesses()
        {
            // If either task is already running, do not start new tasks.
            if (_mainGameLoopTask is not null && !_mainGameLoopTask.IsCompleted) return;
            if (_monitorTask is not null && _monitorTask.IsCompleted) return;

            // Create a new cancellation token source for this run
            var token = CancellationTokenSource.Token;

            // Task for main game loop
            _mainGameLoopTask = Task.Run(async () =>
            {
                // Main game loop: runs continuously until cancellation is requested or the game is over
                while (!token.IsCancellationRequested && !ChessGameService.IsGameOver)
                {
                    try
                    {
                        // Run the main game loop logic
                        await RunChessGame();
                        await Task.Delay(100);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("TaskConceledExpection");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"ERROR in RunGameLoop: {e.Message} <---> {e.StackTrace}");
                    }
                }
            }, token);

            // Task for monitoring checks, special moves, and timers
            _monitorTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !ChessGameService.IsGameOver)
                {
                    try
                    {
                        // Track the number of moves played
                        await TrackMoveCount();

                        // Check if either player is in check and update the board accordingly
                        await Task.WhenAll(
                            GameControls.SettingControls.IsCheckIndicator(
                                () => GameControls.FindKingChessPiece(ChessGameService.opponetColor, ChessBoard), ChessBoard),
                            GameControls.SettingControls.IsCheckIndicator(
                                () => GameControls.FindKingChessPiece(ChessGameService.playerColor, ChessBoard), ChessBoard)
                        );

                        // Track special moves (castling, en passant, pawn promotion) in parallel
                        await Task.WhenAll(
                            TrackCastlingMoves(), 
                            TrackPawnEnPassantMoves(), 
                            TrackPawnPromotionMoves());

                        // If lighting chess mode is enabled, check if the timer has expired
                        if (ChessGameService.LightingChess)
                        {
                            // Check if the game timer has expired and handle game over if necessary
                            await IsTimerExpired();
                        }

                        await Task.Delay(300);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("TaskConceledExpection");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("In timer loop");
                        break;
                        
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"ERROR in Timer/Checks: {e.Message} <---> {e.StackTrace}");
                    }
                }

                // If game over detected, cancel the shared token to stop other tasks if needed
                if (ChessGameService.IsGameOver && !CancellationTokenSource.Token.IsCancellationRequested)
                {
                    try { CancellationTokenSource.Cancel(); } catch { }
                }
                
            }, token);
        }

        /// <summary>
        /// Stops the game processes by signaling cancellation to the background tasks. Optionally cancels any TaskCompletionSources to unblock awaits, 
        /// and waits for the tasks to complete within a specified timeout.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for the tasks to complete.</param>
        /// <param name="cancelTCS">Whether to cancel any TaskCompletionSources to unblock awaits.</param>
        /// <param name="waitForCompletion">Whether to wait for the tasks to complete before returning.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task StopGameProcessesAsync(TimeSpan? timeout = null, bool cancelTCS = false, bool waitForCompletion = true)
        {
            // request cancellation for any loops using the shared token
            try { CancellationTokenSource.Cancel(); } catch { }

            if (cancelTCS)
            {
                // Make sure any TaskCompletionSources awaited inside the loops are completed/canceled
                // so that awaits like `await PlayerTurnPause.Task` don't block forever.
                try { PlayerTurnPause?.TrySetResult(); } catch { }
                try { PausePawnSelection?.TrySetResult(); } catch { }
                try { TimerExpiredPause?.TrySetResult(); } catch { }
                try { PauseGameIsOver?.TrySetResult(); } catch { }
            }
            if (!waitForCompletion)
            {
                // Do not wait for the background loops to finish. Caller explicitly requested fire-and-forget.
                // We already signaled cancellation above; clear local references and return immediately.
                _mainGameLoopTask = null;
                _monitorTask = null;
                return;
            }

            var t1 = _mainGameLoopTask ?? Task.CompletedTask;
            var t2 = _monitorTask ?? Task.CompletedTask;
            var all = Task.WhenAll(t1, t2);

            if (timeout.HasValue)
            {
                var finished = await Task.WhenAny(all, Task.Delay(timeout.Value));
                if (finished != all)
                {
                    Debug.WriteLine("StopGameProcessesAsync: timeout waiting for loops to finish.");
                }
            }
            else
            {
                try { await all; } catch { /* ignore exceptions from canceled tasks */ }
            }

            _mainGameLoopTask = null;
            _monitorTask = null;
        }

        #endregion

        #region -- Chess Game --

        /// <summary>
        /// Method runs the game, generates moves, tracks checks and checkmates, timers (if enabled),
        /// resets board squares, and track pawn promotions
        /// </summary>
        /// <returns> void </returns>
        private async Task RunChessGame()
        {
            if (ChessGameService == null)
                return;

            // check if game is in a playable state
            if (ChessGameService.IsGameOver || ChessGameService.IsGamePause || ChessGameService.IsPawnPromotionSelection)
            {
                // game over
                if (ChessGameService.IsGameOver)
                {
                    await PauseGameIsOver.Task;

                    return;
                }

                if (ChessGameService.IsPawnPromotionSelection)
                {
                    // Game is in a pawn promotion selection state
                    if (ChessGameService.IsPawnPromotionSelection && ChessGameService.LightingChess)
                    {
                        await Application.Current.Dispatcher.Invoke(async () =>
                        {
                            ChessGameService?.StopPlayerTimer?.Invoke();
                        });
                    }

                    // pause game until pawn promotion selection is complete
                    await PausePawnSelection.Task;
                    return;
                }

                if (ChessGameService.IsGamePause)
                {
                    await PlayerTurnPause.Task;
                    return;
                }
            }

            // Only regenerate moves when flagged to avoid expensive repeated work
            if (GenerateMoves)
            {
                await TaskWrapper.TaskVoidWrapper(() => { MovementControls.GenerateBoardMoves(ChessBoard); });

                // clear flag until next move modifies the board
                GenerateMoves = false;
            }
            else
            {
                // small delay so the loop can't spin tightly when nothing changed
                await Task.Delay(200, CancellationTokenSource.Token);
            }

            // game is in a playable state
            if ((!ChessGameService.IsGamePause && !ChessGameService.IsGameOver) || !ChessGameService.IsPawnPromotionSelection)
            {
                // check if either player is in check and update the board accordingly
                var playerInCheck = await IsAnyPlayerInCheck();

                // check if game is in a playable state
                if (StockFishEval?.IsDisposed == false)
                {
                    // check if the game is in a draw or checkmate state
                    var gameState = await DetermineGameState(playerInCheck, ChessBoard, ChessGameService.CurrentTurn);
                    var isDraw = gameState.GameState is GameState.Draw;

                    // if game is in a checkmate state, exit the loop
                    if (gameState.GameState is GameState.Checkmate)
                        return;

                    PlayerTurnPause = new TaskCompletionSource();

                    // if game is not in a draw state and it's the player's turn, execute the player's move
                    if (!isDraw && ChessGameService.CurrentTurn == ChessGameService.playerColor)
                    {
                        ArgumentNullException.ThrowIfNull(ChessGameService.selectedGameMode, "Game mode must be seleted to continue!");
                        ArgumentNullException.ThrowIfNull(StockFishEval, "Stockfish evaluator must be set!");

                        if (ChessGameService.selectedGameMode.Equals("AI-AI"))
                        {
                            // prevent opponent from making move if game is over
                            if (ChessGameService.IsGamePause)
                            {
                                // pause game until player turn is complete
                                await PlayerTurnPause.Task;
                                await Task.Delay(100, CancellationTokenSource.Token);
                            }

                            // execute npc's turn
                            await StockfishPlayer(StockfishAIs[1], StockFishEval, ChessGameService.playerColor);

                            // player escaped check, reset
                            if (StockfishAIs[1].IsCheckControls.IsPlayerCheck)
                                StockfishAIs[1].IsCheckControls.IsPlayerCheck = false;
                        }
                        // if game is not in a draw state and it's the player's turn, execute the player's move
                        else
                        {
                            // if game is not in a draw state and it's the player's turn, execute the player's move
                            await PlayerTurnPause.Task;
                            
                            // player escaped check, reset 
                            if (Player.IsCheckControls.IsPlayerCheck)
                                Player.IsCheckControls.IsPlayerCheck = false;
                        }

                    }
                    else if (!isDraw && ChessGameService.CurrentTurn != ChessGameService.playerColor)
                    {

                        ArgumentNullException.ThrowIfNull(StockFishEval, "Stockfish evaluator must be set!");

                        // prevent opponent from making move if game is over
                        if (ChessGameService.IsGamePause)
                        {
                            await PlayerTurnPause.Task;
                            await Task.Delay(100, CancellationTokenSource.Token);
                        }

                        // execute npc's turn
                        await StockfishPlayer(StockfishAIs[0], StockFishEval, ChessGameService.opponetColor);

                        // player escaped check, reset
                        if (StockfishAIs[0].IsCheckControls.IsPlayerCheck)
                            StockfishAIs[0].IsCheckControls.IsPlayerCheck = false;
                    }

                    // check if game is in a playable state
                    if (gameState.GameState is GameState.Draw)
                        await IsGameOver(ChessPieceColors.NONE, gameState);

                    else
                        StockfishMoveClock.UpdateFullMoveClock(MoveCount); // update full move clock

                    await Task.Delay(500);
                }

            }
        }

        #endregion

        #region -- Players --

        /// <summary>
        /// Executes the Stockfish AI player's move.
        /// </summary>
        /// <param name="stockfishPlayer">The Stockfish AI player.</param>
        /// <param name="evaluator">The Stockfish evaluator.</param>
        /// <param name="playerColor">The color of the player.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task StockfishPlayer(StockfishAI stockfishPlayer, StockFishEvaluator evaluator, ChessPieceColors playerColor)
        {
            // find npc's king
            var king = (playerColor == ChessPieceColors.WHITE) ? WhiteKing : BlackKing;
            ArgumentNullException.ThrowIfNull(king);
            await Task.Delay(500);

            // prevent opponent from making a move if the game is paused
            if (ChessGameService.IsGamePause)
                await PlayerTurnPause.Task;
            

            // execute npc's turn
            (ChessMoveInfo? MoveInfo, StockChessMoveInfo? SfMoveInfo) = await stockfishPlayer.PlayerMove(
                ChessBoard, playerColor, evaluator, StockfishMoveClock, stockfishPlayer.GetParamName()
                );

            // add move to message board
            if (MoveInfo is not null)
            {
                // add to chess board
                MovesQueue.PushMove(MoveInfo);

                GenerateMoves = true;

                // end of npc's turn
                ChessGameService.CurrentTurn = (playerColor is ChessPieceColors.WHITE) ? ChessPieceColors.BLACK : ChessPieceColors.WHITE;

                // update half move clock
                StockfishMoveClock.UpdateHalfMoveClock(SfMoveInfo);

                // reset player clock for the new turn (only when timer enabled and not in pawn promotion)
                if (ChessGameService.LightingChess)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => ChessGameService.PlayerClockReset?.Invoke());
                }

                // set enPassant reset color to remove EnPassant after each players turn
                // if enPassant not set by the same player
                if (!ChessBoard.EnPassantTarget.TargetSquare.Equals("-"))
                {
                    ChessBoard.EnPassantTarget.ResetColor = playerColor;
                }

                // prevent opponent from if game is pause
                if (ChessGameService.IsGamePause)
                    await PlayerTurnPause.Task;
                

                // reset board squares after npc's turn
                await TaskWrapper.TaskVoidWrapper(() => { GameControls.SettingControls.ResetBoardSquares(ChessBoard.ChessSquares_); });
            }
        }
       

        /// <summary>
        /// Executes the player's move when a chess square is selected. Validates the move, updates the game state, and manages turn transitions.
        /// </summary>
        /// <param name="square">The selected chess square.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutePlayerMove(object? square)
        {
            // check if the selected square is a valid chess square and if it's the player's turn
            if ((square is ChessSquare chessSquare && ChessGameService.CurrentTurn == ChessGameService.playerColor))
            {
                // reset board squares after player has made a move
                GameControls.SettingControls.ResetBoardSquares(GameControls.ChessBoard.ChessSquares_);

                await Task.Delay(100);

                // execute player's turn
                (ChessMoveInfo? MoveInfo, StockChessMoveInfo? SfMoveInfo) = await Player.PlayerMove(
                    chessSquare, ChessGameService.playerColor, Player.GetParamName()
                    );

                // add move to message board
                if (MoveInfo is not null)
                {
                    MovesQueue.PushMove(MoveInfo);

                    GenerateMoves = true;

                    // end of player's turn
                    ChessGameService.CurrentTurn = ChessGameService.opponetColor;
                    
                    // update half move clock
                    StockfishMoveClock.UpdateHalfMoveClock(SfMoveInfo);

                    // stop player timer of move selected
                    if (ChessGameService.LightingChess)
                        await Application.Current.Dispatcher.InvokeAsync(() => ChessGameService.StopPlayerTimer?.Invoke());

                    // set enPassant reset color to remove EnPassant after each players turn
                    // if enPassant not set by the same player
                    if (!ChessBoard.EnPassantTarget.TargetSquare.Equals("-"))
                    {
                        ChessBoard.EnPassantTarget.ResetColor = ChessGameService.playerColor;
                    }

                    // reset board squares after npc's turn\
                    // un-pause the game to continue
                    await Task.Delay(400);
                    PlayerTurnPause.TrySetResult();
                    PlayerTurnPause = new TaskCompletionSource();
                }
            }
        }

        #endregion

        #region -- Special Moves Monitoring --

        /// <summary>
        /// Tracks pawn en passant moves.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TrackPawnEnPassantMoves()
        {
            await TaskWrapper.TaskVoidWrapper(() => { GameControls.MonitorEnPassantActivity(MoveCount, ChessBoard); }); // await

        }

        /// <summary>
        /// Tracks pawn promotion moves.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TrackPawnPromotionMoves()
        {
            await TaskWrapper.TaskVoidWrapper(() => { GameControls.MonitorPawnPromotedMoveActivity(ChessBoard); });
        }

        /// <summary>
        /// Tracks castling moves.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TrackCastlingMoves()
        {
            await TaskWrapper.TaskVoidWrapper(() => { GameControls.MonitorCastlingActivity(ChessBoard); });
        }

        #endregion

        #region -- Player Checks and Checkmates --
        /// <summary>
        /// Checks if any player is currently in check and updates the game state accordingly. 
        /// If a player is in check, it generates the possible moves to escape check and checks for checkmate conditions.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a boolean indicating whether any player is in check.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<bool> IsAnyPlayerInCheck()
        {
            // check if game is in a playable state
            var CurrentTurn = ChessGameService.CurrentTurn;

            // find the king of current player
            var king = (ChessGameService.CurrentTurn == ChessPieceColors.WHITE) ? WhiteKing : BlackKing;
            ArgumentNullException.ThrowIfNull(king); // throw exception

            (bool IsCheck, List<ChessPiece> TargetChessPieces) results = (false, []);

            // check if the current player is in check and get the list of attacking pieces
            if (CurrentTurn == ChessGameService.playerColor)
            {
                ArgumentNullException.ThrowIfNull(ChessGameService.selectedGameMode, "Game mode not set!");
                if (ChessGameService.selectedGameMode.Equals("AI-AI"))
                {
                    // check if the current player is in check and get the list of attacking pieces
                    results = await TaskWrapper.TaskGenericWrapper(() => {
                        return StockfishAIs[1].IsCheckControls.IsCheck(playerColor: ChessGameService.playerColor, king: king, chessBoard: ChessBoard);
                    });
                }
                else
                {
                    // check if the current player is in check and get the list of attacking pieces
                    results = await TaskWrapper.TaskGenericWrapper(() =>
                    {
                        return Player.IsCheckControls.IsCheck(playerColor: ChessGameService.playerColor, king: king, chessBoard: ChessBoard);
                    });
                }
            }
            else
            {
                // check if the current player is in check and get the list of attacking pieces
                results = await TaskWrapper.TaskGenericWrapper(() => 
                {
                    return StockfishAIs[0].IsCheckControls.IsCheck(playerColor: ChessGameService.opponetColor, king: king, chessBoard: ChessBoard); 
                });
            }

            // if the current player is in check, generate the possible moves to escape check and check for checkmate conditions
            if (results.IsCheck)
            {
                // update the move type to CHECK for the last move in the queue
                MovesQueue?.ModifyTopOfStack(move =>
                {
                    move.Type = MoveType.CHECK;
                });

                // generate the possible moves to escape check for the current player's king
                await TaskWrapper.TaskVoidWrapper(() => { GameControls.MovementControls.GenerateIsCheckMoves(king, results.TargetChessPieces); });

                await Task.Delay(500);

                // if there are no possible moves to escape check, the current player is checkmated
                if (results.TargetChessPieces.Count == 0)
                    throw new InvalidOperationException("Target Chess Pieces is empty!");

                bool IsCheckMate = false;

                // check if the current player is checkmated based on the game mode and the current turn
                if (CurrentTurn == ChessGameService.playerColor)
                {
                    ArgumentNullException.ThrowIfNull(ChessGameService.selectedGameMode, "Game mode not set!");

                    if (ChessGameService.selectedGameMode.Equals("AI-AI"))
                    {
                        // check if the current player is checkmated based on the game mode and the current turn
                        IsCheckMate = await TaskWrapper.TaskGenericWrapper(() =>
                        {
                            return StockfishAIs[1].IsCheckControls.IsCheckMate(playerColor: ChessGameService.playerColor, king: king, 
                                 targetChessPieces: results.TargetChessPieces, chessBoard: ChessBoard);
                        });
                    }
                    else
                    {
                        // check if the current player is checkmated based on the game mode and the current turn
                        IsCheckMate = await TaskWrapper.TaskGenericWrapper(() =>
                        {
                            return Player.IsCheckControls.IsCheckMate(ChessGameService.playerColor, king, 
                                results.TargetChessPieces, ChessBoard);
                        });
                    }

                }
                else
                {
                    // check if the current player is checkmated based on the game mode and the current turn
                    IsCheckMate = await TaskWrapper.TaskGenericWrapper(() => { 
                        return StockfishAIs[0].IsCheckControls.IsCheckMate(playerColor: ChessGameService.opponetColor,king: king, 
                            targetChessPieces: results.TargetChessPieces, chessBoard: ChessBoard); });
                }

                // if the current player is checkmated, update the move type to CHECKMATE and end the game with the winner
                if (IsCheckMate)
                {
                    // update the move type to CHECKMATE for the last move in the queue
                    MovesQueue?.ModifyTopOfStack(move =>
                    {
                        move.Type = MoveType.CHECKMATE;
                    });

                    // determine the winner based on the current turn
                    var Winner = (CurrentTurn == ChessPieceColors.WHITE) ? ChessPieceColors.BLACK : ChessPieceColors.WHITE;

                    // end the game and show the winner
                    await IsGameOver(Winner, (null, null));
                }

                // return true to indicate that the current player is in check
                return true;
            }

            // return false to indicate that the current player is not in check
            return false;
        }

        #endregion

        #region -- Pawn Promotion Relay Command --

        /// <summary>
        /// Executes the pawn promotion process when a player selects a piece to promote their pawn to. Updates the chess board and game state accordingly.
        /// </summary>
        /// <param name="selectedPiece">The chess piece selected to replace the pawn.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutePawnPromotion(object? selectedPiece)
        {
            ChessGameService.IsPawnPromotionSelection = true;

            // cast object
            var promotion = selectedPiece as ChessPiece;

            // check if the promotion piece and chess board are valid
            if (promotion is not null && ChessBoard.ChessSquares_ != null)
            {
                // get the square and the selected chess piece for promotion
                var square = promotion.CurrentLocation.GetSquare(ChessBoard.ChessSquares_);
                var chessPiece = PawnPromotion.GetSelectedChessPiece(promotion);

                // if the square and chess piece are valid, update the chess board and game state
                if (square != null && square.ChessPiece_ != null && chessPiece != null)
                {
                    // remove the pawn from the chess board and add the selected piece for promotion
                    ChessBoard.RemoveChessPiece(ChessGameService.playerColor, square.ChessPiece_.CurrentLocation);
                    ChessBoard.AddChessPiece(ChessGameService.playerColor, chessPiece);

                    // switch pawn for selected piece on the chess board
                    square.ChessPiece_ = chessPiece;

                    GameControls.PawnPromotionChessPieces = null;

                    // hide pawn promotion selection grid and reset the pawn promotion state
                    GameControls.IsPawnSelectionGridVisible = Visibility.Collapsed;
                    ChessGameService.IsPawnPromotionSelection = false;

                    // un-pause the game to continue
                    PausePawnSelection.TrySetResult();
                    PausePawnSelection = new TaskCompletionSource();
                }
            }
        }
        #endregion

        #region -- Chess Game Turn ((White + Black) == 1 turn) Count --

        /// <summary>
        /// Tracks the number of moves played in the game. Increments the move count after each complete turn (white and black).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task TrackMoveCount()
        {
            // Increment the move count after each complete turn (white and black)
            if (ChessGameService.CurrentTurn == ChessPieceColors.WHITE && OneCompleteTurn)
            {                
                MoveCount += 1;
                OneCompleteTurn = false;
            }
            // Increment the move count after each complete turn (white and black)
            else if(ChessGameService.CurrentTurn == ChessPieceColors.BLACK && !OneCompleteTurn)
            {
                OneCompleteTurn = true;
            }
        }

        #endregion

        #region -- Game Timer Expiration --

        /// <summary>
        /// Checks if the game timer has expired for either player. If the game time or player time reaches zero, 
        /// it sets the game over state and invokes the end-of-game dialog with appropriate messages.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task IsTimerExpired()
        {
            if(ChessGameService.GameTimeRemaining <= TimeSpan.Zero)
            {
                ChessGameService.IsGameOver = true;

                // Show end of game dialog for time expiration
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    ChessGameService.IsEndOfGame?.Invoke("Time's Up!",
                        $"The game has ended due to time expiration. The Match is a draw!\nNew Match to start a new game.\nHome to quit game.",
                        "Home", "New Match!");
                });

                await TimerExpiredPause.Task;
            }
            // Check if the player's time has expired
            else if (ChessGameService.PlayerTimeRemaining <= TimeSpan.Zero)
            {
                ChessGameService.IsGameOver = true;

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    ChessGameService.IsEndOfGame?.Invoke("Time's Up!",
                        $"The game has ended due to time expiration.\n{ChessGameService.opponetColor} is the winner!\nNew Match to start a new game.\nHome to quit game.",
                        "Home", "New Match!");
                });

                // Wait for the timer expired pause task to complete
                await TimerExpiredPause.Task;
            }
        }

        #endregion

        #region -- Game Over --

        /// <summary>
        /// Checks if the game is over due to checkmate or draw conditions. 
        /// If the game is over, it sets the game state accordingly and invokes the end-of-game dialog with appropriate messages.
        /// </summary>
        /// <param name="winner">The color of the winning player.</param>
        /// <param name="chessBoardState">The state of the chess board.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task IsGameOver(ChessPieceColors winner, (GameState? GameState, string? GameStateMsg ) chessBoardState)
        {
            PlayerTurnPause = new TaskCompletionSource();
            ChessGameService.IsGameOver = true;
            ChessGameService.IsGamePause = true;

            DispatcherOperation<Task> dispatcherOp;

            // Check if the game is over due to a draw condition
            if (chessBoardState.GameState is not null && 
                chessBoardState.GameState is GameState.Draw && 
                chessBoardState.GameStateMsg is not null )
            {
                // Show end of game dialog for draw condition
                dispatcherOp = Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    // Play draw sound effect if enabled
                    await ChessGameService.IsEndOfGame.Invoke($"DRAW!",
                        $"{chessBoardState.GameStateMsg}\nNew Match to start a new game!\nHome to exit game.", "Home", "New Match!");
                });
            }
            else
            {
                // Show end of game dialog for checkmate condition
                if (AppSettingsService.Settings.IsEnableSoundEffects)
                   ChessSoundPlayer.PlayCheckmate();

                // stop player timer if lighting chess mode is enabled
                if (ChessGameService.LightingChess)
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        ChessGameService.StopPlayerTimer?.Invoke();
                    });
                }

                // Show end of game dialog for checkmate condition
                dispatcherOp = Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await ChessGameService.IsEndOfGame.Invoke($"Checkmate!",
                        $"\n{winner} won the Match!\nNew Match to start a new game!\nHome to exit game.", "Home", "New Match!");
                });
            }

            // Wait for the dispatcher operation to complete
            await dispatcherOp.Task.Unwrap().ConfigureAwait(true);
            
        }

        #endregion

        #region -- Game State Determination --

        /// <summary>
        /// Determines the current game state based on the player's check status, the chess board configuration, and the current turn.
        /// </summary>
        /// <param name="currentPlayerInCheck">Indicates whether the current player is in check.</param>
        /// <param name="inCheck">Indicates whether the opponent is in check.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <param name="currentTurn">The color of the player whose turn it is.</param>
        /// <returns>A tuple containing the game state and a message describing the game state.</returns>
        private async Task<(GameState GameState, string GameStateMsg)> DetermineGameState(bool inCheck, Board chessBoard, ChessPieceColors currentTurn)
        {
            // Check if the game state has already been evaluated for the current turn to avoid redundant evaluations
            if (TurnEvaluated == currentTurn)
                return (GameState.Continuing, "");
            else
                TurnEvaluated = currentTurn;

            // Convert the current chess board state to FEN notation for evaluation
            var fen = BoardToFenConverter.ToFen(
                      ChessBoard,
                      currentTurn,
                      (ChessGameService.opponetColor is ChessPieceColors.BLACK),
                      ChessBoard.WhiteCastleRightsKingSide,
                      ChessBoard.WhiteCastleRightsQueenSide,
                      ChessBoard.BlackCastleRightsKingSide,
                      ChessBoard.BlackCastleRightsQueenSide,
                      ChessBoard.EnPassantTarget.TargetSquare
                      );

            // Evaluate the game state using Stockfish to determine if the current player is in checkmate
            var results = StockFishEval?.DetermineGameState(fen, inCheck);

            // Record the current position in the game history to track repetitions for draw conditions
            RecordCurrentPosition(fen);

            // Check if the game state indicates a checkmate condition
            if (results.HasValue && results.Value is GameState.Checkmate)
                return (GameState.Checkmate, "");

            // Check if the game state indicates a draw condition based on stalemate, insufficient material, or repetition
            var isDraw = IsDraw(currentTurn, inCheck, fen, chessBoard, results);

            // If the game is in a draw state, return the draw state and message
            if (isDraw.IsDraw)
                return (GameState.Draw, isDraw.ChessBoardStateMsg);

            // If the current player is in check, return the check state
            if (inCheck)
                return (GameState.Check, "");

            // If none of the above conditions are met, return the continuing game state
            return (GameState.Continuing, "");

        }

        /// <summary>
        /// Determines if the game is in a draw state based on stalemate, insufficient material, or repetition conditions.
        /// </summary>
        /// <param name="currentTurn">The color of the player whose turn it is.</param>
        /// <param name="inCheck">Indicates whether the opponent is in check.</param>
        /// <param name="fen">The FEN notation representing the current state of the chess board.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <param name="chessBoardState">The current state of the chess board as determined by Stockfish.</param>
        /// <returns>A tuple indicating whether the game is in a draw state and a message describing the draw condition.</returns>
        private (bool IsDraw, string ChessBoardStateMsg) IsDraw(
            ChessPieceColors currentTurn, bool inCheck, string fen, Board chessBoard, GameState? chessBoardState)
        {
            // Check for stalemate condition: If the current player is not in check and has no legal moves, it's a stalemate.
            if (IsStalemate(currentTurn, inCheck, chessBoardState))
                return (true, "Stalemate! King is not in check but has no legal moves!");

            // Check for insufficient material condition: If neither player has enough pieces to checkmate, it's a draw.
            if (IsInsufficientMaterial(chessBoard))
                return (true, "Insufficient Material");

            // Check for the 50-move rule condition: If 100 half-moves have occurred without a pawn move or capture, it's a draw.
            if (StockfishMoveClock.HalfMoveClock >= 100)
                return (true, "100 consecutive moves halfmoves have occured without a pawn move or capture!");

            // Check for fivefold repetition condition: If the same position has occurred five times, it's a draw.
            if (IsFivefoldRepetiton(fen))
                return (true, "Same Positions\noccurred five times!");

            // If none of the draw conditions are met, return false indicating the game is not in a draw state.
            return (false, "");
        }

        /// <summary>
        /// Determines if the current player is in a stalemate condition. A stalemate occurs when the current player is not in check but has no legal moves available.
        /// </summary>
        /// <param name="currentPlayer">The color of the player whose turn it is.</param>
        /// <param name="inCheck">Indicates whether the current player is in check.</param>
        /// <param name="chessBoardState">The current state of the chess board as determined by Stockfish.</param>
        /// <returns>True if the current player is in a stalemate condition; otherwise, false.</returns>
        private bool IsStalemate(ChessPieceColors currentPlayer, bool inCheck, GameState? chessBoardState)
        {
            // Check for stalemate condition: If the current player is not in check and has no legal moves, it's a stalemate.
            if (chessBoardState is not null && !inCheck && chessBoardState is GameState.Stalemate)
            {
                // Get the list of legal moves available for the current player using the ChessPieceExtensions class.
                var legalMoves = ChessPieceExtenstions.GetMoves<MovesAvailable>(ChessBoard, currentPlayer);
                // If there are no legal moves available, return true indicating a stalemate condition.
                if (legalMoves.Count == 0) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the current position has occurred five times in the game history, indicating a fivefold repetition draw condition.
        /// </summary>
        /// <param name="fen">The FEN string representing the current board position.</param>
        /// <returns>True if the position has occurred five times; otherwise, false.</returns>
        private bool IsFivefoldRepetiton(string fen)
        {
            // Get the repetition key for the current position based on the FEN string.
            string repetitionKey = GetRepetitionKey(fen);

            // Check if the repetition key exists in the PositionOccurrences dictionary and if the count is greater than or equal to 5.
            return ChessGameService.PositionOccurrences.TryGetValue(repetitionKey, out int count) && count >= 5;
        }

        /// <summary>
        /// Determines if the game is in an insufficient material condition, where neither player has enough pieces to checkmate the opponent.
        /// </summary>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>True if the game is in an insufficient material condition; otherwise, false.</returns>
        private static bool IsInsufficientMaterial(Board chessBoard)
        {
            ArgumentNullException.ThrowIfNull(chessBoard.ChessSquares_);

            // Get the list of chess pieces currently on the board, excluding null squares.
            List<ChessPiece> chessPiecesOnBoard = [.. chessBoard.ChessSquares_
                .Where(square => square.ChessPiece_ is not null)
                .Select(square => square.ChessPiece_!)];

            // Count the number of chess pieces on the board that are not kings.
            int chessPiecesNotKing = chessPiecesOnBoard.Count(cp => cp is not null && cp.PieceType != PieceType.KING);

            // king vs king
            if (chessPiecesNotKing == 0) return true;

            // any pawn, rook, or queen on board -- checkmate is possible
            if (chessPiecesOnBoard.Any(cp => cp is not null && (cp.PieceType is PieceType.PAWN or PieceType.ROOK or PieceType.QUEEN))) return false;

            // At this point, the only possible pieces are:
            // King, Bishop, Knight

            // King + Knight vs King
            if (chessPiecesNotKing == 1)
                return true;

            // King + Bishop vs King + Bishop
            if (chessPiecesNotKing == 2)
            {
                ChessPiece[] minorPieces = [.. chessPiecesOnBoard.Where(cp => cp.PieceType is PieceType.BISHOP or PieceType.KNIGHT)];

                // Both remaining pieces must be bishops
                if (minorPieces.All(cp => cp.PieceType == PieceType.BISHOP))
                {
                    // Need to determine whether both bishops
                    // occupy the same-colored squares.
                    return AreBishopsOnSameColor(minorPieces);
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if two bishops are on the same color squares. In chess, bishops can only move diagonally and are restricted to squares of the same color throughout the game. 
        /// This method checks if both bishops occupy squares of the same color.
        /// </summary>
        /// <param name="bishops"></param>
        /// <returns></returns>
        private static bool AreBishopsOnSameColor(ChessPiece[] bishops)
        {
            if(bishops.Length != 2) return false;

            ChessSquareLocation bishop1 = bishops[0].CurrentLocation;

            ChessSquareLocation bishop2 = bishops[1].CurrentLocation;

            // Determine the color of the squares occupied by each bishop based on their coordinates.
            bool bishop1Color = (bishop1.X + bishop1.Y) % 2 == 0;
            bool bishop2Color = (bishop2.X + bishop2.Y) % 2 == 0;

            // Return true if both bishops occupy squares of the same color, otherwise return false.
            return bishop1Color == bishop2Color;
        }

        /// <summary>
        /// Generates a unique key for the current board position based on the FEN string. The key is used to track the number of occurrences of the same position in the game history, 
        /// which is important for detecting draw conditions such as threefold or fivefold repetition.
        /// </summary>
        /// <param name="fen">The FEN string representing the current board position.</param>
        /// <returns>A unique key representing the current board position.</returns>
        /// <exception cref="ArgumentException">Thrown when the FEN string is invalid.</exception>
        private static string GetRepetitionKey(string fen)
        {
            // Split the FEN string into its components using space as the delimiter. The FEN string consists of six fields: piece placement, active color, castling availability,
            // en passant target square, halfmove clock, and fullmove number.
            string[] fields = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if(fields.Length != 6)
            {
                throw new ArgumentException("Invalid FEN.", nameof(fen));
            }

            // Return a unique key for the current board position by joining the first four fields of the FEN string.
            // The first four fields are sufficient to represent the position for repetition detection.
            return string.Join(
                ' ',
                fields[0],
                fields[1],
                fields[2],
                fields[3]);
        }

        /// <summary>
        /// Records the current board position in the game history by updating the count of occurrences for the given FEN string.
        /// </summary>
        /// <param name="fen">The FEN string representing the current board position.</param>
        private void RecordCurrentPosition(string fen)
        {
            string repetitionKey = GetRepetitionKey(fen);

            // Update the count of occurrences for the current position in the PositionOccurrences dictionary.
            if (ChessGameService.PositionOccurrences.TryGetValue(
                repetitionKey,
                out int count))
            {
                ChessGameService.PositionOccurrences[repetitionKey] = count + 1;
            }
            else
            {
                ChessGameService.PositionOccurrences[repetitionKey] = 1;
            }
        }

        #endregion

        #region -- Game Reset, Restart, and Exit --

        /// <summary>
        /// Resets the chess game to its initial state. Stops any ongoing game processes, clears the move stack, resets the chess board, and reinitializes game state variables.
        /// </summary>
        /// <returns></returns>
        public async Task Reset()
        {
            // stop any ongoing game processes with a timeout of 2 seconds and cancel the task completion source
            await StopGameProcessesAsync(timeout: TimeSpan.FromSeconds(2), cancelTCS: true);

            ChessBoard.Reset();

            // clear move stack
            MovesQueue.Clear();

            Player?.IsCheckControls.Reset();

            // reset check controls for all Stockfish AI players
            foreach (var stockfishPlayer in StockfishAIs ?? [])
            {
                // npc 
                if (stockfishPlayer is not null
                    && stockfishPlayer.IsCheckControls.IsPlayerCheck == true)
                {
                    stockfishPlayer.IsCheckControls.Reset();
                }
            }

            // reset move clock
            StockfishMoveClock.Reset();

            // replace cancellation token source
            CancellationTokenSource = new CancellationTokenSource();

            // reinitialize TCS before starting processes to avoid race
            PlayerTurnPause = new TaskCompletionSource();
            PausePawnSelection = new TaskCompletionSource();
            PauseGameIsOver = new TaskCompletionSource();

            // reset game
            ChessGameService.CurrentTurn = ChessPieceColors.WHITE;
            ChessGameService.IsGameOver = false;
            ChessGameService.IsCheckMateWhite = false;
            ChessGameService.IsCheckMateBlack = false;
            ChessGameService.IsGamePause = false;
            ChessGameService.PositionOccurrences.Clear();

            // reset game state variables
            MoveCount = 0;
            OneCompleteTurn = false;
            GenerateMoves = true;
            TurnEvaluated = ChessPieceColors.NONE;

            // find kings on the chessboard
            BlackKing = GameControls.FindKingChessPiece(ChessPieceColors.BLACK, ChessBoard);
            WhiteKing = GameControls.FindKingChessPiece(ChessPieceColors.WHITE, ChessBoard);

            // reset game controls
            await GameControls.Reset();

            // reset timer expired pause task completion source if lighting chess mode is enabled
            if (ChessGameService.LightingChess)
            {
                TimerExpiredPause = new TaskCompletionSource();
            }

            // now start fresh loops
            StartGameProcesses();

        }

        /// <summary>
        /// Unpauses the chess game, allowing players to continue their turns. 
        /// This method sets the game pause state to false and signals any waiting tasks that the game has resumed.
        /// </summary>
        /// <returns></returns>
        public async Task UnPauseGame()
        {
            ChessGameService.IsGamePause = false;
            PlayerTurnPause.TrySetResult();
            PlayerTurnPause = new TaskCompletionSource();
        }

        /// <summary>
        /// Restarts the chess game loop, optionally pausing the game before restarting.
        /// </summary>
        /// <param name="isPause"></param>
        /// <returns></returns>
        public async Task RestartGameLoop(bool isPause = false)
        {
            if (isPause)
            {
                ChessGameService.IsGamePause = false;
                //PlayerTurnPause.TrySetResult();
            }

            // stop any ongoing game processes with a timeout of 1 second
            await StopGameProcessesAsync(TimeSpan.FromSeconds(1));
            CancellationTokenSource = new CancellationTokenSource();
            StartGameProcesses();
        }

        /// <summary>
        /// Exits the chess game by stopping any ongoing game processes, stopping the Stockfish evaluation, 
        /// and disposing of any resources associated with the Stockfish evaluation.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ExitGame()
        {
            await StopGameProcessesAsync(TimeSpan.FromSeconds(1));
            StockFishEval?.StopSearch();
            StockFishEval?.Dispose();
        }

        #endregion
    }
}
