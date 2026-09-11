using ChessGame.Structs;
using ChessGame.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ChessGame.Views
{
    /// <summary>
    /// Interaction logic for ChessMatch.xaml
    /// </summary>
    public partial class ChessMatch : UserControl
    {
        private ChessMatchVM? vm { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChessMatch class.
        /// </summary>
        public ChessMatch()
        {
            InitializeComponent();
            this.Loaded += ChessMatchLoad;

        }

        /// <summary>
        /// Handles the Loaded event of the ChessMatch control. Sets up event handlers for piece movement animations.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The RoutedEventArgs instance containing the event data.</param>
        private void ChessMatchLoad(object sender, RoutedEventArgs e)
        {
            // Check if the DataContext is of type ChessMatchVM
            if (this.DataContext is ChessMatchVM vm)
            {
                if (vm.ChessGame != null)
                {
                    this.vm = vm;

                    // Subscribe to the OnPieceMoveAnimated and OnPiecesAnimateCasltingMove events for the player and Stockfish AI players
                    if (vm.ChessGame.Player is not null)
                    {
                        this.vm.ChessGame.Player.GameControls.MovementControls.OnPieceMoveAnimated += AnimatePieceMove;
                        this.vm.ChessGame.Player.GameControls.MovementControls.OnPiecesAnimateCasltingMove += AnimateCastlingMove;
                    }

                    // Subscribe to the events for each Stockfish AI player
                    foreach (var stockfishPlayer in this.vm.ChessGame.StockfishAIs ?? [])
                    {
                        Debug.WriteLine($"stock fish player is null: {stockfishPlayer is null}");
                        if (stockfishPlayer is not null)
                        {
                            stockfishPlayer.GameControls.MovementControls.OnPieceMoveAnimated += AnimatePieceMove;
                            stockfishPlayer.GameControls.MovementControls.OnPiecesAnimateCasltingMove += AnimateCastlingMove;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event for buttons that open popups. Opens the appropriate popup based on the button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The RoutedEventArgs instance containing the event data.</param>
        private void IsOpen_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;

            if (button != null && button.Name.Equals("GameRules"))
            {
                PopUpGameRules.IsOpen = true;
            }
            else
            {
                PopUpSubMenu.IsOpen = true;
            }

              
        }

        /// <summary>
        /// Handles the Click event for buttons that close popups. Closes the appropriate popup based on the button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The RoutedEventArgs instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;

            if (button != null && button.Name.Equals("CloseButton"))
            {
                PopUpGameRules.IsOpen = false;
            }
            else
            {
                PopUpSubMenu.IsOpen = false;
            }
        }

        /// <summary>
        /// Animates the movement of a chess piece from one square to another on the chessboard.
        /// </summary>
        /// <param name="PlacePiece">The action to place the chess piece on the destination square.</param>
        /// <param name="from">The starting location of the chess piece.</param>
        /// <param name="to">The destination location of the chess piece.</param>
        /// <param name="imageSource">The image source of the chess piece.</param>
        private async void AnimatePieceMove(Action PlacePiece, ChessSquareLocation from, ChessSquareLocation to, string imageSource)
        {
            // scaling factor
            double squareSize = 700.0 / 8;

            // image
            var image = new Image
            {
                Source = new BitmapImage(new Uri(imageSource, UriKind.Relative)),
                Width = 50,
                Height = 50
            };

            // calculate from and to positions
            double fromX = (from.X * squareSize);
            double fromY = (from.Y * squareSize);
            double toX = (to.X * squareSize);
            double toY = (to.Y * squareSize);

            // set initial position of the image
            Canvas.SetLeft(image, 0); // x
            Canvas.SetTop(image, 0); // y

            // transform image
            var transform = new TranslateTransform();
            image.RenderTransform = transform;

            // add image
            AnimationCanvas.Children.Add(image);

            // duration
            var duration = TimeSpan.FromMilliseconds(300);

            // start animation y value - vertical
            transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(
                OffSetValue(fromY, fromX, toY, toX, from.X, from.Y).fromY,
                OffSetValue(fromY, fromX, toY, toX, from.X, from.Y).toY,
                duration
                ));

            // animate x value - horizontal 
            transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(
                OffSetValue(fromY, fromX, toY, toX, from.X, from.Y).fromX,
                OffSetValue(fromY, fromX, toY, toX, from.X, from.Y).toX,
                duration
                ));

            await Task.Delay(duration);

            // remove animated image
            AnimationCanvas.Children.Remove(image);

            // place chess piece
            PlacePiece.Invoke();

        }

        /// <summary>
        /// Animates the castling move of the king and rook on the chessboard.
        /// </summary>
        /// <param name="PlaceKing">The action to place the king on the destination square.</param>
        /// <param name="PlaceRook">The action to place the rook on the destination square.</param>
        /// <param name="fromValues">The starting locations of the king and rook.</param>
        /// <param name="toValues">The destination locations of the king and rook.</param>
        /// <param name="images">The image sources of the king and rook.</param>
        public async void AnimateCastlingMove(Action PlaceKing, Action PlaceRook, (ChessSquareLocation from1, ChessSquareLocation from2) fromValues,
            (ChessSquareLocation to1, ChessSquareLocation to2) toValues, (string imageSourceKing, string imageSourceRook) images)
        {
            // Animate the king's move first
            AnimatePieceMove(PlaceKing, fromValues.from1, toValues.to1, images.imageSourceKing);

            await Task.Delay(100);

            // Animate the rook's move after a short delay
            AnimatePieceMove(PlaceRook, fromValues.from2, toValues.to2, images.imageSourceRook);
        }

        /// <summary>
        /// Calculates the offset values for the animation based on the starting and ending positions of the chess piece.
        /// </summary>
        /// <param name="fromY">The starting Y position of the chess piece.</param>
        /// <param name="fromX">The starting X position of the chess piece.</param>
        /// <param name="toY">The ending Y position of the chess piece.</param>
        /// <param name="toX">The ending X position of the chess piece.</param>
        /// <param name="x">The X coordinate of the chess piece on the board.</param>
        /// <param name="y">The Y coordinate of the chess piece on the board.</param>
        /// <returns>The offset values for the animation.</returns>
        private (double fromY, double fromX, double toY, double toX) OffSetValue(
            double fromY, double fromX, double toY, double toX, int x, int y)
        {
            // Initialize the offset values
            (double fromY, double fromX, double toY, double toX) values = (fromY, fromX, toY, toX);

            // Adjust the offset values based on the Y coordinate of the chess piece
            if (x >= 0 && x < 8 && y == 0)
            {
                fromY += 27;
                fromX += 10;

                toY += 27;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 1)
            {
                fromY += 25;
                fromX += 10;

                toY += 25;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 2)
            {
                fromY += 23;
                fromX += 10;

                toY += 23;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 3)
            {
                fromY += 20;
                fromX += 10;

                toY += 20;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 4)
            {
                fromY += 17;
                fromX += 10;

                toY += 17;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 5 || y == 6)
            {
                fromY += 15;
                fromX += 10;

                toY += 15;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }
            else if (x >= 0 && x < 8 && y == 7)
            {
                fromY += 10;
                fromX += 10;

                toY += 10;
                toX += 10;

                values = (fromY, fromX, toY, toX);
            }

            return values;
        }
    }
}

