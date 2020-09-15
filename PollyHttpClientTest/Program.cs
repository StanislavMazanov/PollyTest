using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollyHttpClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server().Run();


            Console.WriteLine("Нажмите любую кнопку...");
            Console.ReadKey();

        }
    }
}
