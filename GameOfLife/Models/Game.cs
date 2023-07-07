using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace GameOfLife.Models
{
 
    public class Game
    {
        private int _refreshDelay = 500;
        
        public bool Paused { get; private set; }
        public event Func<Task>? OnChangeAsync;
        public bool[,] CurrentState { get; set; }
        
        public Game(int width, int heigth)
        {
            // initialize game
            Paused = false;
            var state = new bool[width, heigth];

            var ranGen = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < heigth; y++)
                {
                    state[x, y] = ranGen.Next(100) <= 35;
                }
            }

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
