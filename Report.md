# Report



## What have you learned during this assignment?
- Setting up a game engine using Unity3D, primarily in 2D but with confidence to work with 3D.
- Using "component" design pattern.
    

## What has worked well and what’s not during the development?
### Worked well:
- Component system is awesome.
- Using prefabs and prefab variants is very helpful.
- Connection between Unity Editor and Visual Studio 2019 is smooth.
- Particle system is easy to use and it helps the game to appear more "alive".
- For every Unity related question there is an answer somewhere on a forum or a stackexchange.

### Did not work well:
- A Unity editor bug was frustrating, where a popup window would disappear when clicked. The popup window is not catching input click, which then falls through to the main window and that closes the popup. Solution: hold mouse click longer when opening the popup.
- Unity API changed a little over time, so old forum threads are not very relevant nowadays (solved by limiting google search to the recent years).
- Importing NuGet packages is difficult. In the end, it was not necessary for this assignment (I wanted to add a JSON library but then I found out Unity supports it already).
- Unity random number generator has a weird behavior (produces the same numbers during the same frame update for different component instances?).
- The official beginner Unity tutorials are not very helpful. Their focus seems to be more on marketing Unity features and their ease of use to novices, rather than going through basics systematically. I found it a lot faster and systematic to just read Unity documentation, or search for answers to the immediate questions that I had at the moment. The following tutorial is also very thorough and systematic, though I only used it for basics on sprites and tilemaps: https://www.raywenderlich.com/unity 


## What was the biggest obstacle you have encountered?
There were no big obstacles, just many smaller ones that were quickly solved by a short googling session.
Here I list some of them:
- Destructive interference of multiple audio sources. Solved by using multiple different audio clips and playing one randomly (gunshots, tank sounds) and by randomizing when the audio starts playing (adding a variability to gun reload times).
- Difficulty understanding quaternion and vector rotations.
- Destroy(component) should produce a warning that user probably means Destroy(component.gameObject), otherwise the silent error is difficult to debug for a novice user :)


## How satisfied are you with the result?
I am quite satisfied to use this as a demo for my portfolio. Of course, there are many things that could have been improved and I would very much liked to do so to learn more about Unity, but I don't have time at the moment. I will list some of my suggestions:
- Map is too regular visually. Tileset assets contain tiles that could make it look nicer.
- In retrospect, isometric 3D would look nicer and would solve issues with overlapping enemy sprites.
- Gameplay could be fine-tuned to make it more challenging but not too hard by doing a deeper analysis of cost of building towers and profit of killing enemies.
- More menu screens could be added: highscore table, help menu,...
- Enemy tracking could "lead" the target (taking it's movement speed and direction into account when shooting a projectile).


## What tool chain have you used?
- Unity Editor
- Visual Studio 2019
- Audacity
- Normally I would also use git versioning (using SmartGIT client) but this time I didn't.


## How automatized your tool chain is?
I don't see what exactly should be automatized here. If I were working on a bigger project with multiple people, I guess it would make sense to automate building and testing versions of the project.


## How extendable your solution is? How easy is to add another level, another enemy, another tower?

### Level
I would say this one is the easiest. The level is defined in an external text file which can be easily edited. I also added a helper code that stores the current tilemap for easy level design (though it does not store proper game settings).
It would be necessary to add and link a level button on level selection screen. For a higher number of levels the process would be improved to be automatic: 
- the buttons would be populated automatically based on what levels were detected in resources
- the level selection screen would be scrollable

### Enemy
Easy, just make a new prefab with an EnemyController and any of the 2 pathfinding controllers, and adjust their settings. In most of the places in the code, the enemies are handled by the enemy controller for each enemy type transparently. The main GameController would have to be adapted to allow more enemy types. Also, level difficulty settings would have to be extended because they define special settings for each enemy type.

### Tower
Again, each type of tower is controlled by the same TowerController. Tower mechanics is somewhat modular: its projectile can be defined, and each projectile has a secondary projectile (a rocket tower fires a rocket projectile that hits a target dealing direct damage and creates a secondary pressure wave projectile that deals smaller area-of-effect indirect damage). Projectile impact particles are also modular.
UI would have to be adapted to allow more tower placement buttons, and the main GameController would have to be adapted too.


## How much time it took you to create the game?
I was working on multiple parts at once, because I did not have a clear picture of how would the final result look like, so it was difficult to estimate time spent between programming the game engine, modifying assets, building the scene, designing UI and levels, and testing it along the way.
I focused on these things sequentially: 
1. tilemap definition of the scene
2. simple enemies moving in a straight line
3. dynamic map loading from a file
4. enemy movement on the map (pathfinding using breadth-first-search)
5. tower placement
6. tower tracking enemies
7. tower shooting projectiles
8. particle systems
9. game management (play, pause, game over, money, score)
10. adding menu screens
11. gameplay fine-tuning (enemy health, rewards, tower price, projectile damage)


## Are you confident you will be able to start working on another AS3 game for PC + Android now?
Definitely. I love the component system, it makes source code very organized. Seeing how easy it is, I am no longer surprised how many small games were released in the past few years. In my opinion, it would take more work to design game assets than to program an engine for a small game.
