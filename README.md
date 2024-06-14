# BrickBreaker

`BrickBreaker` is a classic arcade game implemented in C# using WPF (Windows Presentation Foundation). The game involves breaking bricks using a ball that bounces off a paddle controlled by the player.

## Features

- **Arrow**: A visual indicator that rotates to set the initial direction of the ball.
- **Ball**: A moving object that bounces off bricks, the paddle, and the walls.
- **Brick**: Blocks that can be destroyed by the ball. Some bricks are undestroyable.
- **Paddle**: The player's control to bounce the ball.
- **Game Management**: Controls the game state, including starting, running, and ending the game.

## Classes

### Arrow
- Represents the initial aiming mechanism before the game starts.
- Rotates back and forth to let the player choose the initial angle for the ball.

### Ball
- Represents the ball in the game that interacts with bricks and the paddle.
- Handles movement and collision detection.

### Brick
- Represents a destructible or undestroyable block in the game.
- Changes color based on its hit points (HP).

### UndestroyableBrick
- A subclass of `Brick` that cannot be destroyed.

### Game
- Manages the overall game state and logic.
- Handles user inputs, updates the game state, and renders objects on the game canvas.

## Enum Types

### Direction
- Defines directions for movement and collision detection (`Top`, `Left`, `Bottom`, `Right`, `None`).

### GameState
- Defines the states of the game (`Playing`, `Lost`, `Won`).

## Getting Started

### Prerequisites

- .NET Framework
- Visual Studio or any other C# development environment

### Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/vokurkaa/BrickBreaker.git
    ```
2. Open the solution file in Visual Studio.

### Running the Game

1. Build the solution in Visual Studio.
2. Run the project.

### Controls

- Use the arrow keys to move the paddle left and right.
- Press any key to start the game and launch the ball.

## Game Mechanics

- **Starting**: The game begins by rotating an arrow. Press any key to launch the ball in the direction of the arrow.
- **Playing**: Control the paddle using the left and right arrow keys to keep the ball in play.
- **Winning**: Break all destroyable bricks to win the game.
- **Losing**: Let the ball fall off the bottom of the screen to lose the game.

## AI Assistance

As a newcomer to WPF, I utilized AI tools to aid in the development of this project. These tools helped me to:
- Understand and implement WPF concepts and practices that were unfamiliar to me.
- Learn and apply mathematical formulas necessary for game mechanics.
- Summarize and utilize vector operations in the `Vector2D` class.
- Draft this README file.
