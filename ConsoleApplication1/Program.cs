using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Хей хо");
            Console.ReadLine();
            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
}
