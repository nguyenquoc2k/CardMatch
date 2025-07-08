using System;
using UnityEngine;

namespace MultiplayerGames
{
    public class Theme : MonoBehaviour
    // Overridding Awake function without calling base.Awake() or setting instance = this WILL RESULT IN USELESS SINGLETON
    // UBehavior's static field instance will always be replaced by the newest instance
    {
        public static Theme instance;

        private void Awake()
        {
            instance = this;
        }

        public enum Player
        {
            PLAYER_1,
            PLAYER_2,
        }

        //default Color
        readonly static Color PLAYER1_COLOR = new Color(0.427f, 0.714f, 0.941f); //Blue
        readonly static Color PLAYER2_COLOR = new Color(0.941f, 0.498f, 0.427f); //Red
        readonly static Color PLAYER3_COLOR = new Color(1f, 0.9821f, 0.1745f); //yellow
        readonly static Color PLAYER4_COLOR = new Color(0.4455f, 0.9058f, 0.3450f); //green

        [SerializeField] Color player1Color = PLAYER1_COLOR,
            player2Color = PLAYER2_COLOR,
            player3Color = PLAYER3_COLOR,
            player4Color = PLAYER4_COLOR;

        public static Color GetColor(Player p)
        {
            if (instance == null)
            {
                switch (p)
                {
                    case Player.PLAYER_1:
                        return PLAYER1_COLOR;
                    case Player.PLAYER_2:
                        return PLAYER2_COLOR;
                    default:
                        return Color.white;
                }
            }
            else
            {
                switch (p)
                {
                    case Player.PLAYER_1:
                        return instance.player1Color;
                    case Player.PLAYER_2:
                        return instance.player2Color;
                    default:
                        return Color.white;
                }
            }
        }

        public static Color GetPlayer1Color()
        {
            return GetColor(Player.PLAYER_1);
        }

        public static Color GetPlayer2Color()
        {
            return GetColor(Player.PLAYER_2);
        }

        public static string GetPlayerName(Player player)
        {
            switch (player)
            {
                case Player.PLAYER_1: return "blue";
                case Player.PLAYER_2: return "red";
                default: return "blue";
            }
        }
    }
}