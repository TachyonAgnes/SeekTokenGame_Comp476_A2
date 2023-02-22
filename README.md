# Comp476_A1
# Game Freeze Tag
Game Freeze Tag is a simple multiplayer game controlled with the WASD keys, developed using the Unity engine. The game features two teams: Chaser and Evader. Players begin as members of the Evader team, and the game lasts for five minutes. If an Evader is touched by a Chaser, they become frozen. If the frozen Evader is not rescued by another player within 15 seconds, they become a Chaser. Tokens spawn periodically on the map, and only players and Chasers can touch them.

The game fails if any of the following conditions are met:

- An Evader becomes a Chaser.
- The Evader obtains less than half of the total tokens when the game ends.
The game is won if the Evader survives until the end of the game and wins.

## Controls
- WASD: Move the player up, down, left, and right.

## Rules
- Evaders become frozen when touched by Chasers.
- Frozen Evaders become Chasers after 15 seconds if they are not rescued.
- Only players and Chasers can touch Tokens.
- The game fails if:
  - An Evader becomes a Chaser.
  -The Evader obtains less than half of the total tokens when the game ends.
The game is won if the Evader survives until the end of the game and wins.

## Installation and Running
1. Clone the repository to your local machine.
2. Open the Unity engine and import the repository.
3. Run the game scene and start playing.

## Contributors
- Author: Zisen Ling
- Email: para.bola@qq.com
