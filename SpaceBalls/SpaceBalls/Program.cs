using System;

namespace SpaceBalls
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SpaceBalls game = new SpaceBalls())
            {
                game.Run();
            }
        }
    }
#endif
}

