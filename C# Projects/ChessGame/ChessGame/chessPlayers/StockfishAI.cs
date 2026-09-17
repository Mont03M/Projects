using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.SpecialMoves;
using ChessGame.ChessPieces;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Controls;
using ChessGame.Enums;
using ChessGame.Game.MoveQueue;
using ChessGame.Interfaces.Game;
using ChessGame.Square;
using ChessGame.Strategies.Tools.StockfishTools;
using ChessGame.Utilities;
using System.Diagnostics;
using static ChessGame.Strategies.Tools.StockfishTools.StockFishEvaluator;


namespace ChessGame.ChessPlayers
{
    /// <summary>
    /// StockfishAI class represents an AI player that uses the Stockfish chess engine to evaluate the current state of the chess board and select the best move.
    /// </summary>
    public class StockfishAI : ObservableObject
    {
        #region -- Properties --
        public GameControls GameControls { get; set; } = default!;
        public IsCheckControls IsCheckControls { get; set; } = default!;

        // file and rank coordinates to bridge gaps between stockfish castling locations and the chess board's own coordinate planes
        private readonly Dictionary<string, string> castlingLocations = new()
        {
            // white castling locations
            {"e1g1", "e1h1"},
            {"e1c1", "e1a1"},

            // black castling locations
            {"e8g8", "e8h8"},
            {"e8c8", "e8a8"}

        };

        public string GetParamName()
        {
            return $"{nameof(GameControls.ChessGameService)} is null or {nameof(GameControls.ChessGameService.selectedGameMode)} is null";
        }

        #endregion

        #region -- Constructor --

        public StockfishAI(GameControls gameControls)
        {
            GameControls = gameControls;
            IsCheckControls = new IsCheckControls(GameControls);
        }

        #endregion

        #region -- Stockfish Move Method --

        /// <summary>
        /// Method uses the Stockfish chess engine to evaluate the current state of the chess board and select the best move for the player.
        /// </summary>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <param name="playerColor">The color of the player making the move.</param>
        /// <param name="evaluator">The Stockfish evaluator used to analyze the board and select the best move.</param>
        /// <param name="stockfishMoveClock">The move clock for the Stockfish engine.</param>
        /// <param name="paramName">The name of the parameter to be used in exception messages.</param>
        /// <returns>A tuple containing the move information and the Stockfish move information.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(ChessMoveInfo? MoveInfo, StockChessMoveInfo? SfMoveInfo)> PlayerMove(Board? chessBoard, ChessPieceColors playerColor, 
            StockFishEvaluator evaluator, StockfishMoveClock stockfishMoveClock, string paramName)
        {
            if (chessBoard?.ChessSquares_ == null)
                throw new InvalidOperationException("StockfishAI must select a move!");

            try
            {
                if (GameControls.ChessGameService is null || GameControls.ChessGameService.selectedLevel is null)
                    throw new ArgumentNullException(paramName);
                
                string fen = string.Empty;

                // opponet is black, white is on bottom screen
                // otherwise the board orientation changed, white on top screen
                fen = BoardToFenConverter.ToFen(
                       chessBoard.DeepClone(),
                       playerColor,
                       (GameControls.ChessGameService.opponetColor is ChessPieceColors.BLACK),
                       chessBoard.WhiteCastleRightsKingSide,
                       chessBoard.WhiteCastleRightsQueenSide,
                       chessBoard.BlackCastleRightsKingSide,
                       chessBoard.BlackCastleRightsQueenSide,
                       chessBoard.EnPassantTarget.TargetSquare,
                       stockfishMoveClock.HalfMoveClock,
                       stockfishMoveClock.FullMoveClock
                       );
               
                // selected depth based on selected level
                int depth = 0;
                if (GameControls.ChessGameService.selectedLevel.Equals("Easy"))
                    depth = 2;
                else if (GameControls.ChessGameService.selectedLevel.Equals("Meduim"))
                    depth = 10;
                else
                    depth = 15;
                
                Debug.WriteLine($"FEN: {fen} depth: {depth}");
                
                // stockfish evaluator analyzes the fen to select the best move
                // returns the bestmove for the current chess board state and an evaluation score.
                StockfishSearchResult result = evaluator.Analyze(fen: fen, depth: depth);

                (ChessSquare? fromSquare, ChessSquare? toSquare, ChessPiece? promotionChessPiece) squares;

                // checks the length of the string
                // length 5 -- pawn promotion
                if (result.BestMove.Length == 5)
                    squares = SelectedSquaresWithPromotionChessPiece(result, playerColor, chessBoard); // promotion checks
                else
                    squares = SelectedSquares(result, chessBoard); // selected chess squares (file and rank coordinates)
        
                ArgumentNullException.ThrowIfNull(squares.fromSquare);
                ArgumentNullException.ThrowIfNull(squares.toSquare);

                // check for nulls early (denfensive-checks)
                if (squares.fromSquare?.ChessPiece_ is not ChessPiece selectedfromSquareChessPiece ||
                    squares.fromSquare?.ChessPiece_ is not IChessMoves movingChessPiece || 
                    squares.toSquare == null)
                {
                    throw new Exception($"Expected ChessMoves but got {squares.fromSquare?.ChessPiece_?.GetType().Name}");
                }

                // stores the move type of the best move selected
                MoveType moveType = MoveType.NORMAL;

                // check the selected move is valid 
                // Ensures chess board moves are valid and preserves the chess board states.
                bool isValid = PlayerControls.IsPlayerAvailableMove(movingChessPiece, squares.toSquare, out MovesAvailable? typeOfMove) ||
                               PlayerControls.IsPlayerAttackMove(movingChessPiece, squares.toSquare, out typeOfMove) ||
                               PlayerControls.IsPlayerSpecialMove(movingChessPiece, squares.toSquare, out typeOfMove);

                if (!isValid)
                {
                    throw new Exception("not valid!!!!");
                }

                // checks if the best move is a castling move
                if (squares.fromSquare != null && 
                    squares.toSquare != null && 
                    PlayerControls.PlayerSelectedCastlingMove(squares.fromSquare, squares.toSquare))
                {
                    // run castling chess piece movement on UI thread
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        // performs castling movement and updates each chess pieces current location
                        GameControls.PlayerControls.PlayerCastlingMove(squares.fromSquare, squares.toSquare, GameControls.ChessBoard);
                    });

                    // returns move type information
                    return (new ChessMoveInfo(playerColor, PieceType.KING, result.BestMove, MoveType.CASTLING), null);
                }

                // check if the best move is a capture 
                if (squares.toSquare != null &&
                    PlayerControls.IsCaptureMove(squares.toSquare) && 
                    squares.toSquare.ChessPiece_ != null)
                {
                    // store the file and rank of the captured chess piece
                    var chessPieceLocation = squares.toSquare.ChessPiece_.CurrentLocation;

                    // game mode is AI vs AI -- first AI represents the player -- second AI represents the opponent
                    if (GameControls.ChessGameService.selectedGameMode is not null &&
                        GameControls.ChessGameService.selectedGameMode.Equals("AI-AI") && playerColor == GameControls.ChessGameService.playerColor)
                    {
                        // add removed chess piece to a list of removed opponent chess pieces
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            GameControls.ChessBoard.RemovedOpponentChessPieces?.Add(squares.toSquare.ChessPiece_.PieceImage);
                        });

                    }
                    // regardless of game mode second stockfish AI's removed chess pieces
                    else
                    {
                        // add removed chess piece to a list of removed player chess pieces
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            GameControls.ChessBoard.RemovedPlayerChessPieces?.Add(squares.toSquare.ChessPiece_.PieceImage);
                        });
                    }

                    // removed capture chess piece from list of watched chess pieces on the chess board
                    chessBoard.RemoveChessPiece(colorToRemove: (playerColor is ChessPieceColors.WHITE) 
                        ? ChessPieceColors.BLACK : ChessPieceColors.WHITE, chessPieceToRemove: chessPieceLocation);

                    // captured chess piece is pawn type
                    if (squares.toSquare.ChessPiece_ is Pawn pawn && 
                        GameControls.SpecialMoveHandler?.PawnChessPieces.ContainsKey(pawn.PieceColor) == true)
                    {
                        // remove pawn from a list of watched pawns
                        GameControls.SpecialMoveHandler?.PawnChessPieces[pawn.PieceColor].Remove(pawn.PawnIndex);
                    }
                    else if (squares.toSquare.ChessPiece_ is Rook rook &&
                       GameControls.SpecialMoveHandler?.CastlingChessPieces.ContainsKey(rook.PieceColor) == true)
                    {
                        GameControls.SpecialMoveHandler?.CastlingChessPieces[rook.PieceColor].Remove(rook.CastleSide);
                    }

                    // move type is a capture
                    moveType = MoveType.CAPTURE;
                }

                // check if selected best move is an enPassant capture
                else if (squares.fromSquare != null && 
                    squares.toSquare != null &&
                    GameControls.PlayerControls.IsEnPassentCaptureMove(squares.fromSquare, squares.toSquare, GameControls.ChessBoard) && 
                    chessBoard.EnPassantTarget.Pawn != null)
                {
                    // store the file and rank of the captured chess piece (pawn)
                    var chessPieceLocation = chessBoard.EnPassantTarget.Pawn.CurrentLocation;

                    // run on UI thread - ensures pawn on the chess board is removed safely
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        // performs the enPassant capture move and updates the pawn performing the enPassant attack's current location.
                        PlayerControls.RemovePawnInEnPassentMove(GameControls.ChessBoard, playerColor, GameControls.ChessGameService);
                    });

                    // remove the capture pawn from a list of watched chess pieces on the chess board
                    chessBoard.RemoveChessPiece(colorToRemove: (playerColor is ChessPieceColors.WHITE)
                        ? ChessPieceColors.BLACK : ChessPieceColors.WHITE, chessPieceToRemove: chessPieceLocation);

                    // remove the pawn from a list of watched pawns 
                    if(squares.toSquare.ChessPiece_ is Pawn pawn && 
                        GameControls.SpecialMoveHandler?.PawnChessPieces.ContainsKey(pawn.PieceColor) == true)
                    {
                        GameControls.SpecialMoveHandler?.PawnChessPieces[pawn.PieceColor].Remove(pawn.PawnIndex);
                    }

                    moveType = MoveType.EnPASSENT;
                }

                // check for null values before performing any further actions.
                if (squares.toSquare != null && squares.fromSquare != null)
                {
                    // bestmove is a promotion move type
                    if(squares.promotionChessPiece != null)
                    {
                        // remove the pawn from a list of watched chess pieces
                        chessBoard.RemoveChessPiece(playerColor, squares.fromSquare.ChessPiece_.CurrentLocation);

                        // add the new chess piece to a list of watched chess pieces on the chess board
                        chessBoard.AddChessPiece(playerColor, squares.promotionChessPiece);
                    }

                    // Performs the chess piece movement from one square to another.
                    // Updates the chess pieces current location.
                    GameControls.MovementControls.PerfomChessPieceMove(
                        selectedfromSquareChessPiece, squares.fromSquare, 
                        squares.toSquare, GameControls.ChessBoard, 
                        squares.promotionChessPiece
                        );

                    // determine the move type
                    if (squares.promotionChessPiece is not null)
                        moveType = MoveType.PAWN_PROMOTION;
                    else if(typeOfMove is not null)
                        moveType = typeOfMove.MoveType;
                    
                }

                // Returns move type information used during the chess game.
                return (new ChessMoveInfo(playerColor, selectedfromSquareChessPiece.PieceType, result.BestMove, moveType, squares.promotionChessPiece?.PieceType), 
                    new StockChessMoveInfo(selectedfromSquareChessPiece.PieceType, moveType));
            }

            catch (Exception e)
            {
                Debug.WriteLine($"ERROR!!! {e.Message} --- Error {e.InnerException?.StackTrace} {e.StackTrace} " +
                    $"{e.GetBaseException().Data.Values} {e.Data.Values}");

                Debugger.Break();

            }

            return (null, null);
        }

        #endregion

        #region -- Selected Square Methods --

        /// <summary>
        /// Method parses the best move from the Stockfish search result and returns the corresponding from and to squares on the chess board.
        /// </summary>
        /// <param name="result">The Stockfish search result containing the best move and evaluation score.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>A tuple containing the from square, to square, and the chess piece involved in the move.</returns>
        public (ChessSquare? fromSquare, ChessSquare? toSquare, ChessPiece? chessPiece) SelectedSquares(StockfishSearchResult result, Board chessBoard)
        {
            string bestMove = string.Empty;
            
            // casting move is only valid if selected by the king chess piece for a stockfish player
            var checkSquare = chessBoard?.ChessSquares_?.FirstOrDefault(get => get.ChessBoardLocation.Equals(result.BestMove[..2]));

            // gets the correct castling locations defined by the chess board's coordinate planes
            if (checkSquare != null && 
                checkSquare.ChessPiece_ != null && 
                checkSquare.ChessPiece_.PieceType == PieceType.KING &&
                castlingLocations.TryGetValue(result.BestMove, out var bestMoveIsCastlingMove))
                bestMove = bestMoveIsCastlingMove;
            else
                bestMove = result.BestMove;

            // get move locations
            var from = bestMove[..2];
            var to = bestMove[2..4];

            // from square
            var fromSquare = chessBoard?.ChessSquares_?
                .FirstOrDefault(get => get.ChessBoardLocation.Equals(from));

            // to square
            var toSquare = chessBoard?.ChessSquares_?
                .FirstOrDefault(get => get.ChessBoardLocation.Equals(to));

            // null
            if (fromSquare is null && toSquare is null)
                return (null, null, null);

            return (fromSquare, toSquare, null);
        }


        /// <summary>
        /// Method parses the best move from the Stockfish search result and returns the corresponding from and to squares on the chess board, 
        /// along with the selected promotion chess piece if applicable.
        /// </summary>
        /// <param name="result">The Stockfish search result containing the best move and evaluation score.</param>
        /// <param name="playerColor">The current player's chess piece color.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>A tuple containing the from square, to square, and the selected promotion chess piece if applicable.</returns>
        public (ChessSquare? fromSquare, ChessSquare? toSquare, ChessPiece? chessPiece) SelectedSquaresWithPromotionChessPiece(
            StockfishSearchResult result, ChessPieceColors playerColor,Board chessBoard
            )
        {
            // parse input
            var from = result.BestMove[..2];
            var to = result.BestMove[2..4];

            // get promotion chess piece type
            char promotion = result.BestMove[4];
            promotion = char.ToUpperInvariant(promotion); // upper case char to get a consistent chess piece lookup

            // get squares
            var fromSquare = chessBoard?.ChessSquares_?
                .FirstOrDefault(get => get.ChessBoardLocation.Equals(from));
            var toSquare = chessBoard?.ChessSquares_?
                .FirstOrDefault(get => get.ChessBoardLocation.Equals(to));

            // stores the chess pieces used during a pawn promotion.
            IEnumerable<ChessPiece> promotionChessPieces = [];

            // check from square contains a chess piece and its of pawn type
            // check to Square for null values
            if (fromSquare?.ChessPiece_ is ChessPiece cp && cp is Pawn pawn && toSquare is not null)
            {
                // get chess piece selection based on the current player's chess piece color
                if (playerColor == ChessPieceColors.WHITE)
                {
                    promotionChessPieces = ChessPieceExtenstions.PawnPromotionChessPieces(
                        GameControls.AppSettingService.Settings.ChessSetSelected().whitePieces, pawn, toSquare.BoardLocation);
                }
                else
                {
                    promotionChessPieces = ChessPieceExtenstions.PawnPromotionChessPieces(
                        GameControls.AppSettingService.Settings.ChessSetSelected().blackPieces, pawn, toSquare.BoardLocation);
                }

                // build dictonary of selectable chess pieces (Rook, Queen, Bishop, Knight)
                var dictionaryChessPieces = promotionChessPieces.ToDictionary(x => x.PieceType, x => x);

                // get the selected chess piece type based on char value (N - knight, Q - queen, R - rook, B - bishop)
                var selectedChessPiece = dictionaryChessPieces[CharToPieceType(promotion)];
                // clone chess piece and get a chess piece of the selected chess piece type
                var chessPiece =  PawnPromotion.GetSelectedChessPiece(selectedChessPiece);

                return (fromSquare, toSquare, chessPiece);

            }

            return (fromSquare, toSquare, null);
        }

        #endregion

        #region -- Helper Method --

        /// <summary>
        /// Method converts a character representing a chess piece type to the corresponding PieceType enum value.
        /// </summary>
        /// <param name="pieceType">The character representing the chess piece type (e.g., 'N' for Knight, 'Q' for Queen).</param>
        /// <returns>The corresponding PieceType enum value.</returns>
        public static PieceType CharToPieceType(char pieceType)
        {
            return pieceType switch
            {
                'B' => PieceType.BISHOP,
                'N' => PieceType.KNIGHT,
                'R' => PieceType.ROOK,
                'Q' => PieceType.QUEEN,
                _ => PieceType.BISHOP,
            };
        }

        #endregion
    }
}

