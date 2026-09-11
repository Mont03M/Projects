# ChessGame

A C# WPF chess application built using the **MVVM architecture**, with a focus on clean separation of game state, move generation, legality validation, UI presentation, asynchronous game processing, and AI strategy integration. This repository is primarily a full-featured chess application that contains a complete rules engine. It implements per-piece pseudo-legal move generation, legality filtering, special moves (castling, en passant, promotion), and game-state detection, so it can fully enforce chess rules and produce legal moves. In short this repository is a playable chess application with an internal rules/move generator and full game-state handling, suitable for UI play and engine integration.

## Project Overview

The project is organized into several core components that separate the chess game logic from the user interface and supporting services:<br>

- **Board, ChessGame and GameMatchVM** – Manages the chess board, squares, piece locations, and overall game state.<br>
- **Move Generation and Validation** – Generates piece-specific moves and validates them against chess rules and king safety.<br>
- **Special Moves** – Handles castling, en passant, and pawn promotion.<br>
- **MVVM UI Architecture** – Separates application logic from WPF views through ViewModels, services, and commands.<br>
- **User Interface** – Provides dedicated interfaces for the main application, game configuration, settings, and chess gameplay.<br>
- **Asynchronous Game Processing** – Uses asynchronous programming and background tasks to process game operations and monitor game state.<br>
- **AI and Strategy Integration** – Supports computer-controlled play through custom game logic and Stockfish integration.<br>
- **Game State and Draw Detection** – Tracks check, checkmate, stalemate, and other draw conditions.<br>

## User Interface

The application provides several dedicated user interfaces that separate game configuration, application settings, and gameplay.<br>

### Main UI

The **Main UI** serves as the primary interface for the application and manages navigation between the different sections of the chess application.<br>

It provides access to game setup, application settings, and the chess match interface through the MVVM navigation architecture.<br>

### Settings UI

The **Settings UI** allows players to customize the appearance and behavior of the chess application.<br>

Available settings include:<br>

- Board configuration<br>
- Chess board appearance and customization<br>
- Chess piece selection<br>
- Selection between available chess piece sets<br>
- Enable or disable chess gameplay sound effects<br>

These settings are managed through the application's settings services and can be applied independently from the core chess game logic.<br>

### Game Options UI

The **Game Options UI** allows players to configure the type and difficulty of a chess match before starting a game.<br>

Supported game modes include:<br>

- **Player vs. AI** – A human player competes against the computer.<br>
- **AI vs. AI** – Two computer-controlled players compete against each other.<br>

The application also provides multiple AI difficulty levels:<br>

- **Easy**<br>
- **Medium**<br>
- **Hard**<br>

Players can also enable **Lightning Chess**, which introduces time controls and requires players to complete their moves within the configured time limits.<br>

- **30-seconds per move player clock**<br>
- **30-minute game clock**<br>

### Chess Match UI

The **Chess Match UI** is the primary gameplay interface where players interact with the chess board.<br>

This interface is responsible for presenting:<br>

- The chess board and pieces<br>
- Player moves<br>
- Available and legal moves<br>
- Turn information<br>
- Check and checkmate states<br>
- Special moves<br>
- Pawn promotion<br>
- Game status<br>
- Move history and sequencing<br>

The Chess Match UI communicates with the underlying ViewModels and game services, allowing the user interface to remain separated from the core chess rules and game-processing logic.<br>

## Asynchronous Game Processing

The game uses **two asynchronous tasks that are scheduled on the .NET ThreadPool** to separate continuous game processing from game-state monitoring.<br>

The asynchronous architecture allows the application to monitor and process chess operations without blocking the WPF UI thread. The background tasks coordinate game execution and continuously evaluate the current board state.

These asynchronous processes are responsible for operations such as:<br>

- Generating available and legal moves<br>
- Monitoring **check**<br>
- Detecting **checkmate**<br>
- Monitoring **pawn promotion**<br>
- Monitoring and Processing **castling**<br>
- Monitoring and Processing **en passant**<br>
- Monitoring game-state changes<br>
- Managing the main game loop and turn processing<br>

Cancellation tokens and asynchronous delays are used to control the lifetime of background operations and prevent unnecessary processing when the game is paused, ended, or reset.<br>

This approach allows continuously running game operations to execute independently of the UI, keeping the WPF interface responsive while the chess engine processes the current game state.<br>

## Board and Piece Architecture

Board state and fundamental chess structures are contained within the board-related components:<br>

- `Board.cs`<br>
- `ChessSquare.cs`<br>
- `ChessSquareLocation.cs`<br>

Each chess piece contains its own movement logic, allowing movement rules to remain encapsulated within the corresponding piece implementation:<br>

- `ChessPiece.cs`<br>
- `Bishop.cs`<br>
- `King.cs`<br>
- `Knight.cs`<br>
- `Pawn.cs`<br>
- `Queen.cs`<br>
- `Rook.cs`<br>

This structure makes it easier to maintain and extend individual piece movement rules without tightly coupling them to the rest of the game.<br>

## Move Generation and Legality

Move generation is implemented through the `chessGameMoves` components, including:<br>

- `MovesAvailable.cs`<br>
- `Peek.cs`<br>

The application uses a **two-stage move-validation process**:<br>

1. **Pseudo-legal move generation** – Each piece generates moves according to its normal movement rules.<br>
2. **Legal move validation** – Candidate moves are evaluated against the current board state to ensure the player's king is not left in check.<br>

This approach allows the application to distinguish between moves that are valid based solely on piece movement and moves that are actually legal under the rules of chess.<br>

Move generation and legality checks are processed as part of the game's asynchronous workflow, allowing the application to continuously evaluate the current position without blocking the UI.<br>

## King Safety, Pins, and Check Detection

King safety is handled through dedicated check and move-validation helpers, including:<br>

- `CheckMoveHelper.cs`<br>
- `ChessMovesExtensions.cs`<br>
- `IsCheckControls.cs`<br>

The application analyzes attack paths and simulates candidate board states to identify:<br>

- Pieces pinned to the king<br>
- Direct attacks against the king<br>
- Discovered attacks<br>
- Legal responses to check<br>
- Moves that would expose the king to attack<br>

Attack-ray scanning is used to evaluate sliding-piece threats, while move simulation allows the application to determine whether a candidate move results in an illegal king position.<br>

Check detection is continuously monitored by the asynchronous game-processing system.<br>

## Checkmate, Stalemate, and Draw Detection

Checkmate and stalemate are determined by evaluating the legal moves available to the side whose turn it is.<br>

- **Checkmate** occurs when the king is under attack and no legal moves are available.<br>
- **Stalemate** occurs when the king is not under attack but the player has no legal moves (Draw).<br>

Draws are determined by evaluating the game state, pieces on the board, and number of occurrences of each chess board state using fen notion.<br>

- **Insufficient Material** occurs if neither player has enough pieces to checkmate.<br>
- **100-Move Rule** occurs if 100-half-moves have occurred without a pawn move or capture.<br>
- **Five Fold Repetition** occurs if the same position has occurred five times.<br>

Game-state and draw information are represented through:<br>

- `GameState.cs`<br>
- `DrawReason.cs`<br>

The asynchronous game monitoring system evaluates these conditions as the game progresses, allowing check, checkmate, stalemate, and other game-ending conditions to be detected during gameplay.<br>

## Special Moves

Special chess rules are separated into dedicated handlers:<br>

- `Castling.cs`<br>
- `EnPassant.cs`<br>
- `PawnPromation.cs`<br>
- `SpecialMoveHandler.cs`<br>

The game-processing system monitors the conditions required for these special moves and coordinates their processing with the current game state.<br>

### Pawn Promotion

Pawn promotion is monitored when a pawn reaches its promotion rank. The game can pause normal turn processing while the player selects a new piece before continuing gameplay.<br>

### Castling

Castling logic evaluates the king and rook state, castling rights, clear movement paths, and whether the king or required squares are under attack before allowing the move.<br>

### En Passant

En passant is handled through dedicated state tracking and validation to ensure the move is only available when the required preceding pawn move and board conditions are satisfied.<br>

## MVVM and Application Architecture

The WPF application follows the **Model-View-ViewModel (MVVM)** pattern.<br>

The UI is separated into:<br>

- `Model/` - A shared domain model that implements a navigation service.<br>
- `Views/` – WPF user interface components<br>
- `ViewModels/` – Application and presentation logic<br>
- `services/` – Shared application services such as settings and navigation<br>

The primary interfaces include the **Main UI**, **Settings UI**, **Game Options UI**, and **Chess Match UI**, each supported by corresponding ViewModels and application services.<br>

A move queue structure is also used to manage move information and sequencing:<br>

- `MovesQueue/`<br>
- `ChessMoveInfo`<br>

This allows moves to be organized and processed independently from the visual presentation of the board.<br>

## AI and Stockfish Integration

The project includes support for both custom strategy development and **Stockfish** engine integration.<br>

Stockfish-related components include:<br>

- `strategies/stockfish`<br>
- `chessPlayers/StockfishAI.cs`<br>
- `strategies/tools/stockfishTools/BoardToFenConverter.cs`<br>
- `strategies/tools/stockfishTools/StockChessMoveInfo.cs`<br>

The board state can be converted to **FEN notation** for communication with the Stockfish engine. Engine responses are then converted back into chess move information that can be processed by the application.<br>

This provides the foundation for computer-controlled gameplay and allows the project to integrate different AI strategies with an established chess engine.<br>

## Additional Features

The application also includes supporting resources for a complete playable chess experience, including:<br>

- Customizable chess boards<br>
- Multiple chess piece sets<br>
- Enable or disable gameplay sound effects<br>
- Player vs. AI gameplay<br>
- AI vs. AI gameplay<br>
- Multiple AI difficulty levels<br>
- Lightning Chess with time controls<br>
- Check, checkmate, stalemate, and draw detection<br>
- Castling, en passant, and pawn promotion<br>
- Stockfish engine integration<br>
- Move history and sequencing<br>
- Application settings<br>
- UI themes and styling<br>
- Development and testing functionality<br>

## Architecture Summary

The architecture is designed to keep **the WPF user interface, chess rules, game-state management, asynchronous processing, special-move handling, and AI logic separated from one another**. This separation makes the application easier to maintain, test, debug, and extend with additional features, game modes, AI strategies, and user interface improvements.<br>

## Stockfish

This application uses the Stockfish chess engine, which is licensed under the GNU General Public License version 3 (GPLv3).<br>

The Stockfish license is included in strategies/stockfish/Copying.txt, and the corresponding Stockfish source code is provided in the strategies/stockfish/src directory.<br>

Stockfish Copyright © the Stockfish developers.<br>
