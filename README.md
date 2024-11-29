
# ♟️ Chess AI ♟️

A modern web-based chess application featuring Online PvP (Player vs. Player), Local PvP, and PvAI (Player vs. AI) modes. This project is designed for flexibility, allowing dynamic integration of chess AI models as separate files. Built using **C#**, **JavaScript**, and **HTML** with **ASP.NET** and **SignalR** for seamless real-time interaction.

## Features

- **Local PvP Mode**: Play against another player on the same device or network.
- **PvAI Mode**: Challenge the AI and test your skills against dynamically loaded chess AI models.
- **Dynamic AI Integration**: Easily load and integrate custom AI models into the application.
- **Real-Time Gameplay**: Powered by SignalR, enabling smooth and responsive interactions.
- **Modern Tech Stack**: Built with ASP.NET and a combination of C# and JavaScript for efficient performance and scalability.

## Getting Started

### Prerequisites

- **ASP.NET Core SDK**: Ensure you have the latest version installed.
- A web browser to access the game.

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/S-14Chess-p/Senior-Project-ChessAI.git
   ```
2. Navigate to the project directory:
   ```bash
   cd Senior-Project-ChessAI/ChessAI
   ```
3. Build and run the project:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to:
   ```
   http://localhost:5178
   ```

### Adding Custom AI Models

1. Place your custom AI model file in the designated directory (e.g., `\ChessAI\Models\AIs`).
2. Ensure the model adheres to the required format and API for smooth integration.
3. Restart the application to load the new model.

## Technologies Used

- **C#**: For backend logic and AI model integration.
- **JavaScript/HTML**: For the front-end interface and gameplay rendering.
- **ASP.NET Core**: To build a scalable and maintainable web application.
- **SignalR**: For real-time updates and multiplayer functionality.
