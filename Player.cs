using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

public class Player
{
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
    public int CellSize { get; private set; }
    public bool IsVisible { get; private set; } = true;
    private float opacity = 1.0f;
    private float fadeOutSpeed = 0.05f;
    public PlayerState State { get; private set; } = PlayerState.Active;
    public float Opacity { get; private set; } = 1f;
    public int Score { get; set; }

    private float moveCooldown = 0.2f; // Temps minimum entre deux déplacements (en secondes)
    private float cooldownTimer = 0f;  // Chronomètre pour gérer le délai de déplacement
    public string name { get; set; }

    public Player(string Name, Vector2 position, Texture2D texture, int cellSize)
    {
        name = Name;
        Position = position;
        Texture = texture;
        CellSize = cellSize;
        Score = 0;
    }

    public void Move(KeyboardState keyboardState, GameTime gameTime, int gridWidth, int gridHeight, Platform platform)
    {
        if (State != PlayerState.Active) return;

        // Mettre à jour le chronomètre de cooldown
        cooldownTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Si le cooldown n'est pas écoulé, ne pas permettre de déplacement
        if (cooldownTimer < moveCooldown) return;

        Vector2 direction = Vector2.Zero;

        // Vérifiez les entrées clavier pour un seul mouvement
        if (keyboardState.IsKeyDown(Keys.Left)) direction.X = -1;
        else if (keyboardState.IsKeyDown(Keys.Right)) direction.X = 1;
        else if (keyboardState.IsKeyDown(Keys.Up)) direction.Y = -1;
        else if (keyboardState.IsKeyDown(Keys.Down)) direction.Y = 1;

        // Si aucune direction n'est donnée, ne pas effectuer de mouvement
        if (direction == Vector2.Zero) return;

        // Enregistrer la position actuelle pour la collecte des scores
        Vector2 oldPosition = Position;

        // Calculer la nouvelle position
        Position += direction;

        // S'assurer que la position reste dans les limites de la grille
        Position = new Vector2(
            MathHelper.Clamp(Position.X, 0, gridWidth - 1),
            MathHelper.Clamp(Position.Y, 0, gridHeight - 1)
        );

        // Réinitialiser le cooldown après un mouvement valide
        cooldownTimer = 0f;

        // Collecter les scores sur la cellule actuelle
        CollectScoresOnPath(oldPosition, platform);
    }

    private void CollectScoresOnPath(Vector2 oldPosition, Platform platform)
    {
        int collectedScore = platform.GetScoreAtCell(Position);
        if (collectedScore != 0)
        {
            Score += collectedScore;
            platform.CollectScore(Position);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Calculer la position de dessin en pixels
        Rectangle destinationRectangle = new Rectangle(
            (int)(Position.X * CellSize),
            (int)(Position.Y * CellSize),
            CellSize,
            CellSize
        );

        spriteBatch.Draw(Texture, destinationRectangle, Color.White * Opacity);
    }

    public void Update(GameTime gameTime)
    {
        if (State == PlayerState.Invisible)
        {
            opacity -= fadeOutSpeed;

            if (opacity <= 0)
            {
                opacity = 0;
                State = PlayerState.Dead;
                IsVisible = false;
            }
        }
    }

    public enum PlayerState
    {
        Active,
        Invisible,
        Dead
    }
}
