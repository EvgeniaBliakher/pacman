using System;

// Pacman
// Evgenia Golubeva, 1. rocnik, MFF UK 
// letni semestr 2020
// zapoctovy program Programovani II

namespace pacman
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // main entry point of program
            using (var game = new Game1())
                game.Run();
        }
    }
}