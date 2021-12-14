using System;

namespace AccountBalance3 {
    class Program 
    {

        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var app = new Application();
            app.Bootstrap();
            app.Run();
            app.Dispose();
        }
    }
}
