using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Breakout
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int HEIGHT = 600;                                              //Deafult Height
        const int WIDTH = 870;                                               //Default Width
        const float PADDLE_SPEED = 6.0f;                                        
        const float BALL_SPEED = 6f;
        const int BLOCK_PADDING = 100; 

        int score = 0;                                                      //user score
        

        Texture2D paddleSprite;
        Texture2D ballSprite;
        Texture2D blockSprite;
        Vector2 paddlePosition;
        Vector2 ballPosition;
        Vector2 ballVelocity;
        bool[,] gameMap = new bool[10, 4];
        Rectangle[,] blockMap = new Rectangle[10, 4];
        Random rand = new Random();
        SpriteFont myFont;
        SoundEffect bonk;
        SoundEffect block;

        bool gameOver = false;
        bool inGame = false;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //Set default height and width
            graphics.PreferredBackBufferHeight = HEIGHT;
            graphics.PreferredBackBufferWidth = WIDTH;

            initializeGame();
  
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            // Loads all of the attributes for the game
            spriteBatch = new SpriteBatch(GraphicsDevice);
            paddleSprite = Content.Load<Texture2D>(@"Sprites\paddle");
            ballSprite = Content.Load<Texture2D>(@"Sprites\ball");
            blockSprite = Content.Load<Texture2D>(@"Sprites\block");
            paddlePosition = new Vector2(WIDTH/2 - paddleSprite.Width / 2, HEIGHT -50 - paddleSprite.Height / 2);
            myFont = Content.Load<SpriteFont>(@"Fonts\SpriteFont1");
            bonk = Content.Load<SoundEffect>(@"Sounds\bonk");
            block = Content.Load<SoundEffect>(@"Sounds\block");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            //Exits if escape is pressed
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // Sets A and left arrow keys to move the paddle to the left
            if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                paddlePosition.X -= PADDLE_SPEED;
            }
            //Sets D and right arrow keys to move the paddle to the right
            if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                paddlePosition.X += PADDLE_SPEED;
            }
            //Reset Button
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                initializeGame();
                initializeBall();
            }
            //Moves both the paddle and the ball every cycle
            paddlePosition.X = MathHelper.Clamp(paddlePosition.X, 0, WIDTH - paddleSprite.Width);
            ballPosition += (ballVelocity * BALL_SPEED);

            //Sets bounds for the game map
            if (ballPosition.X <= 0 || ballPosition.X >= WIDTH - ballSprite.Width)
                ballVelocity.X *= -1;
            if (ballPosition.Y <= 0)
                ballVelocity.Y *= -1;
            if (ballPosition.Y >= HEIGHT - ballSprite.Height)
            {
                initializeGame();
                gameOver = true;
                
            }
            
            
            //Creates a Rectangle of the ball, and runs the collision functions
            Rectangle ballRectangle = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballSprite.Width, ballSprite.Height);
            calcPaddleColl(ballRectangle);
            calculateCollisions(ballRectangle);
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            spriteBatch.Draw(paddleSprite, paddlePosition, Color.White);
            spriteBatch.Draw(ballSprite, ballPosition, Color.White);

            //Creates the blocks with different colors
            for (int i = 0; i < 10; i++)
            {
                if(gameMap[i,0] == true)
                    spriteBatch.Draw(blockSprite, new Vector2(i*87, BLOCK_PADDING), Color.Red);
            }
            for (int i = 0; i < 10; i++)
            {
                if (gameMap[i, 1] == true)
                    spriteBatch.Draw(blockSprite, new Vector2(i * 87, blockSprite.Height +1 + BLOCK_PADDING), Color.Blue);
            }
            for (int i = 0; i < 10; i++)
            {
                if (gameMap[i, 2] == true)
                    spriteBatch.Draw(blockSprite, new Vector2(i * 87, 2* blockSprite.Height + 1 + BLOCK_PADDING), Color.Yellow);
            }
            for (int i = 0; i < 10; i++)
            {
                if (gameMap[i, 3] == true)
                    spriteBatch.Draw(blockSprite, new Vector2(i * 87, 3 * blockSprite.Height + 1 + BLOCK_PADDING), Color.Green);
            }
            spriteBatch.DrawString(myFont, "Score: " + score, new Vector2(WIDTH - 200, 5),Color.Red);
            //Sets game to victory status
            if (score == 100)
            {
                ballVelocity = Vector2.Zero;
                spriteBatch.DrawString(myFont, "YOU WIN! Press Enter to play again", new Vector2(100, HEIGHT / 2), Color.Red);
            }
            //Sets the pregame status
            if(!inGame)
                spriteBatch.DrawString(myFont, "Press enter to start. Use the arrow keys, or a and d to move", new Vector2(10, HEIGHT / 2), Color.Green);
            //Sets gameover status
            if(gameOver)
                spriteBatch.DrawString(myFont, "Game Over. Press enter to try again", new Vector2(150, HEIGHT / 2), Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void initializeGame()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    blockMap[i, j] = new Rectangle(i * 87, j * 31 + BLOCK_PADDING, 89, 30);
                    gameMap[i, j] = true;
                }
            }
            score = 0;
            
        }

        void initializeBall()
        {
            ballPosition = new Vector2(paddlePosition.X + paddleSprite.Width /2, paddlePosition.Y - paddleSprite.Height - 5);
            //Gives the ball a random direction to start with
            double alpha = rand.NextDouble();
            float randY = (float)(alpha * 40);

            ballVelocity = new Vector2(randY + 50, -100);
            ballVelocity.Normalize();
            if (rand.NextDouble() > 0.5)
                ballVelocity.X *= -1;

            inGame = true;
            gameOver = false;

        }

        private void calculateCollisions(Rectangle ball)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ball.Intersects(blockMap[i, j]))
                    {
                        int secLeft = Math.Max(blockMap[i,j].Left, ball.Left);
                        int secTop = Math.Max(blockMap[i,j].Top, ball.Top);
                        int secRight = Math.Min(blockMap[i,j].Right, ball.Right);
                        int secBot = Math.Min(blockMap[i,j].Bottom, ball.Bottom);
                        //Caculates the rectangle of intersection, and determines which side the ball hit
                        if(Math.Abs(secLeft - secRight) > Math.Abs(secTop - secBot))
                            ballVelocity.Y *= -1;
                        if (Math.Abs(secLeft - secRight) < Math.Abs(secTop - secBot))
                            ballVelocity.X *= -1;

                        
                        gameMap[i, j] = false;
                        blockMap[i, j] = new Rectangle(0, 0, 0, 0);
                        score+= 4 - j;       
                        block.Play();
                        //Exits loop, and continues the game
                        i = 10;
                        j = 4;
                    }                   
                }
            }
        }

        private void calcPaddleColl(Rectangle ball)
        {
            Rectangle paddle = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleSprite.Width, paddleSprite.Height);
            if (ball.Intersects(paddle))
            {
                int secLeft = Math.Max(paddle.Left, ball.Left);
                int secTop = Math.Max(paddle.Top, ball.Top);
                int secRight = Math.Min(paddle.Right, ball.Right);
                int secBot = Math.Min(paddle.Bottom, ball.Bottom);
                if (Math.Abs(secLeft - secRight) > Math.Abs(secTop - secBot))
                {
                    //Reverses the balls X and Y direction if the ball hits the outer parts of the paddle
                    if (ballPosition.X + ballSprite.Width / 2 < paddlePosition.X + paddleSprite.Width / 3 ||
                        ballPosition.X + ballSprite.Width / 2 > paddlePosition.X + paddleSprite.Width * 2 / 3)
                    {
                        ballVelocity.Y *= -1;
                        ballVelocity.X *= -1;
                    }
                    else
                        ballVelocity.Y *= -1;

                }
                if (Math.Abs(secLeft - secRight) < Math.Abs(secTop - secBot))
                    ballVelocity.X *= -1;

                bonk.Play();                                            //Plays teh bonk sound when the ball hits the paddle
            }
        }

    }
}
