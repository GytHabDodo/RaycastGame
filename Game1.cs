using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.VisualBasic;
class Game1
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);
    static void Main()
    {
        Console.Clear();
        int frameCount = 0;
        int fps = 0;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        long lastTime = stopwatch.ElapsedMilliseconds;
        string[] map = {
            "####################",
            "#          #       #",
            "#     ##      #    #",
            "#                  #",
            "####        ####   #",
            "#                  #",
            "####################"
        };
        char[,] projectileSprite = new char[,]
        {
            { ',', ' ', '|', ' ', ',' },
            { ' ', '\\', 'H', '/', ' ' },
            { '-', 'G', '@', 'G', '-' },
            { ' ', '/', 'H', '\\', ' ' },
            { ',', ' ', '|', ' ', ',' },
        };
        char[,] explodeProjectileSprite = new char[,]
        {
            { '\\', ' ', '#', '#', '/' },
            { '#', '#', '#', '#', '#' },
            { '-', 'G', '@', 'G', '-' },
            { '#', '#', '#', '#', '#' },
            { '/', '#', '#', ' ', '\\' },
        };
        char[,] enemySprite = new char[,]
        {
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|', ' ', ' ', ' ' },
            { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
            { ' ', ' ', ' ', '@', ' ', ' ', ' ', ' ', 'T', '\\', ' ', ' ' },
            { ' ', 'T', 'T', 'A', 'R', ' ', ' ', '.', ' ', 'T', ' ', ' ' },
            { 'A', ' ', 'E', 'T', 'E', 'G', ',', ' ', 'T', 'T', ' ', ' ' },
            { '/', ' ', 'A', 'R', 'F', ' ', ' ', ' ', '/', 'T', ' ', ' ' },
            { '|', ' ', 'A', 'N', 'C', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
            { ' ', ' ', 'G', ' ', 'G', ' ', ' ', ' ', 'T', 'T', ' ', ' ' },
            { ' ', 'V', ',', ' ', 'T', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
            { ' ', 'C', ' ', ' ', 'Y', ' ', ' ', ' ', ';', '|', ' ', ' ' }
        };
        bool isEnemyMoving = false;
        double playerX = 4.5;
        double PI = Math.PI;
        double playerY = 1.5;
        double playerAngle = PI / 2; // 90*, turned down
        float rotationSpeed = 0.04f;
        float speed = 0.6f;
        int screenWidth = 310;
        int screenHeight = 90;
        double FOV = PI / 3; // 60 degrees in radians
        int times_check_temp = 0;
        double portal1X = 9.5;
        double portal1Y = 1.5;
        double portal2X = 16.5;
        double portal2Y = 3.5;
        bool wasPortalUsed = false;
        string moveDirection = "N";
        int portalLeft = -1;
        double portalHeight = -1;
        double portalRight = -1;
        bool wasPortalRayUsed = false;
        var temp = false;
        bool cursor = true;
        int enemyAnimationStep = 1;
        bool enemyMove = false;
        Console.CursorVisible = false;

        bool miniMap = false;
        int miniMapScale = 1;
        char[,] frameBuffer = new char[screenHeight, screenWidth];
        bool isEnemyVisible = false;
        List<Projectile> projectiles = new();
        Random random = new();
        int enemytestX = random.Next(0, map[0].Length);
        int enemytestY = random.Next(0, map.Length);
        if (map[enemytestY][enemytestX] == '#')
        {
            while (map[enemytestY][enemytestX] == '#')
            {
                enemytestX = random.Next(0, map[0].Length);
                enemytestY = random.Next(0, map.Length);
            }
        }
        Enemy enemy1 = new();
        {
            enemy1.EnemyX = enemytestX + 0.5;
            enemy1.EnemyY = enemytestY + 0.5;
            enemy1.Health = 5;
            enemy1.IsAlive = true;
        }

        // Main game loop
        while (true)
        {
            if (enemy1.IsAlive)
            {
                if (!isEnemyMoving) enemyAnimationStep = 0;
                if (isEnemyMoving)
                {
                    if (enemyAnimationStep > 30) enemyAnimationStep = 1;
                    enemyAnimationStep++;
                    if (enemyAnimationStep == 2)
                    {
                        enemySprite = new char[,]
                        {
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'T', '\\', ' ', ' ' },
                        { ' ', ' ', ' ', '@', ' ', ' ', ' ', ' ', ' ', 'T', ' ', ' ' },
                        { ' ', 'T', 'T', 'A', 'R', ' ', ' ', ',', 'T', 'T', ' ', ' ' },
                        { 'A', ' ', 'E', 'T', 'E', 'G', ',', ' ', '/', 'T', ' ', ' ' },
                        { '/', ' ', 'A', 'R', 'F', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                        { '|', ' ', 'A', 'N', 'C', ' ', ' ', ' ', 'T', 'T', ' ', ' ' },
                        { ' ', ' ', 'G', ' ', 'G', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                        { ' ', ' ', 'V', ' ', 'T', ' ', ' ', ' ', ';', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', ' ', 'Y', ' ', ' ', ' ', ' ', ' ', ' ', ' ' }
                        };
                    }
                    else if (enemyAnimationStep == 16)
                    {
                        enemySprite = new char[,]
                        {
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                        { ' ', ' ', ' ', '@', ' ', ' ', ' ', ' ', 'T', '\\', ' ', ' ' },
                        { ' ', 'T', 'T', 'A', 'R', ' ', ' ', '.', ' ', 'T', ' ', ' ' },
                        { 'A', ' ', 'E', 'T', 'E', 'G', ',', ' ', 'T', 'T', ' ', ' ' },
                        { '/', ' ', 'A', 'R', 'F', ' ', ' ', ' ', '/', 'T', ' ', ' ' },
                        { '|', ' ', 'A', 'N', 'C', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                        { ' ', ' ', 'G', ' ', 'G', ' ', ' ', ' ', 'T', 'T', ' ', ' ' },
                        { ' ', ' ', 'V', ' ', 'T', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                        { ' ', ' ', 'C', ' ', ' ', ' ', ' ', ' ', ';', '|', ' ', ' ' }
                        };
                    }
                }
                else if (enemyAnimationStep == 0)
                {
                    enemyAnimationStep = 1;
                    enemySprite = new char[,]
                    {
                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|', ' ', ' ', ' ' },
                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                    { ' ', ' ', ' ', '@', ' ', ' ', ' ', ' ', 'T', '\\', ' ', ' ' },
                    { ' ', 'T', 'T', 'A', 'R', ' ', ' ', '.', ' ', 'T', ' ', ' ' },
                    { 'A', ' ', 'E', 'T', 'E', 'G', ',', ' ', 'T', 'T', ' ', ' ' },
                    { '/', ' ', 'A', 'R', 'F', ' ', ' ', ' ', '/', 'T', ' ', ' ' },
                    { '|', ' ', 'A', 'N', 'C', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                    { ' ', ' ', 'G', ' ', 'G', ' ', ' ', ' ', 'T', 'T', ' ', ' ' },
                    { ' ', 'V', ',', ' ', 'T', ' ', ' ', ' ', 'T', ' ', ' ', ' ' },
                    { ' ', 'C', ' ', ' ', 'Y', ' ', ' ', ' ', ';', '|', ' ', ' ' }
                    };
                }
            }
            else
            {
                if (temp == true) enemyAnimationStep = 0;
                temp = false;
                enemyAnimationStep++;
                enemySprite = new char[,]
                {
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '.', ' ' },
                { ' ', ' ', ' ', ' ', '#', ' ', ' ', ' ', '#', ' ', ' ', ' ' },
                { ' ', '\\', ' ', ' ', ' ', ' ', '/', ' ', ',', ' ', ' ', ' ' },
                { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '#', '#', ' ', ' ' },
                { ' ', ',', '#', ',', '#', ' ', ' ', '#', ' ', '.', ' ', ' ' },
                { '#', ' ', ' ', '#', '#', ' ', '#', ' ', ' ', ' ', ' ', ' ' },
                { ',', '#', ' ', ',', '#', ' ', ' ', ' ', '#', '#', '/', ' ' },
                { ' ', ' ', ' ', ' ', '#', ' ', '/', ' ', ',', ' ', ' ', ' ' },
                { ' ', ' ', '#', ' ', ',', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                { ' ', '#', ',', ' ', ' ', ' ', ' ', '/', '#', ' ', ' ', ' ' },
                { ' ', ',', ' ', ' ', '#', ' ', '#', ' ', ' ', '#', ' ', ' ' }
                };
                if (enemyAnimationStep >= 45)
                {
                    enemytestX = random.Next(0, map[0].Length);
                    enemytestY = random.Next(0, map.Length);
                    while (map[enemytestY][enemytestX] == '#' || Math.Sqrt((enemytestX + 0.5 - playerX) * (enemytestX + 0.5 - playerX) + (enemytestY + 0.5 - playerY) * (enemytestY + 0.5 - playerY)) < 4)
                    {
                        enemytestX = random.Next(0, map[0].Length);
                        enemytestY = random.Next(0, map.Length);
                    }
                    enemy1.EnemyX = enemytestX + 0.5;
                    enemy1.EnemyY = enemytestY + 0.5;
                    enemy1.IsAlive = true;
                    enemy1.Health = 5;
                }
            }
            if (times_check_temp < 20)
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.SetBufferSize(Math.Max(screenWidth, Console.LargestWindowWidth),
                        Math.Max(screenHeight + 1, Console.LargestWindowHeight));
                }
                Console.SetWindowSize(Math.Min(screenWidth, Console.LargestWindowWidth),
                    Math.Min(screenHeight + 1, Console.LargestWindowHeight));
                //times_check_temp++;
            }
            double[] distanceFromWall = new double[screenWidth]; //for each x on raycasting


            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    frameBuffer[y, x] = ' ';
                }
            }

            // Cast rays and render the scene
            double angleStep = FOV / screenWidth;
            int rayCount = 0;
            char[,] miniMapBuffer = new char[map.Length * miniMapScale, map[0].Length * miniMapScale];
            for (int y = 0; y < map.Length; y++)
                for (int x = 0; x < map[y].Length; x++)
                    for (int cy = 0; cy < miniMapScale; cy++)
                        for (int cx = 0; cx < miniMapScale; cx++)
                            miniMapBuffer[y * miniMapScale + cy, x * miniMapScale + cx] = map[y][x];
            for (int y = 0; y < miniMapBuffer.GetLength(0); y++)
            {
                for (int x = 0; x < miniMapBuffer.GetLength(1); x++)
                {
                    {
                        if ((int)(playerY * miniMapScale) == y && (int)(playerX * miniMapScale) == x) miniMapBuffer[y, x] = 'P';
                        if ((int)(portal1Y * miniMapScale) == y && (int)(portal1X * miniMapScale) == x) miniMapBuffer[y, x] = 'O';
                        if ((int)(portal2Y * miniMapScale) == y && (int)(portal2X * miniMapScale) == x) miniMapBuffer[y, x] = 'O';
                        if ((int)(enemy1.EnemyY * miniMapScale) == y && (int)(enemy1.EnemyX * miniMapScale) == x) miniMapBuffer[y, x] = 'E';
                    }
                }
            }
            isEnemyVisible = false;
            portalHeight = -1;
            portalLeft = -1;
            portalRight = -1;
            for (int x = 0; x < screenWidth; x++)
            {
                rayCount++;
                double rayAngle = playerAngle - FOV / 2 + x * angleStep;
                bool CollisionWithWall = false;
                double rayLength = 0.000001;
                double rayDirectionX = Math.Cos(rayAngle);
                double rayDirectionY = Math.Sin(rayAngle);
                double rayX = playerX;
                double rayY = playerY;
                wasPortalRayUsed = false;

                while (!CollisionWithWall && rayLength < 15)
                {
                    rayLength += 0.05;
                    rayX += 0.05 * rayDirectionX;
                    rayY += 0.05 * rayDirectionY;
                    bool isRayInPortal1 = Math.Sqrt((rayX - portal1X) * (rayX - portal1X) + (rayY - portal1Y) * (rayY - portal1Y)) <= 0.1;
                    bool isRayInPortal2 = Math.Sqrt((rayX - portal2X) * (rayX - portal2X) + (rayY - portal2Y) * (rayY - portal2Y)) <= 0.1;
                    if (isRayInPortal1 || isRayInPortal2) if (rayCount - 1 < portalLeft || portalLeft == -1) portalLeft = rayCount - 1;
                    if (isRayInPortal1 || isRayInPortal2) if (screenHeight / rayLength < portalHeight || portalHeight == -1) portalHeight = screenHeight / rayLength;
                    if (isRayInPortal1 || isRayInPortal2) if (rayCount > portalRight || portalRight == -1) portalRight = rayCount;

                    if (moveDirection == "Forward")
                    {
                        if (!wasPortalUsed && !wasPortalRayUsed && isRayInPortal1)
                        {
                            wasPortalRayUsed = true;
                            rayX = portal2X;
                            rayY = portal2Y;
                        }
                        if (!wasPortalUsed && !wasPortalRayUsed && isRayInPortal2)
                        {
                            wasPortalRayUsed = true;
                            rayX = portal1X;
                            rayY = portal1Y;
                        }
                    }
                    else
                    {
                        if (!wasPortalRayUsed && isRayInPortal1)
                        {
                            wasPortalRayUsed = true;
                            rayX = portal2X;
                            rayY = portal2Y;
                        }
                        if (!wasPortalRayUsed && isRayInPortal2)
                        {
                            wasPortalRayUsed = true;
                            rayX = portal1X;
                            rayY = portal1Y;
                        }
                    }

                    //if (Math.Sqrt((rayX - 3)*(rayX - 3) + (rayY - 3)*(rayY - 3)) < 0.4) break;
                    // simple cylinder (circle) shape on the map at (3,3) and with radius 0.4

                    int testX = (int)Math.Floor(rayX);
                    int testY = (int)Math.Floor(rayY);

                    int mmaptestY = (int)(miniMapScale * rayY);
                    int mmaptestX = (int)(miniMapScale * rayX);

                    if (testX < 0 || testX >= map[0].Length || testY < 0 || testY >= map.Length)
                    {
                        break;
                    }
                    if (mmaptestX >= 0 && mmaptestX < miniMapBuffer.GetLength(1) && mmaptestY >= 0 && mmaptestY < miniMapBuffer.GetLength(0))
                    {
                        if (miniMapBuffer[mmaptestY, mmaptestX] == ' ')
                        {
                            miniMapBuffer[mmaptestY, mmaptestX] = '.'; // rays
                            //for (int i = -5; i < 6; i++) if (rayCount+i == screenWidth / 2) miniMapBuffer[mmaptestY, mmaptestX] = '='; // center view ray
                        }
                    }

                    if (map[testY][testX] == '#')
                    {
                        CollisionWithWall = true;
                        while (rayLength > 0 && map[testY][testX] == '#')
                        {
                            rayLength -= 0.0001;
                            rayX -= 0.0001 * rayDirectionX;
                            rayY -= 0.0001 * rayDirectionY;
                            testX = (int)Math.Floor(rayX);
                            testY = (int)Math.Floor(rayY);
                        }
                        rayLength += 0.0001;
                    }
                }
                // Calculate wall height
                rayLength = (float)Math.Cos(playerAngle - rayAngle) * rayLength; // Correct for fish-eye effect
                distanceFromWall[x] = rayLength;
                int wallHeight = (int)(screenHeight / rayLength);
                int ceiling = (screenHeight - wallHeight) / 2;
                int floor = screenHeight - ceiling + 2;

                // Fill the buffer for this column
                int portalCeiling = (screenHeight - (int)portalHeight) / 2;
                int portalFloor = portalCeiling + (int)portalHeight;
                for (int y = 0; y < screenHeight; y++)
                {

                    if (y < ceiling)
                    {
                        frameBuffer[y, x] = ' '; // Ceiling
                    }
                    else if (y >= ceiling && y < floor)
                    {
                        frameBuffer[y, x] = GetWallShade(rayLength, ceiling, floor, y); // Wall
                    }
                    else
                    {
                        frameBuffer[y, x] = '.';
                    }
                    if (y >= portalCeiling && y < portalFloor && x == portalRight && portalHeight != -1 && portalLeft != -1 && portalRight != -1)
                    {
                        frameBuffer[y, x] = 'I';
                    }
                    if (y >= portalCeiling && y < portalFloor && x == portalLeft && portalHeight != -1 && portalLeft != -1 && portalRight != -1)
                    {
                        frameBuffer[y, x] = 'I';
                    }

                }
            }
            double diffX = enemy1.EnemyX - playerX;
            double diffY = enemy1.EnemyY - playerY;
            double angleToEnemy = Math.Atan2(diffY, diffX);
            double angleDiff = angleToEnemy - playerAngle;
            double distanceToEnemy = Math.Sqrt(diffX * diffX + diffY * diffY);
            while (angleDiff < -PI) angleDiff += 2 * PI; // keep the same value but beetween -180 and 180
            while (angleDiff > PI) angleDiff -= 2 * PI;
            int enemyHeight = (int)Math.Round(screenHeight / distanceToEnemy);
            int enemyWidth = enemyHeight;

            int enemyColumn = (int)((angleDiff + FOV / 2) / FOV * screenWidth); //convert into (int)(percentage of screen width) as 0 to 1
            int enemyTop = (screenHeight - enemyHeight) / 2;
            int enemyLeft = enemyColumn - enemyWidth / 2;
            int enemyRight = enemyLeft + enemyWidth;
            if (enemyRight > 0 && enemyLeft < screenWidth) //if (Math.Abs(angleDiff) < FOV / 2) is the same but using angles
            {
                int enemyStartX = Math.Max(enemyLeft, 0);
                int enemyEndX = Math.Min(enemyLeft + enemyWidth, screenWidth);
                int enemyStartY = Math.Max(enemyTop, 0);
                int enemyEndY = Math.Min(enemyTop + enemyHeight, screenHeight);
                for (int spriteX = enemyStartX; spriteX < enemyEndX; spriteX++)
                {
                    if (distanceFromWall[spriteX] > Math.Abs(distanceToEnemy))
                    {
                        isEnemyVisible = true;
                        for (int spriteY = enemyStartY; spriteY < enemyEndY; spriteY++)
                        {
                            char enemySymbol = enemySprite[(spriteY - enemyTop) * enemySprite.GetLength(0) / enemyHeight, (spriteX - enemyLeft) * enemySprite.GetLength(1) / enemyWidth];
                            if (enemySymbol == ' ') continue;
                            frameBuffer[spriteY, spriteX] = enemySymbol;
                        }
                    }
                }
            }
            foreach (var proj in projectiles)
            {
                bool isProjInPortal1 = Math.Sqrt((proj.ProjectileX - portal1X) * (proj.ProjectileX - portal1X) + (proj.ProjectileY - portal1Y) * (proj.ProjectileY - portal1Y)) < 0.1;
                bool isProjInPortal2 = Math.Sqrt((proj.ProjectileX - portal2X) * (proj.ProjectileX - portal2X) + (proj.ProjectileY - portal2Y) * (proj.ProjectileY - portal2Y)) < 0.1;

                double diffProjX = proj.ProjectileX - playerX;
                double diffProjY = proj.ProjectileY - playerY;
                double angleToProj = Math.Atan2(diffProjY, diffProjX);
                double angleDiffProj = angleToProj - playerAngle;
                while (angleDiffProj < -PI) angleDiffProj += 2 * PI;
                while (angleDiffProj > PI) angleDiffProj -= 2 * PI;
                double distanceToProj = Math.Sqrt(diffProjY * diffProjY + diffProjX * diffProjX);
                int projHeight = (int)(screenHeight / distanceToProj) / 4 * proj.SizeMultiplier;
                int projColumn = (int)((angleDiffProj + FOV / 2) / FOV * screenWidth);
                int projWidth = projHeight;
                int projLeft = projColumn - projWidth / 2;
                int projRight = projLeft + projWidth;
                if (projRight > 0 && projLeft < screenWidth)
                {
                    int projTop = (screenHeight - projHeight) / 2;
                    int projBottom = projTop + projHeight;
                    for (int projX = Math.Max(projLeft, 0); projX < Math.Min(projRight, screenWidth); projX++)
                    {
                        for (int projY = Math.Max(projTop, 0); projY < Math.Min(projBottom, screenHeight); projY++)
                        {
                            char projSymbol = proj.UsingProjSprite[(projY - projTop) * proj.UsingProjSprite.GetLength(0) / projHeight, (projX - projLeft) * proj.UsingProjSprite.GetLength(1) / projWidth];
                            if (projSymbol == ' ') continue;
                            if (distanceFromWall[projX] > distanceToProj) frameBuffer[projY, projX] = projSymbol;
                        }
                    }
                }
            }

            isEnemyMoving = false;
            if (enemyMove && enemy1.IsAlive)
            {
                if (distanceToEnemy < 5)
                {
                    isEnemyMoving = true;
                    double enemyNextX = enemy1.EnemyX - Math.Cos(angleToEnemy) * 0.03;
                    double enemyNextY = enemy1.EnemyY - Math.Sin(angleToEnemy) * 0.03;
                    if (map[(int)enemy1.EnemyY][(int)enemyNextX] != '#')
                    {
                        enemy1.EnemyX = enemyNextX;
                    }
                    if (map[(int)enemyNextY][(int)enemy1.EnemyX] != '#')
                    {
                        enemy1.EnemyY = enemyNextY;
                    }
                }
            }
            var toRemove = new List<Projectile>();
            foreach (var proj in projectiles)
            {
                if (proj.IsAlive)
                {
                    double projEnemyDiffX = proj.ProjectileX - enemy1.EnemyX;
                    double projEnemyDiffY = proj.ProjectileY - enemy1.EnemyY;
                    if (Math.Sqrt(projEnemyDiffY * projEnemyDiffY + projEnemyDiffX * projEnemyDiffX) < 0.175)
                    {
                        toRemove.Add(proj);
                        proj.IsAlive = false;
                        proj.SizeMultiplier = 5;
                        proj.UsingProjSprite = explodeProjectileSprite;
                        enemy1.Health--;
                    }
                    proj.ProjectileX += Math.Cos(proj.Angle) * 0.3;
                    proj.ProjectileY += Math.Sin(proj.Angle) * 0.3;
                    if (map[(int)proj.ProjectileY][(int)proj.ProjectileX] == '#')
                    {
                        proj.IsAlive = false;
                        proj.UsingProjSprite = explodeProjectileSprite;
                        proj.SizeMultiplier = 5;
                        toRemove.Add(proj);
                        
                        proj.ProjectileX -= Math.Cos(proj.Angle) * 0.5;
                        proj.ProjectileY -= Math.Sin(proj.Angle) * 0.5;
                    }
                }
                else
                {
                    toRemove.Add(proj);
                    proj.DeathTimer++;
                }
            }
            if (enemy1.Health <= 0) enemy1.IsAlive = false;
            foreach (var proj in toRemove)
            {
                if (proj.DeathTimer == 2) proj.SizeMultiplier = 3;
                if (proj.DeathTimer == 5) proj.SizeMultiplier = 2;
                if (proj.DeathTimer == 10) proj.SizeMultiplier = 1;
                if (proj.DeathTimer > 15)
                {
                    projectiles.Remove(proj);
                }
            }
            //template enemy movement towards the player when distance < 6
            // next test comment to verify git verioning
            if (cursor)
            {
                int middleY = screenHeight / 2;
                int middleX = screenWidth / 2;
                frameBuffer[middleY - 1, middleX] = '│';
                frameBuffer[middleY + 1, middleX] = '│';
                frameBuffer[middleY, middleX - 1] = '─';
                frameBuffer[middleY, middleX + 1] = '─';
                frameBuffer[middleY, middleX - 2] = '─';
                frameBuffer[middleY, middleX + 2] = '─';
                frameBuffer[middleY, middleX] = '◌';
            }
            StringBuilder frame = new StringBuilder(screenHeight * (screenWidth + 2));

            frame.Clear();

            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    if (miniMap && y < miniMapBuffer.GetLength(0) && x < miniMapBuffer.GetLength(1))
                        frame.Append(miniMapBuffer[y, x]);
                    else
                        frame.Append(frameBuffer[y, x]);
                }
                frame.AppendLine();
            }
            Console.Write(frame.ToString());
            Console.WriteLine($"Player Position: ({playerY}, {playerX}) PlayerAngle: {playerAngle} FOV: {FOV} EnemyVisible: {isEnemyVisible} enemyAnimStep: {enemyAnimationStep} projCount: {projectiles.Count}                  ");
            Console.SetCursorPosition(0, 0);
            frameCount++;
            long currentTime = stopwatch.ElapsedMilliseconds;
            if (currentTime - lastTime >= 1000)
            {
                fps = frameCount;
                frameCount = 0;
                lastTime = currentTime;
                Console.Title = $"FPS: {fps}";
            }
            // calculate the next position based on movement
            double nextX = playerX;
            double nextY = playerY;
            while (Console.KeyAvailable == true)
            {
                Console.ReadKey(true);
            } //remove stored keys
            if (IsKeyPressed(ConsoleKey.W)) // move forward
            {
                nextX += speed / 10 * Math.Cos(playerAngle);
                nextY += speed / 10 * Math.Sin(playerAngle);
                moveDirection = "Forward";
            }
            else if (IsKeyPressed(ConsoleKey.S)) // move backward
            {
                nextX -= speed / 10 * Math.Cos(playerAngle);
                nextY -= speed / 10 * Math.Sin(playerAngle);
                moveDirection = "Backward";
            }
            if (IsKeyPressed(ConsoleKey.RightArrow)) // move backward
            {
                nextX += speed / 20 * Math.Cos(playerAngle + PI / 2);
                nextY += speed / 20 * Math.Sin(playerAngle + PI / 2);
            }
            if (IsKeyPressed(ConsoleKey.LeftArrow)) // move backward
            {
                nextX -= speed / 20 * Math.Cos(playerAngle + PI / 2);
                nextY -= speed / 20 * Math.Sin(playerAngle + PI / 2);
            }
            if (IsKeyJustPressed(ConsoleKey.M)) miniMap = !miniMap; //toggle mini map
            if (IsKeyJustPressed(ConsoleKey.N))
            {
                cursor = !cursor; //toggle cursor
            }
            // Check for collisions with walls
            if (map[(int)playerY][(int)nextX] != '#') // check x direction
            {
                playerX = nextX;
            }
            if (map[(int)nextY][(int)playerX] != '#') // check y direction
            {
                playerY = nextY;
            }
            if (IsKeyPressed(ConsoleKey.A)) // Turn left
            {
                playerAngle -= rotationSpeed;
                if (playerAngle < 0) playerAngle += 2 * PI;
            }
            if (IsKeyPressed(ConsoleKey.D)) // Turn right
            {
                playerAngle += rotationSpeed;
                if (playerAngle > 2 * PI) playerAngle -= 2 * PI;
            }
            if (IsKeyJustPressed(ConsoleKey.OemPlus)) FOV += 0.1; // Increase FOV
            if (IsKeyJustPressed(ConsoleKey.F))
            {
                projectiles.Add(new Projectile { ProjectileX = playerX, ProjectileY = playerY, IsAlive = true, Angle = playerAngle, UsingProjSprite = projectileSprite, SizeMultiplier = 1 });
                //fire
            }
            if (IsKeyJustPressed(ConsoleKey.OemMinus)) FOV -= 0.1;
            if (IsKeyJustPressed(ConsoleKey.O)) miniMapScale++;
            if (IsKeyJustPressed(ConsoleKey.Oem4))
            {
                portal1X = playerX + 1 * Math.Cos(playerAngle);
                portal1Y = playerY + 1 * Math.Sin(playerAngle);
            }
            if (IsKeyJustPressed(ConsoleKey.Oem6))
            {
                portal2X = playerX + 1 * Math.Cos(playerAngle);
                portal2Y = playerY + 1 * Math.Sin(playerAngle);
            }
            if (IsKeyJustPressed(ConsoleKey.I)) miniMapScale--;
            if (IsKeyJustPressed(ConsoleKey.Z)) enemyMove = !enemyMove;
            //if (isEnemyVisible) enemyMove = false;
            //else enemyMove = true;
            if (IsKeyJustPressed(ConsoleKey.E))
            {
                enemytestX = random.Next(0, map[0].Length);
                enemytestY = random.Next(0, map.Length);
                while (map[enemytestY][enemytestX] == '#')
                {
                    enemytestX = random.Next(0, map[0].Length);
                    enemytestY = random.Next(0, map.Length);
                }
                enemy1.EnemyX = enemytestX + 0.5;
                enemy1.EnemyY = enemytestY + 0.5;
            }

            bool isInPortal1 = Math.Sqrt((playerX - portal1X) * (playerX - portal1X) + (playerY - portal1Y) * (playerY - portal1Y)) < 0.1;
            bool isInPortal2 = Math.Sqrt((playerX - portal2X) * (playerX - portal2X) + (playerY - portal2Y) * (playerY - portal2Y)) < 0.1;
            if (!wasPortalUsed && isInPortal1)
            {
                wasPortalUsed = true;
                playerX = portal2X; // + Math.Cos(playerAngle) * 0.1;
                playerY = portal2Y; // + Math.Sin(playerAngle) * 0.1;
            }
            else if (!wasPortalUsed && isInPortal2)
            {
                wasPortalUsed = true;
                playerX = portal1X; // + Math.Cos(playerAngle) * 0.1;
                playerY = portal1Y; // + Math.Sin(playerAngle) * 0.1;
            }
            else if (!isInPortal1 && !isInPortal2) wasPortalUsed = false;
            
            Thread.Sleep(10);
        }
        static bool IsKeyPressed(ConsoleKey key)
        {
            return (GetAsyncKeyState((int)key) & 0x8000) != 0;
        }
        static bool IsKeyJustPressed(ConsoleKey key)
        {
            bool pressed = IsKeyPressed(key);

            if (pressed && lastKey != key)
            {
                lastKey = key;
                return true;
            }

            if (!pressed && lastKey == key)
            {
                lastKey = ConsoleKey.NoName;
            }

            return false;
        }
    }
    static ConsoleKey lastKey = ConsoleKey.NoName;

    static char GetWallShade(double rayLength, int ceiling, int floor, int y)
    {
        if (y == floor - 2 && rayLength < 2) return '▓';
        if (y == ceiling + 2 && rayLength < 2) return '▓';
        if (y == floor - 2 && rayLength < 3) return '▒';
        if (y == ceiling + 2 && rayLength < 3) return '▒';
        if (y == floor - 2 && rayLength < 4) return '░';        // "texture"
        if (y == ceiling + 2 && rayLength < 4) return '░';
        if (y == floor - 2 && rayLength < 6) return ' ';
        if (y == ceiling + 2 && rayLength < 6) return ' ';


        if (rayLength < 2) return '█';
        else if (rayLength < 3) return '▓';
        else if (rayLength < 4) return '▒';
        else if (rayLength < 6) return '░';
        else if (rayLength < 6.5) return '`';
        else return ' ';

    }
    class Enemy
    {
        public double EnemyX { get; set; }
        public double EnemyY { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
    }
    class Projectile
    {
        public double ProjectileX { get; set; }
        public double ProjectileY { get; set; }
        public bool IsAlive { get; set; }
        public double Angle { get; set; }
        public required char[,] UsingProjSprite { get; set; }
        public int DeathTimer { get; set; }
        public int SizeMultiplier { get; set; }
    }
}