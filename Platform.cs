using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
public class Platform
{
    public Texture2D CellTexture { get; set; }
    public int CellSize { get; set; }
    private int GridWidth;
    private int GridHeight;
    public List<Vector2> Cells { get; set; } // Liste des cellules intactes
    public Dictionary<Vector2, int> CellScores { get; set; } // Dictionnaire de scores pour chaque cellule

    private Random random = new Random();

    public Platform(int gridWidth, int gridHeight, int cellSize, Texture2D cellTexture)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        CellSize = cellSize;
        CellTexture = cellTexture;
        Cells = new List<Vector2>(); // Initialiser la liste des cellules
        CellScores = new Dictionary<Vector2, int>(); // Initialiser le dictionnaire des scores
        InitializeCells();
    }
    private void InitializeCells()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Vector2 cell = new Vector2(x, y);
                Cells.Add(cell); // Ajouter toutes les cellules de la grille

                // Décider si cette cellule doit avoir un score (par exemple, 30% de chances)
                if (random.NextDouble() < 0.3) // 30% de chances d'avoir un bonus
                {
                    int score = GenerateRandomScore();
                    CellScores[cell] = score; // Attribuer un score à cette cellule
                }
            }
        }
    }


    private int GenerateRandomScore()
    {
        // Génère des scores aléatoires (+100, +500, *2, etc.)
        int[] possibleScores = { +100, +500, +2, -50,-200 }; // Scores possibles
        return possibleScores[random.Next(possibleScores.Length)];
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 offset = default)
    {
        foreach (var cell in Cells)
        {
            Rectangle destinationRectangle = new Rectangle(
                (int)(cell.X * CellSize + offset.X), // Position X en pixels
                (int)(cell.Y * CellSize + offset.Y), // Position Y en pixels
                CellSize, // Largeur
                CellSize  // Hauteur
            );

            spriteBatch.Draw(CellTexture, destinationRectangle, Color.White);

            // Affichage du score sur la cellule
            if (CellScores.ContainsKey(cell))
            {
                string scoreText = CellScores[cell].ToString(); // Score en texte
                Vector2 textPosition = new Vector2(
                    destinationRectangle.X + (CellSize / 2) - (font.MeasureString(scoreText).X / 2),
                    destinationRectangle.Y + (CellSize / 2) - (font.MeasureString(scoreText).Y / 2)
                );

                spriteBatch.DrawString(font, scoreText, textPosition, Color.Black); // Texte en noir
            }
        }
    }



    public bool IsCellIntact(int row, int col)
    {
        return Cells.Contains(new Vector2(col, row)); // Vérifie si la cellule existe
    }

    public void CollapseRandomCells(Random random, int numberOfCells)
    {
        for (int i = 0; i < numberOfCells && Cells.Count > 0; i++)
        {
            int index = random.Next(Cells.Count);
            Vector2 collapsedCell = Cells[index];
            Cells.RemoveAt(index); // Effondrer une cellule
            CellScores.Remove(collapsedCell); // Supprimer le score de la cellule effondrée
        }
    }

    // Retourne le score de la cellule donnée, ou 0 si la cellule a été effondrée
    public int GetScoreAtCell(Vector2 cell)
    {
        return CellScores.ContainsKey(cell) ? CellScores[cell] : 0;
    }

    // Marquer la cellule comme "récoltée" en supprimant son score
    public void CollectScore(Vector2 cell)
    {
        if (CellScores.ContainsKey(cell))
        {
            CellScores.Remove(cell); // Supprimer le score de cette cellule
        }
    }
}
