using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

public static class Array2D
{
  public static U[,] Mapi<T,U>(this T[,] values, Func<int,int,T,U> fn)
  {
    var result = new U[values.GetLength(0),values.GetLength(1)];
    for (int x = 0; x < values.GetLength(0); x++)
    {
      for (var y = 0; y < values.GetLength(1); y++)
      {
        result[x,y] = fn(x, y, values[x, y]);
      }
    }

    return result;
  }
}