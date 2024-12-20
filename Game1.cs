using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework.Audio;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;


namespace V1JeuVideo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        private GameState gameState = GameState.Playing;
        private float transitionAlpha = 0f;
        private bool isOnSecondPlatform = false;

        private double collapseTimer = 0;
        private double collapseInterval = 1.0;

        private string name =" ";
        private Player player;
        private Platform platform1;
        private Platform platform2;

        private Texture2D cellTexture;
        private Texture2D cellTexture2;
        private Texture2D playerTexture;
        private Texture2D blackTexture;

        private int gridWidth;
        private int gridHeight;
        private int cellSize = 80;

        private SpriteFont font;

        private SoundEffect deathSound;
        private SoundEffect roundEndSound;

        private bool isKeyPressed = false;
        private KeyboardState previousKeyboardState = Keyboard.GetState();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            gridWidth = GraphicsDevice.Viewport.Width / cellSize;
            gridHeight = GraphicsDevice.Viewport.Height / cellSize;

            platform1 = new Platform(gridWidth, gridHeight, cellSize, null);
            platform2 = new Platform(gridWidth, gridHeight, cellSize, null);

            player = new Player(name, new Vector2(gridWidth / 2, gridHeight - 1), null, cellSize);
            ResetPlayerScore();

            gameState = GameState.EnteringName; // Démarrage dans l'état de saisie
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Charger le contenu du fichier XML
            LoadGameContent();

            // Charger les textures 
            blackTexture = new Texture2D(GraphicsDevice, 1, 1);
            blackTexture.SetData(new[] { Color.Black });
        }

        private void LoadGameContent()
        {
            string filePath = "Content/GameConfig.xml";
            XDocument doc = XDocument.Load(filePath);

            // Charger les textures
            var textures = doc.Root.Element("Textures").Elements("Texture");
            foreach (var texture in textures)
            {
                string name = texture.Attribute("Name").Value;
                string path = texture.Attribute("Path").Value;

                switch (name)
                {
                    case "Cell":
                        cellTexture = Content.Load<Texture2D>(path);
                        break;
                    case "Cell2":
                        cellTexture2 = Content.Load<Texture2D>(path);
                        break;
                    case "Player":
                        playerTexture = Content.Load<Texture2D>(path);
                        break;
                }
            }

            // Charger les polices
            var fonts = doc.Root.Element("Fonts").Elements("Font");
            foreach (var fontElement in fonts)
            {
                string name = fontElement.Attribute("Name").Value;
                string path = fontElement.Attribute("Path").Value;

                if (name == "DefaultFont")
                {
                    font = Content.Load<SpriteFont>(path);
                }
            }

            // Charger les sons
            var sounds = doc.Root.Element("Sounds").Elements("Sound");
            foreach (var sound in sounds)
            {
                string name = sound.Attribute("Name").Value;
                string path = sound.Attribute("Path").Value;

                switch (name)
                {
                    case "DeathSound":
                        deathSound = Content.Load<SoundEffect>(path);
                        break;
                    case "RoundEndSound":
                        roundEndSound = Content.Load<SoundEffect>(path);
                        break;
                }
            }

            // Charger les plateformes
            platform1 = new Platform(gridWidth, gridHeight, cellSize, cellTexture);
            platform2 = new Platform(gridWidth, gridHeight, cellSize, cellTexture2);

            // Charger le joueur
           
            // var playerElement = doc.Root.Element("Player");
            // int playerX = int.Parse(playerElement.Element("InitialPosition").Attribute("X").Value);
            // int playerY = int.Parse(playerElement.Element("InitialPosition").Attribute("Y").Value);

            var playerElement = doc.Root.Element("Player");
            int playerX = 5; // on Remets la valeur de la position du joueur par defaut 
            int playerY = 5;

            // Charger le score et le remettre a zero
            int playerScore = int.Parse(playerElement.Element("scores").Element("Score").Value);
            player = new Player(name, new Vector2(playerX, playerY), playerTexture, cellSize)
            {
                Score = 0
            };
           
        }



        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState(); // Obtenir l'état du clavier

            switch (gameState)
            {
                case GameState.EnteringName:
                    //gerer etat du joueur qui entre son nom 
                    AskForPlayerName(keyboardState);
                    break;
                case GameState.Playing:
                    // Gérer l'état "Playing"
                    HandlePlayingState(gameTime, keyboardState);
                    break;
                case GameState.Transition:
                    // Gérer la transition entre les plateformes
                    HandleTransitionState(gameTime);
                    break;
                case GameState.TransitionToGameOver:
                    // Gérer la transition vers l'écran de Game Over
                    HandleTransitionToGameOver(gameTime);
                    break;
                case GameState.GameOver:
                    // Redémarrer si le joueur appuie sur R
                    if (keyboardState.IsKeyDown(Keys.R) && !previousKeyboardState.IsKeyDown(Keys.R))
                    {
                        RestartGame();
                    }
                    // Sauvegarder et quitter si le joueur appuie sur E
                    else if (keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
                    {
                      
                        Exit();  // Quitter le jeu
                    }
                    break;

                default:
                    // Permettre au joueur de saisir son nom si ce n'est pas encore fait
                    if (string.IsNullOrEmpty(name))
                    {
                        AskForPlayerName(keyboardState);
                    }
                    else
                    {
                        base.Update(gameTime); // Continuer les mises à jour normales
                    }
                    break;
            }


            base.Update(gameTime);
        }

        private void RestartGame()
        {
            // Réinitialiser l'état du jeu
            isOnSecondPlatform = false;
            Initialize();
            gameState = GameState.Playing;
            ResetPlayerScore();
        }

        private void ResetPlayerScore()
        {
            string filePath = "Content/GameConfig.xml";
            try
            {
                Console.WriteLine("Réinitialisation du score...");

                XDocument doc = XDocument.Load(filePath);
                var playerElement = doc.Root.Element("Player");

                if (playerElement != null)
                {
                    // Réinitialiser le score à zéro
                    playerElement.Element("Score").Value = "0";
                    player.Score = 0;
                    doc.Save(filePath);

                    Console.WriteLine("Score réinitialisé à zéro.");
                }
                else
                {
                    Console.WriteLine("L'élément 'Player' est introuvable.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la réinitialisation du score : {ex.Message}");
            }
        }



        private void AskForPlayerName(KeyboardState keyboardState)
        {
            // Add letters A-Z to the name
            foreach (var key in keyboardState.GetPressedKeys())
            {
                if (key >= Keys.A && key <= Keys.Z && !previousKeyboardState.IsKeyDown(key))
                {
                    if (name.Length < 20) // Limit to 20 characters
                    {
                        name += key.ToString(); // Add the letter to the name
                    }
                }
            }

            // Handle Backspace to remove the last character
            if (keyboardState.IsKeyDown(Keys.Back) && !previousKeyboardState.IsKeyDown(Keys.Back))
            {
                if (name.Length > 0)
                {
                    name = name.Substring(0, name.Length - 1); // Remove the last character
                }
            }

            // Handle Enter to confirm the name
            if (keyboardState.IsKeyDown(Keys.Enter) && !previousKeyboardState.IsKeyDown(Keys.Enter))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    // Set the player's name
                    player.name = name; // Assuming player is already initialized
                    gameState = GameState.Playing; // Move to the playing state
                    Console.WriteLine($"Player name confirmed: {name}");
                }
            }

            // Update previous keyboard state
            previousKeyboardState = keyboardState;
        }




        private void DrawEnteringNameState()
        {
            string promptText = "Enter your name: " + name;
            string instructionText = "Press Enter to confirm";

            Vector2 promptPosition = new Vector2(GraphicsDevice.Viewport.Width / 2 - font.MeasureString(promptText).X / 2, GraphicsDevice.Viewport.Height / 2);
            Vector2 instructionPosition = promptPosition + new Vector2(0, font.LineSpacing + 10); // Ajouter un décalage sous le texte principal

            spriteBatch.DrawString(font, promptText, promptPosition, Color.White);
            spriteBatch.DrawString(font, instructionText, instructionPosition, Color.Gray);
        }




        private void HandlePlayingState(GameTime gameTime, KeyboardState keyboardState)
        {
            // Timer pour l'effondrement des cellules
            collapseTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (collapseTimer >= collapseInterval)
            {
                if (!isOnSecondPlatform)
                {
                    platform1.CollapseRandomCells(new Random(), 3);
                }
                else
                {
                    platform2.CollapseRandomCells(new Random(), 3);
                }

                collapseTimer = 0;
            }

            // Gérer le mouvement du joueur
            if (!isOnSecondPlatform)
            {
                player.Move(keyboardState, gameTime, gridWidth, gridHeight, platform1);
            }
            else
            {
                player.Move(keyboardState, gameTime, gridWidth, gridHeight, platform2);
            }

            // Vérifier si le joueur est encore sur une cellule intacte
            int row = (int)player.Position.Y;
            int col = (int)player.Position.X;

            if (!isOnSecondPlatform && !platform1.IsCellIntact(row, col))
            {
                gameState = GameState.Transition;
                transitionAlpha = 0f;
                deathSound.Play();
            }
            else if (isOnSecondPlatform && !platform2.IsCellIntact(row, col))
            {
                gameState = GameState.TransitionToGameOver;
                transitionAlpha = 0f;
                deathSound.Play();
            }

            // Mettre à jour l'état du joueur
            player.Update(gameTime);
        }


        private void HandleTransitionState(GameTime gameTime)
        {

            transitionAlpha += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;

            if (transitionAlpha >= 1f)
            {
                if (isOnSecondPlatform)
                {
                    gameState = GameState.GameOver;


                }
                else
                {
                    isOnSecondPlatform = true;
                    LoadSecondPlatform();
                    gameState = GameState.Playing;
                    transitionAlpha = 0f;
                }
            }
        }

        private void SaveGameContent()
        {
            string filePath = "Content/GameConfig.xml";

            try
            {
                Console.WriteLine("Saving game...");

                // Load the XML document
                XDocument doc = XDocument.Load(filePath);

                // Find the player element by name
                var playerElement = doc.Descendants("Player").FirstOrDefault(p => p.Element("Name")?.Value == player.name);

                if (playerElement != null)
                {
                    // If the player exists, update the scores
                    var scoresElement = playerElement.Element("scores");
                    if (scoresElement == null)
                    {
                        scoresElement = new XElement("scores");
                        playerElement.Add(scoresElement);
                    }

                    // Add the new score
                    scoresElement.Add(new XElement("Score", player.Score.ToString()));

                    // Update the initial position
                    var initialPositionElement = playerElement.Element("InitialPosition");
                    if (initialPositionElement != null)
                    {
                        initialPositionElement.Attribute("X").Value = player.Position.X.ToString();
                        initialPositionElement.Attribute("Y").Value = player.Position.Y.ToString();
                    }
                    else
                    {
                        playerElement.Add(new XElement("InitialPosition",
                            new XAttribute("X", player.Position.X.ToString()),
                            new XAttribute("Y", player.Position.Y.ToString())));
                    }
                }
                else
                {
                    // If the player does not exist, create a new player element
                    XElement newPlayerElement = new XElement("Player",
                        new XElement("Name", player.name),
                        new XElement("scores", new XElement("Score", player.Score.ToString())),
                        new XElement("InitialPosition",
                            new XAttribute("X", player.Position.X.ToString()),
                            new XAttribute("Y", player.Position.Y.ToString()))
                    );

                    // Add the new player element to the root
                    doc.Root.Add(newPlayerElement);
                }

                // Save the changes to the XML file
                doc.Save(filePath);
                Console.WriteLine("Game saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
            }
        }




        private void HandleTransitionToGameOver(GameTime gameTime)
        {
            transitionAlpha += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;

            if (transitionAlpha >= 1f)
            {
                if (gameState != GameState.GameOver)
                {

                    roundEndSound.Play();
                     SaveGameContent();
                }

                gameState = GameState.GameOver;

            }

        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (gameState == GameState.EnteringName)
            {
                DrawEnteringNameState();
            }
            else if (gameState == GameState.Playing || gameState == GameState.Transition || gameState == GameState.TransitionToGameOver)
            {
                if (!isOnSecondPlatform)
                {
                    platform1.Draw(spriteBatch, font);
                }
                else
                {
                    platform2.Draw(spriteBatch, font);
                }

                player.Draw(spriteBatch);
            }

            // Affichage du score global et du nom du joueur
            string scoreText = "Score:   " + player.Score;
            string playerNameText = "   Player: " + name;

            // Mesurer la largeur des textes pour les positionner correctement
            Vector2 scoreSize = font.MeasureString(scoreText);
            Vector2 playerNameSize = font.MeasureString(playerNameText);

            // Positionner le score à gauche
            Vector2 scorePosition = new Vector2(10, 10);

            // Positionner le nom du joueur juste à droite du score
            Vector2 playerNamePosition = new Vector2(scorePosition.X + scoreSize.X + 10, 10);

            // Dessiner les textes à l'écran
            spriteBatch.DrawString(font, scoreText, scorePosition, Color.White);
            spriteBatch.DrawString(font, playerNameText, playerNamePosition, Color.GhostWhite);

            if (gameState == GameState.Transition || gameState == GameState.TransitionToGameOver || gameState == GameState.GameOver)
            {
                spriteBatch.Draw(
                    blackTexture,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                    Color.Black * transitionAlpha
                );
            }

            if (gameState == GameState.GameOver)
            {
                player.Position = new Vector2(5, 5);
                // Définir le message principal et le score
                string gameOverText = "GAME OVER!";
                string restartText = "Press  'R'  to  Restart";
                string endGameText = "Press 'E' to End Game";
                string ScoreText = $" Your  Final  Score is : {player.Score}";

                // Mesurer la taille des textes pour les centrer
                Vector2 gameOverSize = font.MeasureString(gameOverText);
                Vector2 restartSize = font.MeasureString(restartText);
                Vector2 endGameSize = font.MeasureString(endGameText);
                Vector2 ScoreSize = font.MeasureString(ScoreText);

                // Calculer les positions centrées
                Vector2 centerScreen = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                Vector2 gameOverPosition = centerScreen - new Vector2(gameOverSize.X / 2, gameOverSize.Y + 50);
                Vector2 ScorePosition = centerScreen - new Vector2(ScoreSize.X / 2, 1);
                Vector2 restartPosition = centerScreen - new Vector2(restartSize.X / 2, -50);
                Vector2 endGamePosition = centerScreen - new Vector2(endGameSize.X / 2, 48);

                // Dessiner les textes
                spriteBatch.DrawString(font, gameOverText, gameOverPosition, Color.Red);
                spriteBatch.DrawString(font, ScoreText, ScorePosition, Color.White);
                spriteBatch.DrawString(font, restartText, restartPosition, Color.Yellow);
                spriteBatch.DrawString(font, endGameText, endGamePosition, Color.Yellow);

                player.Draw(spriteBatch);

            }

            spriteBatch.End();
            base.Draw(gameTime);
        }



        private void LoadSecondPlatform()
        {
            platform2 = new Platform(gridWidth, gridHeight, cellSize, cellTexture2);
            player.Position = new Vector2(gridWidth / 2, gridHeight - 1);
        }

        public static void XslTransform(string xmlFilePath, string xsltFilePath, string outputFilePath)
        {
            try
            {


                var xslt = new XslCompiledTransform();
                var settings = new XsltSettings(true, true);
                var resolver = new XmlUrlResolver();
                xslt.Load(xsltFilePath, settings, resolver);
                var arguments = new XsltArgumentList();

                using var writer = XmlWriter.Create(outputFilePath, xslt.OutputSettings);
                xslt.Transform(new XPathDocument(xmlFilePath), arguments, writer);
                Console.WriteLine($" validation rÃ©ussite :{outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de transformation,{ex.Message}");
            }

        }

        public static async Task ValidateXmlFileAsync(string xsdFilePath, string xmlFilePath)
        {
            var settings = new XmlReaderSettings
            {
                Async = true
            };
            settings.Schemas.Add(null, xsdFilePath);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += ValidationCallBack;
            Console.WriteLine($"Nombre de shemas utilisés dans la validation:{settings.Schemas.Count}");
            using (var reader = XmlReader.Create(xmlFilePath, settings))
                try
                {
                    while (await reader.ReadAsync())
                    {
                    }

                    Console.WriteLine(" la Validation réussite");
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"Fichier manquant: {ex.Message}");
                }
                catch (XmlException ex)
                {
                    Console.WriteLine($"Erreur de lecture du fichier xml: {ex.Message}");
                }
        }

        private static void ValidationCallBack(object? sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("WAENING :" + e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)

            {
                Console.WriteLine("ERROR:" + e.Message);
            }
        }

        public static void Serialization<T>(T obj, string outputFilePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StreamWriter(outputFilePath);
                serializer.Serialize(writer, obj);
                Console.WriteLine($"Sérialisation réussie : {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sérialisation : {ex.Message}");
            }
        }

        public static T Deserialization<T>(string xmlFilePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StreamReader(xmlFilePath);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la désérialisation : {ex.Message}");
                throw;
            }
        }

    }


    public enum GameState
    {
        EnteringName,
        Playing,
        Transition,
        TransitionToGameOver,
        GameOver
    }
}
