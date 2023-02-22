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

## the characteristics of the behaviors I have defined

My AI movements were initially implemented as a direct method. However, considering the complexity of the work and the emphasis on separating displacement and rotation for tasks, I created an abstract class called AIMovement during refactoring, which includes flocking, fleeing, seeking, pursuing, obstacle avoidance, look where you're going, wandering, A* pathfinding, turn toward target, turn away from target, and flocking.

The turn away from target behavior has been abandoned and is no longer used. All AI movements in my AI agent class are implemented by calling the token class, token spawner class, pathfinding class, grid graph class, and AIMovement class. Game scene switching is achieved through the GameManager, and the game's victory condition is implemented by the GameAgent through the GameState.

All movements of my AI agent are completed in the getSteeringSum method, which calls two methods to obtain the steering output: MoveTowardsTarget() and MoveAwayFromTarget(). I fully replicated the requirements set by the instructor in R2 in MoveTowardsTarget(), but the character's movement behavior is not very satisfying due to the slow turning effect. Chasers don't have to worry about comically drifting and hitting walls during movement, but evaders don't have this luxury. Therefore, I simplified MoveAwayFromTarget() by removing the turn-on-spot part and using look where you're going instead of turn toward target. The turn away from target behavior is not very applicable in my code since most of my fleeing and chasing rely heavily on pathfinding, which means I have to track safe spots on the map to escape. If turn away from target is used, it may mean that my entire movement behavior needs to be refactored.

My Obstacle Avoidance is the code I've rewritten the most times. I've tried single and double raycasting, two short and one long, two long and one short, and calculated escape routes using the cross product of the obstacle's normal vector and the positivity/negativity of the y-axis. However, they all failed. The reason is that my map imitates the image shown in the instructor's assignment: narrow roads, long walls, and there are no traditional obstacles in the game, which means that the traditional obstacle avoidance approach is not the optimal solution here: I always collide with obstacles, which always slows me down. OverlapSphere always keeps a comfortable distance from the wall, so I chose to use it. I also used the Close Point technique here, where I give it maximum force as it gets closer to me.

Due to the heavy workload, I used the A* algorithm in the instructor's lab code. I must apologize for this. However, I've implemented the A* algorithm very well in Comp472, and if there are any doubts about my ability, I can show my AI assignments. Based on the lab code, I used the Chebyshev Distance Heuristic and defined the cost of diagonal movement as 1.4, considering diagonal movements. The switch between my A* algorithm and other displacement methods is basically determined by whether there are obstacles between the agent and the target and the distance between the two. When escaping, I calculate the angle between the agent-to-enemy vector and the agent-to-nearest-safe-point vector to determine the safest escape route.

In addition, my movement behavior is basically the same as the lab, but I rewrote it to ensure that I truly learned it. The submission time for the assignment was during the busiest time of this semester, and the size of the game and my available work time didn't quite match up, so the code was always undergoing intense changes, especially the obstacle avoidance. Therefore, I couldn't add comments to all of the code, but I did my best to do so.
