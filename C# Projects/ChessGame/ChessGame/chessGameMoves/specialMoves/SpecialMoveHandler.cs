using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Interfaces.Services;
using ChessGame.Utilities;

namespace ChessGame.ChessGameMoves.SpecialMoves
{
    /// <summary>
    /// This class handles special moves in a chess game, including pawn promotion, en passant, and castling. 
    /// It tracks the state of pawns and castling pieces on the chessboard and provides methods to reset and set up the tracking of these pieces.
    /// </summary>
    public class SpecialMoveHandler : ObservableObject
    {
        #region -- Properties --
        private IIChessGameService? ChessGameService { get; set; } = null;
        public PawnPromotion PawnPromotion { get; set; }
        public EnPassant EnPassant { get; set; }
        public Castling Castling { get; set; }
        private Board ChessBoard { get; set; }
        private bool AllPawnsMoved_ { get; set; } = false;
        private Dictionary<ChessPieceColors, Dictionary<CastlingSide, ChessPiece>> CastlingChessPieces_ { get; set; } = [];
        private Dictionary<ChessPieceColors, Dictionary<int, Pawn?>> PawnChessPieces_ { get; set; } = [];
        public Dictionary<ChessPieceColors, Dictionary<CastlingSide, ChessPiece>> CastlingChessPieces
        {
            get => CastlingChessPieces_;
            set
            {
                lock (SyncRoot.MovesLock)
                {
                    CastlingChessPieces_ = value;
                }
                OnPropertyChanged(nameof(CastlingChessPieces));
            }
        }

        public Dictionary<ChessPieceColors, Dictionary<int, Pawn?>> PawnChessPieces
        {
            get => PawnChessPieces_;
            set
            {
                lock (SyncRoot.MovesLock)
                {
                    PawnChessPieces_ = value;
                }
                OnPropertyChanged(nameof(PawnChessPieces));
            }
        }

        public bool AllPawnsMoved
        {
            get=> AllPawnsMoved_;
            set
            {
                lock (SyncRoot.MovesLock)
                {
                    AllPawnsMoved_ = value;
                }
                OnPropertyChanged(nameof(AllPawnsMoved));
            }
        }

        #endregion

        #region -- Constructor --

        /// <summary>
        /// Initializes a new instance of the SpecialMoveHandler class with the specified chess board and game service.
        /// </summary>
        /// <param name="chessBoard"></param>
        /// <param name="chessGameService"></param>
        public SpecialMoveHandler(Board chessBoard, IIChessGameService? chessGameService)
        {
            ChessGameService = chessGameService;
            ChessBoard = chessBoard;

            PawnPromotion = new PawnPromotion();
            EnPassant = new EnPassant();
            Castling = new Castling();

            SetTrackChessPieces(ChessBoard);
        }
        #endregion

        #region -- Tracking Method --
        /// <summary>
        /// Sets up tracking for chess pieces on the board, specifically pawns and castling pieces, 
        /// by populating dictionaries that map piece colors to their respective pieces.
        /// </summary>
        /// <param name="chessBoard"> board type </param>
        public void SetTrackChessPieces(Board chessBoard)
        {
            if (chessBoard is not null)
            {
                // Initialize dictionaries to track pawns and castling pieces for both white and black players
                Dictionary<int, Pawn?> WhitePawnChessPieces = [];
                Dictionary<int, Pawn?> BlackPawnChessPieces = [];

                // Initialize dictionaries to track castling pieces for both white and black players
                Dictionary<CastlingSide, ChessPiece> WhiteCastlingPieces = [];
                Dictionary<CastlingSide, ChessPiece> BlackCastlingPieces = [];

                // Iterate through each square on the chessboard to identify and categorize pawns and castling pieces
                foreach (var square in chessBoard.ChessSquares_ ?? [])
                {
                    if (square.ChessPiece_ is not ChessPiece piece)
                        continue;

                    // Depending on the type of chess piece, add it to the appropriate tracking dictionary
                    switch (piece)
                    {
                        // Handle pawns by adding them to the pawn tracking dictionary based on their color
                        case Pawn pawn:
                            {
                                var pawnDictionary = pawn.PieceColor == ChessPieceColors.WHITE
                                    ? WhitePawnChessPieces
                                    : BlackPawnChessPieces;

                                var index = pawnDictionary.Count;

                                pawn.PawnIndex = index;
                                pawnDictionary[index] = pawn;
                                break;
                            }
                        // Handle kings by adding them to the castling tracking dictionary based on their color
                        case King king:
                            {
                                var castlingDictionary = king.PieceColor == ChessPieceColors.WHITE
                                    ? WhiteCastlingPieces
                                    : BlackCastlingPieces;
                                castlingDictionary[CastlingSide.KINGCASTLE] = king;
                                break;
                            }
                        // Handle rooks by determining their castling side and adding them to the castling tracking dictionary based on their color
                        case Rook rook:
                            {
                                // Determine the appropriate castling dictionary based on the rook's color
                                var castlingDictionary = rook.PieceColor == ChessPieceColors.WHITE
                                    ? WhiteCastlingPieces
                                    : BlackCastlingPieces;

                                // Determine the castling side (kingside or queenside) based on the rook's starting location and the player's color
                                CastlingSide side;

                                // Determine the castling side based on the rook's starting location and the player's color
                                if (ChessGameService?.playerColor == ChessPieceColors.WHITE)
                                {
                                    side = rook.StartLocation.Y == 0
                                      ? CastlingSide.QUEENSIDE
                                      : CastlingSide.KINGSIDE;
                                }
                                else
                                {
                                    side = rook.StartLocation.Y == 0
                                        ? CastlingSide.KINGSIDE
                                        : CastlingSide.QUEENSIDE;
                                }

                                // Assign the determined castling side to the rook and add it to the appropriate castling dictionary
                                rook.CastleSide = side;
                                castlingDictionary[side] = rook;
                                break;
                            }
                    }
                }

                // Assign the populated dictionaries to the class properties for tracking castling and pawn pieces
                CastlingChessPieces[ChessPieceColors.WHITE] = WhiteCastlingPieces;
                CastlingChessPieces[ChessPieceColors.BLACK] = BlackCastlingPieces;

                // Assign the populated dictionaries to the class properties for tracking pawn pieces
                PawnChessPieces[ChessPieceColors.WHITE] = WhitePawnChessPieces;
                PawnChessPieces[ChessPieceColors.BLACK] = BlackPawnChessPieces;

                // Set the AllPawnsMoved property to false, indicating that not all pawns have moved yet
                AllPawnsMoved = false;
            }
        }

        #endregion

        #region -- Reset Method --
        /// <summary>
        /// Resets the state of the SpecialMoveHandler, 
        /// clearing tracked chess pieces and reinitializing special move handlers for castling, en passant, and pawn promotion.
        /// </summary>
        public void Reset()
        {
            CastlingChessPieces = [];
            PawnChessPieces = [];

            Castling = new Castling();
            EnPassant = new EnPassant();
            PawnPromotion = new PawnPromotion();

            // Reinitialize tracking of chess pieces on the board after resetting
            SetTrackChessPieces(ChessBoard);
        }

        #endregion
    }
}
