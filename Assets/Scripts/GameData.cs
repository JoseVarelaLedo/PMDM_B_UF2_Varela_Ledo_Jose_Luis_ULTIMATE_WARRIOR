using System;

//clase auxiliar para serializar los datos del juego y que se guarden entre niveles las vidas, puntuación y salud

[Serializable]
public class GameData
{    
    public int score;
    public int lives;
    public int health;

    public GameData()
    {
        //
    }

    public GameData (int score, int lives, int health)
    {   
        this.score = score;
        this.lives = lives;
        this.health = health;
    }
}
