using System.IO;
using System.Media;

namespace ChessGame.Game.chessSoundPlayer
{
    /// <summary>
    /// A static class that provides methods to play chess-related sounds.
    /// </summary>
    public static class ChessSoundPlayer
    {
        /// <summary>
        /// The directory where the chess sound files are located.
        /// </summary>
        private static readonly string SoundDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "chessSounds");

        /// <summary>
        /// Plays a chess sound file given its filename.
        /// </summary>
        /// <param name="fileName">The name of the sound file to play.</param>
        private static void Play(string fileName)
        {
            try
            {
                string filePath = Path.Combine(SoundDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Chess sound not found: {filePath}");

                    return;
                }

                // Use SoundPlayer to play the sound file
                using SoundPlayer player = new(filePath);
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Unable to play chess sound '{fileName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Plays the chess intro sound.
        /// </summary>
        public static void PlayIntro()
            => Play("chess_intro_sound.wav");

        /// <summary>
        /// Plays the sound for a chess piece move.
        /// </summary>
        public static void PlayMove()
            => Play("chess_piece_move.wav");

        /// <summary>
        /// Plays the sound for a chess piece capture.
        /// </summary>
        public static void PlayerCapture()
            => Play("chess_piece_capture.wav");

        /// <summary>
        /// Plays the sound for the start of a chess match.
        /// </summary>
        public static void PlayMatchStart()
            => Play("chess_match_start.wav");

        /// <summary>
        /// Plays the sound for a chess piece promotion.
        /// </summary>
        public static void PlayerNavSound()
            => Play("navigation_sound.wav");

        /// <summary>
        /// Plays the sound for a chess piece promotion.
        /// </summary>
        public static void PlayCheckmate()
            => Play("chess_checkmate.wav");
    }
}
