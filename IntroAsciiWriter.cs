namespace RepoRanger;

public static class IntroAsciiWriter
{
    public static void Run(string version)
    {
        string[] lines2 = new[]
        {
            $"The Repo Ranger v{version}. https://rac.so/ranger",
            $"Licensed under the GNU General Public License v3.0: https://www.gnu.org/licenses/gpl-3.0.en.html",
            $"Made with \u2660 by Racso."
        };

        WriteColoredAsciiArtWithText(lines2);
    }

    private static void WriteColoredAsciiArtWithText(string[] lines)
    {
        string[] asciiArt = new[]
        {
            "  ██ ",
            " ██  ",
            "  █  ",
        };

        ConsoleColor darkBlue = ConsoleColor.DarkBlue;
        ConsoleColor lightRed = ConsoleColor.DarkRed;

        Console.WriteLine();
        for (int i = 0; i < asciiArt.Length; i++)
        {
            foreach (char c in asciiArt[i])
            {
                if (c == '░')
                    Console.ForegroundColor = darkBlue;
                else if (c == '█')
                    Console.ForegroundColor = lightRed;

                Console.Write(c);
            }

            Console.ResetColor();

            if (i < lines.Length)
            {
                Console.WriteLine(" " + lines[i]);
            }
            else
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine();
    }
}