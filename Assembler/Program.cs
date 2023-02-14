namespace Assembler
{
    public class Program
    {
        static void Main(string[] args)
        {
            FileHandler.FileHandler Fhandler = new FileHandler.FileHandler();
            Converter converter = new Converter();
            
            //Get the file path from the command line
            Console.Write("Path: ");
            string str = Console.ReadLine();

            if (str == null) // check if the string is null
            {
                Console.WriteLine("No path entered");
            }
            else if (!IsPathValidAndAsm(str)) // check if the path is a valid path and is .asm
            {
                Console.WriteLine("Path is not valid or not an .asm file");
            }
            else //gets the file, converts it, and print out new file in the same directory as the origin file
            {
                List<string> file = Fhandler.GetFile(str);
                List<string> converted = converter.ConvertListToMachineCode(file);
                Fhandler.PrintFile(str, converted, ".hack");
            }
        }

        /// <summary>
        /// Check if a path is valid and asm
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsPathValidAndAsm(string path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.Exists && fi.Extension == ".asm";
        }
    }
}