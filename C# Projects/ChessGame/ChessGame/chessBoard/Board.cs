using ChessGame.ChessGameMoves.SpecialMoves.helpers;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Square;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;


namespace ChessGame.ChessBoard
{
    /// <summary>
    /// Represents a chess board with its squares, pieces, and game state.
    /// </summary>
    public class Board : ObservableObject
    {
        #region -- Properties ---
        private IIChessGameService ChessGameService { get; set; } = default!;

        private AppSettingsService? AppSettingsService { get; set; }

        // Chess Board squares
        private ObservableCollection<ChessSquare>? ChessSquares { get; set; } = default!;

        private List<ChessPiece> WhiteChessPieces_ { get; set; } = [];
        private List<ChessPiece> BlackChessPieces_ { get; set; } = [];
        private ObservableCollection<string>? removedPlayerChessPieces;
        private ObservableCollection<string>? removedOpponentChessPieces;

        // chess board caslting rights 
        public bool WhiteCastleRightsKingSide { get; set; } = true;
        public bool WhiteCastleRightsQueenSide { get; set; } = true;
        public bool BlackCastleRightsKingSide { get; set; } = true;
        public bool BlackCastleRightsQueenSide { get; set; } = true;

        // chess board active enPassant Target 
        private EnPassantHelper EnPassantTarget_ { get; set; } = new EnPassantHelper("-", 0, null, null, null, null);
        public EnPassantHelper EnPassantTarget
        {
            get => EnPassantTarget_;
            set
            {
                // mutate under shared lock; do not hold lock while raising property changed
                lock (SyncRoot.MovesLock)
                {
                    EnPassantTarget_ = value;
                }
                OnPropertyChanged(nameof(EnPassantTarget));
            }
        }

        // List of rank and file coordiantes
        public string[ , ] chessBoardLocation = new string[,]
        {
            // 0    1    2    3    4    5    6    7
            { "a8","b8","c8","d8","e8","f8","g8","h8" }, // 0 
            { "a7","b7","c7","d7","e7","f7","g7","h7"},  // 1
            { "a6","b6","c6","d6","e6","f6","g6","h6" }, // 2
            { "a5","b5","c5","d5","e5","f5","g5","h5"}, //  3
            { "a4","b4","c4","d4","e4","f4","g4","h4" },//  4 
            { "a3","b3","c3","d3","e3","f3","g3","h3"},//   5
            { "a2","b2","c2","d2","e2","f2","g2","h2" },//  6
            { "a1","b1","c1","d1","e1","f1","g1","h1"},//   7

        };

        // List of files
        public ObservableCollection<string> Files { get; } = ["A", "B", "C", "D", "E", "F", "G", "H"];

        // List of ranks
        public ObservableCollection<int> Ranks { get; } =
        [
            8, 7, 6, 5, 4, 3, 2, 1
        ];

        // List of chess squares
        public ObservableCollection<ChessSquare>? ChessSquares_
        {
            get => ChessSquares;
            set
            {
                // assign under lock, but raise property changed outside lock to avoid deadlocks
                lock (SyncRoot.MovesLock)
                {
                    ChessSquares = value;
                }
                OnPropertyChanged(nameof(ChessSquares_));
            }
        }

        // List of removed player chess pieces
        public ObservableCollection<string>? RemovedPlayerChessPieces
        {
            get => removedPlayerChessPieces;
            set
            {
                removedPlayerChessPieces = value;
                OnPropertyChanged(nameof(RemovedPlayerChessPieces));
            }
        }

        // List of removed opponent chess pieces
        public ObservableCollection<string>? RemovedOpponentChessPieces
        {
            get => removedOpponentChessPieces;
            set
            {
                removedOpponentChessPieces = value;
                OnPropertyChanged(nameof(RemovedOpponentChessPieces));
            }
        }

        // Lost of white chess pieces on the board
        public List<ChessPiece> WhiteChessPieces
        {
            get => WhiteChessPieces_;
            set
            {
                WhiteChessPieces_ = value;
                OnPropertyChanged(nameof(WhiteChessPieces));
            }
        }

        // List of black chess pieces on the board
        public List<ChessPiece> BlackChessPieces
        {
            get => BlackChessPieces_;
            set
            {
                BlackChessPieces_ = value;
                OnPropertyChanged(nameof(BlackChessPieces));
            }
        }


        // Helper to create a SolidColorBrush and freeze it so it's safe to use across threads
        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }
            return brush;
        }

        // determine which color is on the bottom screen
        public bool IsBottomPlayer { get; set; } = true;

        #endregion

        #region -- BoardPlacementHelper --

        // helper function
        private readonly PieceType[] boardPlacementHelper =
        [
            PieceType.ROOK,
            PieceType.KNIGHT,
            PieceType.BISHOP,
            PieceType.QUEEN,
            PieceType.KING,
            PieceType.BISHOP,
            PieceType.KNIGHT,
            PieceType.ROOK
        ];

        #endregion

        #region -- Constructor 1 (Used for Deep Copy)--
        public Board(ObservableCollection<ChessSquare> squares)
        {
            ChessSquares_ = new ObservableCollection<ChessSquare>(squares);
        }
        #endregion

        #region -- Constructor 2 --
        public Board(IIChessGameService chessGameService, AppSettingsService appSettingsService) 
        {
            this.ChessGameService = chessGameService;
            this.AppSettingsService = appSettingsService;

            this.ChessSquares_ = [];
            this.RemovedOpponentChessPieces = [];
            this.RemovedPlayerChessPieces = [];

            IsBottomPlayer = (ChessGameService.playerColor == ChessPieceColors.WHITE);

            // switch the ranks and files if the player is black
            if (!IsBottomPlayer)
            {
                SwitchSides(Ranks);
                SwitchSides(Files);
            }
        }
        #endregion

        #region -- Settings and Game Board Functions ---

        /// <summary>
        /// Method is used to draw a chess board for setting configurations 
        /// </summary>
        /// <param name="oddColor"> color of odd chess squares on the board</param>
        /// <param name="evenColor"> color of even chess squares on the board </param>
        public void DrawSettingsBoard(Color oddColor, Color evenColor)
        {
            ChessSquares_?.Clear();
            WhiteChessPieces?.Clear();
            BlackChessPieces?.Clear();

            ArgumentNullException.ThrowIfNull(AppSettingsService);

            for (int i = 0; i < 8; i++)
            {
                // for each column
                for (int j = 0; j < 8; j++)
                {
                    // get file and rank
                    var rankAndFile = chessBoardLocation[i, j];

                    // set chess pieces on the board for settings configuration
                    if (i == 0 && j == 1)
                    {
                        // black pawn 1
                        var pawnB1 = new Pawn(ChessGameService, PieceType.PAWN, ChessPieceColors.BLACK,
                                [Special_Moves.EnPASSENT, Special_Moves.PAWN_PROMOTION],
                                new ChessSquareLocation(i, j),
                                AppSettingsService.Settings.ChessSetSelected().blackPieces[PieceType.PAWN]
                            );

                        ChessSquares_?.Add(new ChessSquare
                        (
                            CreateFrozenBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                            new ChessSquareLocation(i, j),
                            rankAndFile,
                            pawnB1
                        ));

                        BlackChessPieces?.Add(pawnB1);
                    }
                    else if (i == 1 && j == 4)
                    {
                        // black pawn 2
                        var pawnB2 = new Pawn(ChessGameService, PieceType.PAWN, ChessPieceColors.BLACK,
                                [Special_Moves.EnPASSENT, Special_Moves.PAWN_PROMOTION],
                                new ChessSquareLocation(i, j),
                                AppSettingsService.Settings.ChessSetSelected().blackPieces[PieceType.PAWN]
                            );

                        ChessSquares_?.Add(new ChessSquare
                        (
                            CreateFrozenBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                            new ChessSquareLocation(i, j),
                            rankAndFile,
                            pawnB2
                            
                        ));

                        BlackChessPieces?.Add(pawnB2);
                    }
                    else if(i==4 && j == 1)
                    {
                        // white rook 1
                        ChessSquares_?.Add(new ChessSquare
                        (
                           CreateFrozenBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                           new ChessSquareLocation(i, j),
                           rankAndFile,
                           new Rook(ChessGameService, PieceType.ROOK, ChessPieceColors.WHITE,
                               Special_Moves.CASLTING,
                               new ChessSquareLocation(i, j),
                               AppSettingsService.Settings.ChessSetSelected().whitePieces[PieceType.ROOK],
                                (j == 0) ? CastlingSide.QUEENSIDE : CastlingSide.KINGSIDE
                           )
                        ));
                    }
                    else if (i == 4 && j == 2)
                    {
                        // black pawn 3
                        var pawnB3 = new Pawn(ChessGameService, PieceType.PAWN, ChessPieceColors.BLACK,
                                  [Special_Moves.EnPASSENT, Special_Moves.PAWN_PROMOTION],
                                  new ChessSquareLocation(i, j),
                                  AppSettingsService.Settings.ChessSetSelected().blackPieces[PieceType.PAWN]
                              );

                        ChessSquares_?.Add(new ChessSquare
                          (
                              CreateFrozenBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                              new ChessSquareLocation(i, j),
                              rankAndFile,
                              pawnB3
                          ));

                        BlackChessPieces?.Add(pawnB3);
                    }
                    else if (i == 4 && j == 7)
                    {
                        // white bishop 1
                        ChessSquares_?.Add(new ChessSquare
                        (
                            new SolidColorBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                            new ChessSquareLocation(i, j),
                            rankAndFile,
                            new Bishop(ChessGameService, PieceType.BISHOP, ChessPieceColors.WHITE,
                                new ChessSquareLocation(i, j),
                                AppSettingsService.Settings.ChessSetSelected().whitePieces[PieceType.BISHOP]
                            )
                        ));
                    }
                    else if (i == 6 && j == 5)
                    {
                        // black pawn 4
                        var pawnB4 = new Pawn(ChessGameService, PieceType.PAWN, ChessPieceColors.BLACK,
                                [Special_Moves.EnPASSENT, Special_Moves.PAWN_PROMOTION],
                                new ChessSquareLocation(i, j),
                                AppSettingsService.Settings.ChessSetSelected().blackPieces[PieceType.PAWN]
                            );

                        ChessSquares_?.Add(new ChessSquare
                        (
                            new SolidColorBrush((i + j) % 2 == 0 ? oddColor : evenColor),
                            new ChessSquareLocation(i, j),
                            rankAndFile,
                            pawnB4
                            
                        ));

                        BlackChessPieces?.Add(pawnB4);
                    }
                  
                    else
                    {
                        // empty chess squares on the board
                        ChessSquares_?.Add(new ChessSquare
                        (
                               new SolidColorBrush((j + i) % 2 == 0 ? oddColor : evenColor),
                               new ChessSquareLocation(i, j),
                               rankAndFile
                        ));

                    }
                    
                }
            }
        }

        /// <summary>
        /// Method is used to draw the game chess board
        /// Places chess pieces on the correct squares based on the choosen chess set and player color
        /// Sets two coordinates types (x,y coordinates) and (file and rank coordinates) for each chess square on the board
        /// </summary>
        /// <returns>Board (this) used for method chaining</returns>
        // draw chess board
        public Board DrawBoard()
        {
            ArgumentNullException.ThrowIfNull(AppSettingsService);

            // clear chess squares
            if (ChessSquares_?.Count != 0)
                ChessSquares_?.Clear();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // ranks are 2 or 7 place a pawn chess piece, otherwise get a chess piece based on the boardPlacementHelper array (file)
                    var pieceType = (i == 1 || i == 6) ? PieceType.PAWN : boardPlacementHelper[j];

                    // invert rank and file coordinates if the player is black (not bottom player)
                    var rankAndFile = (!IsBottomPlayer) ? chessBoardLocation[7 - i, 7 - j] : chessBoardLocation[i, j];

                    // players chess pieces are on ranks 1 and 2 (i = 6,7) and opponets chess pieces are on ranks 7 and 8 (i = 0,1)
                    if (i == 0 || i == 1 || i == 6 || i == 7)
                    {
                        // place king on right if player is white and on the bottom screen, otherwise place king on the left side 
                        // player is black and on the bottom screen, place king on the right side, otherwise place king on the left side
                        if ((pieceType is PieceType.KING || pieceType is PieceType.QUEEN) && !IsBottomPlayer)
                            pieceType = (pieceType is PieceType.QUEEN) ? PieceType.KING : PieceType.QUEEN;
                        
                        // player chess pieces 
                        if ((i == 6 || i == 7))
                        {

                            // place player chess pieces with choosen chess set color
                            PlaceChessPiece(i, j, pieceType, ChessGameService.playerColor, rankAndFile,
                                GetChessPieces(ChessGameService.playerColor));
                        }
                        else
                        {
                            // place opponet chess pices with other chess set color
                            PlaceChessPiece(i, j, pieceType, ChessGameService.opponetColor, rankAndFile, 
                                GetChessPieces(ChessGameService.opponetColor));
                        }
                    }
                    else
                    {
                        // empty chess squares on the board
                        ChessSquares_?.Add(new ChessSquare
                        (
                            new SolidColorBrush((Color)((j + i) % 2 == 0 ?
                            AppSettingsService.Settings.squareOdd ?? Colors.White : AppSettingsService.Settings.squareEven ?? Colors.Black)),
                            new ChessSquareLocation(i, j),
                            rankAndFile
                        ));
                    }
                }
            }

            // List of chess pieces on the board
            TrackChessPieces();

            return this;
        }
        #endregion

        #region -- Drawing Board Helper Functions ---
        /// <summary>
        /// Places the correct chess piece on the correct location on the chess board 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="pieceType"></param>
        /// <param name="pieceColor"></param>
        /// <param name="chessPieces"></param>
        // place chess pieces based on PieceType and choosen chess set
        private void PlaceChessPiece(int i, int j, PieceType pieceType, ChessPieceColors pieceColor, string rankAndFile, 
            Dictionary<PieceType, string> chessPieces)
        {
            switch (pieceType)
            {
                case PieceType.PAWN:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j), 
                        new Pawn(ChessGameService, pieceType, pieceColor,
                        [Special_Moves.EnPASSENT, Special_Moves.PAWN_PROMOTION], 
                        new ChessSquareLocation(i, j), 
                        chessPieces[pieceType]), rankAndFile);
                    break;
                case PieceType.BISHOP:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j), 
                        new Bishop(ChessGameService, 
                        pieceType, 
                        pieceColor, 
                        new ChessSquareLocation(i, j), 
                        chessPieces[pieceType]), rankAndFile);
                    break;
                case PieceType.KNIGHT:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j), 
                        new Knight(ChessGameService, pieceType, pieceColor, 
                        new ChessSquareLocation(i, j), 
                        chessPieces[pieceType]), rankAndFile);
                    break;
                case PieceType.ROOK:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j),
                        new Rook(ChessGameService, pieceType, pieceColor,
                        Special_Moves.CASLTING,
                        new ChessSquareLocation(i, j),
                        chessPieces[pieceType],
                        (j == 0) ? CastlingSide.QUEENSIDE : CastlingSide.KINGSIDE), rankAndFile);
                    break;
                case PieceType.QUEEN:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j), 
                        new Queen(ChessGameService, pieceType, pieceColor, 
                        new ChessSquareLocation(i, j), 
                        chessPieces[pieceType]), rankAndFile);
                    break;
                case PieceType.KING:
                    SetSquareWithChessPiece(new ChessSquareLocation(i, j), 
                        new King(ChessGameService, pieceType, pieceColor, 
                        Special_Moves.CASLTING, 
                        new ChessSquareLocation(i, j), 
                        chessPieces[pieceType]), rankAndFile);
                    break;
            }
        }

        /// <summary>
        /// Sets a square with a chess piece on the chess board
        /// </summary>
        /// <param name="setLocation"></param>
        /// <param name="chessPiece"></param>
        // place chess piece in the correct location
        private void SetSquareWithChessPiece(ChessSquareLocation setLocation, ChessPiece chessPiece, string rankAndFile)
        {
            ArgumentNullException.ThrowIfNull(AppSettingsService);

            ChessSquares_?.Add(new ChessSquare
            (
               new SolidColorBrush((Color)((setLocation.Y + setLocation.X) % 2 == 0 ?
              AppSettingsService.Settings.squareOdd ?? Colors.White : AppSettingsService.Settings.squareEven ?? Colors.Black)),
               setLocation,
               rankAndFile,
               chessPiece
            ));

        }



        /// <summary>
        ///  // determine chess piece set for players
        /// </summary>
        /// <param name="playerColorChoosen"></param>
        /// <returns>Dictionary containing chess piece set for a specific chess piece color</returns>
        private Dictionary<PieceType, string> GetChessPieces(ChessPieceColors playerColorChoosen)
        {
            ArgumentNullException.ThrowIfNull(AppSettingsService);
            return (playerColorChoosen == ChessPieceColors.WHITE) ?
               AppSettingsService.Settings.ChessSetSelected().whitePieces : AppSettingsService.Settings.ChessSetSelected().blackPieces;
        }

        private static void SwitchSides<T>(ObservableCollection<T> collection)
        {
            for (int i = 0; i < collection.Count / 2; i++)
            {
                int oppositeIndex = collection.Count - 1 - i;
                (collection[i], collection[oppositeIndex]) = (collection[oppositeIndex], collection[i]);
            }

        }

        #endregion

        #region -- Board Tracking Chess Piece Helpers ---

        /// <summary>
        /// Method is used to track the chess pieces on the board and store them in two separate lists for white and black chess pieces
        /// </summary>
        private void TrackChessPieces()
        {
            // clear the lists of chess pieces before tracking
            if (WhiteChessPieces?.Count != 0)
                WhiteChessPieces?.Clear();

            if (BlackChessPieces?.Count != 0)
                BlackChessPieces?.Clear();

            // loop through each chess square on the board and add the chess pieces to the appropriate list based on their color
            foreach (var square in ChessSquares_ ?? [])
            {
                if(square is not null &&
                    square.ChessPiece_ is ChessPiece chessPiece)
                {
                    if (chessPiece.PieceColor is ChessPieceColors.WHITE)
                        WhiteChessPieces?.Add(chessPiece);
                    else
                        BlackChessPieces?.Add(chessPiece);
                }
            }
        }

        /// <summary>
        /// Method is used to remove a chess piece from the board and the appropriate list of chess pieces based on their color
        /// </summary>
        /// <param name="colorToRemove"></param>
        /// <param name="chessPieceToRemove"></param>
        public void RemoveChessPiece(ChessPieceColors colorToRemove, ChessSquareLocation chessPieceToRemove)
        {
            // remove chess piece from the appropriate list based on their color
            var chessPieces = (colorToRemove is ChessPieceColors.WHITE) ? WhiteChessPieces : BlackChessPieces;

            // remove chess piece from the list of chess pieces
            foreach (var cp in chessPieces.ToList())
            {
                if (cp.CurrentLocation == chessPieceToRemove)
                {
                    chessPieces.Remove(cp);
                }
            }
        }

        /// <summary>
        /// Method is used to add a chess piece to the board and the appropriate list of chess pieces based on their color
        /// </summary>
        /// <param name="colorToAdd"> The color of the chess piece to add </param>
        /// <param name="chessPiece"> The chess piece to add </param>
        public void AddChessPiece(ChessPieceColors colorToAdd, ChessPiece chessPiece)
        {
            var chessPieces = (colorToAdd is ChessPieceColors.WHITE) ? WhiteChessPieces : BlackChessPieces;

            chessPieces.Add(chessPiece);
        }

        #endregion

        #region -- Board Moves Function ---
        /// <summary>
        /// Generates the moves for the current chess board
        /// </summary>
        public void BoardMoves()
        {
            try
            {
                if (ChessSquares_ is not null)
                {
                    // generate valid moves for all chess pieces on the board except for the king
                    foreach (var square in ChessSquares_)
                    {

                        if (square.ChessPiece_ != null &&
                            square.ChessPiece_ is ChessPiece chessPiece &&
                            chessPiece is IChessMoves chessMoves &&
                            !string.IsNullOrWhiteSpace(square.ChessPiece_.PieceImage))
                        {
                            if (chessMoves is not King)
                            {
                                chessMoves.GenerateValidMoves(this);
                            }
                        }
                    }

                    // generate available moves and attack moves for the king
                    // this is done after generating valid moves for all other pieces to ensure that the king's moves are not blocked by other pieces
                    foreach (var square in ChessSquares_)
                    {

                        if (square.ChessPiece_ != null &&
                            square.ChessPiece_ is ChessPiece chessPiece &&
                            chessPiece is IChessMoves chessMoves &&
                            !string.IsNullOrWhiteSpace(square.ChessPiece_.PieceImage))
                        {
                            if (chessPiece is King)
                            {
                                chessMoves.GenerateAvailableMoves(this);
                                chessMoves.GenerateAttackMoves(this);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR!!! {ex.Message}");
                System.Environment.Exit(0);
            }
        }

        #endregion

        #region -- Reset Method --
        /// <summary>
        /// Reset the current chess board to its initial state, clearing all pieces and resetting game state variables.
        /// </summary>
        public void Reset()
        {
            // clear game state variables
            this.ChessSquares_ = [];
            this.RemovedPlayerChessPieces = [];
            this.RemovedOpponentChessPieces = [];
            this.WhiteChessPieces = [];
            this.BlackChessPieces = [];

            // reset caslting rights
            WhiteCastleRightsQueenSide = true;
            BlackCastleRightsQueenSide = true;
            WhiteCastleRightsKingSide = true;
            BlackCastleRightsKingSide = true;

            // reset EnPassant attack square
            EnPassantTarget = new EnPassantHelper("-", 0, null, null, null, null);

            // draw a new board
            DrawBoard();
        }
        #endregion

        #region -- Copy Chess Board ---

        /// <summary>
        /// Method deep clones the board and all its ChessSquare entries (and their pieces).
        /// This is intended for simulation purposes so callers can test moves without
        /// mutating the live board shared with the UI.
        /// </summary>
        public Board DeepClone()
        {
            var clonedSquares = new ObservableCollection<ChessSquare>();

            if (this.ChessSquares_ != null)
            {
                foreach (var sq in this.ChessSquares_)
                {
                    clonedSquares.Add(sq.Clone());
                }
            }

            var clonedBoard = new Board(clonedSquares)
            {
                RemovedPlayerChessPieces = this.RemovedPlayerChessPieces != null
                    ? new ObservableCollection<string>(this.RemovedPlayerChessPieces)
                    : [],

                RemovedOpponentChessPieces = this.RemovedOpponentChessPieces != null
                    ? new ObservableCollection<string>(this.RemovedOpponentChessPieces)
                    : [],

                WhiteChessPieces = this.WhiteChessPieces != null 
                    ? [.. this.WhiteChessPieces]
                    : [],

                BlackChessPieces = this.BlackChessPieces != null
                    ? [.. this.BlackChessPieces]
                    : []
            };

            return clonedBoard;
        }

        /// <summary>
        /// Creates a deep copy of the current Board instance, including all ChessSquare objects and their associated ChessPiece objects.
        /// </summary>
        /// <returns> a new Board instance containing the deep copy </returns>
        public Board DeepCopy()
        {
            var squares = new ObservableCollection<ChessSquare>(
                ChessSquares_!.Select(square => square.Clone()));

            return new Board(squares);
        }
        #endregion
    }
}
