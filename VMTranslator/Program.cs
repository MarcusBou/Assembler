using FileHandler;
using System;
using System.Reflection.Metadata;

namespace VMTranslator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FileHandler.FileHandler fhandler = new FileHandler.FileHandler();
            Translator translator = new Translator();
                    //Get the file path from the command line
            Console.Write("Path: ");
            string str = Console.ReadLine();

            if (str == null) // check if the string is null
            {
                Console.WriteLine("No path entered");
            }
            else
            {
                if (str.Contains(".vm")) // check if the path is a file
                {
                    List<string> lines = fhandler.GetFile(str);
                    List<string> converted = translator.TranslateToASM(lines);
                    fhandler.PrintFile(str, converted, ".asm");
                }
                else if(Directory.Exists(str)) // if the path is a directory
                {
                    List<string> lines = fhandler.GetFiles(str, ".vm", "sys.vm");
                    List<string> converted = translator.TranslateToASM(lines);
                    fhandler.PrintFile(str, converted, ".asm", str);
                }
            }
        }
    }
}