using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Text;
using System;

namespace GameOfLife.Models
{
 
    public class Game
    {
        private int _refreshDelay = 500;
        
        public bool Paused { get; private set; }
        public event Func<Task>? OnChangeAsync;
        public bool[,] CurrentState { get; set; }
        public string Seed { get; private set; }
        
        public Game(int width, int heigth)
        {
            // initialize game
            Paused = false;
            var state = new bool[width, heigth];

            var ranGen = new Random();
            var boardSeed = new List<int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < heigth; y++)
                {
                    var isAlive = ranGen.Next(100) <= 20;
                    state[x, y] = isAlive;
                    boardSeed.Add(isAlive ? 1 : 0);
                }
            }

            Seed = EncodeBase64($"{width}:{heigth}:{string.Join("", boardSeed)}");
            CurrentState = state;
        }

        public Game(string seed)
        {
            Paused = false;

            var (width, height, board) = DecodeSeed(seed);

            var state = new bool[width, height];
            var idx = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var isAlive = board[idx] == 1;
                    state[x, y] = isAlive;
                    idx++;
                }
            }

            Seed = seed;
            CurrentState = state;
        }

        public void Run()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!Paused)
                    {
                        // run turn and invoke event
                        CurrentState = RunTurn(CurrentState);
                        OnChangeAsync?.Invoke();
                    }

                    await Task.Delay(_refreshDelay);
                }
            });
        }

        public void TogglePaused() => Paused = !Paused;

        private static string EncodeBase64(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        private static string DecodeBase64(string s)
        {
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }

        private static (int Width, int Height, char[] Board) DecodeSeed(string seed)
        {
            var b64 = DecodeBase64(seed);
            var parts = b64.Split(':');

            int width, height;
            if (parts.Count() < 3 || !int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height))
                throw new ArgumentException("Invalid Seed");

            return (Width: width, Height: height, Board: parts[2].ToCharArray());
        }

        private static bool GetNewCellState(int x, int y, bool[,] state)
        {
            var determineCellState = (int x, int y, bool[,] state) =>
            {
                // return 0 for out of bounds coordinates
                if (x < 0 || x >= state.GetLength(0) || y < 0 || y >= state.GetLength(1))
                    return 0;

                return state[x, y] ? 1 : 0;
            };

            // count number of living neighbors
            var liveNeighbors = determineCellState(x - 1, y, state)
                + determineCellState(x - 1, y + 1, state)
                + determineCellState(x, y + 1, state)
                + determineCellState(x + 1, y + 1, state)
                + determineCellState(x + 1, y, state)
                + determineCellState(x + 1, y - 1, state)
                + determineCellState(x, y - 1, state)
                + determineCellState(x - 1, y - 1, state);

            // return new state of cell
            return (state[x, y], liveNeighbors) switch
            {
                (true, < 2) => false,
                (true, > 3) => false,
                (false, 3) => true,
                _ => state[x, y]
            };
        }

        private static bool[,] RunTurn(bool[,] currentState)
        {
            var xLen = currentState.GetLength(0);
            var yLen = currentState.GetLength(1);

            var futureState = new bool[xLen, yLen];

            for (int x = 0; x < xLen; x++) 
            {
                for (int y = 0; y < yLen; y++)
                {
                    futureState[x, y] = GetNewCellState(x, y, currentState);
                }
            }

            return futureState;
        }
    }
}
