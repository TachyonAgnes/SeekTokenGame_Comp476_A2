# Comp476_A2
# Game SeekTokenGame

This game is based on the implementation of Comp476_A1, which was refactored using behavior trees to improve the code structure, and implemented a series of advanced AI behaviors, tactical decision-making, influence maps, and tactical locations.

[Click Here To Check Screen Shots](screenShots/PDF.pdf)

# Winning conditions
The game fails if any of the following conditions are met:

- All Seekers died
- times up and the Seeker team obtains less than 10 tokens when the game ends.

## Controls
- WASD: Move the player up, down, left, and right.
- Shift: Zoom in/out
- G: throw a blizzard bomb

## Installation and Running
1. Clone the repository to your local machine.
2. Open the Unity engine(2021.3.17f1) and import the repository.
3. Run the game scene and start playing.

## Contributors
- Author: Zisen Ling
- Email: para.bola@qq.com

## conclusion
This assignment is based on Comp476_A1, and I spent a lot of time refactoring the code using behavior trees. Due to the implementation of Nodes not inheriting MonoBehaviour, it was not practical to simply make AIMovement a Node. Also, due to the presence of behavior trees, most of the code in AIAgent had to be removed, including the long if statements I wrote previously. And...Most of the AIMovement I wrote previously was very basic, implementing it as a Node would have made the tree very bloated. Therefore, I took a shortcut and wrote a MovementNode to help pass the agent and apply AIMovement.

In terms of behavior trees, I was still adding content in the early stages of the project, but later on, due to limited time and space, I wrote some spaghetti code on some nodes. But overall, I deeply understand the importance of behavior trees and will use them again in the future.

As for the implementation part, I basically applied the techniques from A1 repeatedly. I also spent some time on post-processing effects and cinemachine as additional learning materials. The challenging parts were the influence map and tactical locations. These two parts were not difficult to write, but debugging took me two days. My tactical locations were written first. So basically it get the list of all tactical locations on the map, then remove the ones outside of a certain range, ignore thoses near the NPC, and finally remove the ones that are visible to the NPC, so that the remaining tactical locations would not intersect with the chaser. This sounds like a good idea, but after implementation, the effect was not satisfactory because this strategy is based on pathfinding, and the agent may suddenly turn around and walk back along the "shortest path", directly running into the chaser. So i implemented the influence map and applied it to pathfinding, which produced good results. However, due to the difficulty of debugging and too many parameters, the influence map really took me a lot of time.
