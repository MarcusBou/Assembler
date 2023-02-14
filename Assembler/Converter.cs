using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    public class Converter
    {
        //AInstruction start value has to be 0
        private readonly string AInstructionStartValue = "0";

        //CInstruction start value has to be 111
        private readonly string CInstructionStartValue = "111";

        //The startindex of auto allocating ram
        private int ramindex = 16;
        
        //The symboltable AInstruction refer too
        private Dictionary<string, string> symbolTable = new Dictionary<string, string>
        {
            { "R0", "0" },
            { "R1", "1" },
            { "R2", "2" },
            { "R3", "3" },
            { "R4", "4" },
            { "R5", "5" },
            { "R6", "6" },
            { "R7", "7" },
            { "R8", "8" },
            { "R9", "9" },
            { "R10", "10" },
            { "R11", "11" },
            { "R12", "12" },
            { "R13", "13" },
            { "R14", "14" },
            { "R15", "15" },
            { "SP", "0" },
            { "LCL", "1" },
            { "ARG", "2" },
            { "THIS", "3" },
            { "THAT", "4" },
            { "SCREEN", "16384" },
            { "KBD", "24576" }
        };

        // The comp table for Cinstructions
        private readonly Dictionary<string, string> compLookup = new Dictionary<string, string>
        {
            { "0", "0101010" },
            { "1", "0111111" },
            { "-1", "0111010" },
            { "D", "0001100" },
            { "A", "0110000" },
            { "!D", "0001101" },
            { "!A", "0110001" },
            { "-D", "0001111"},
            { "-A", "0110011" },
            { "D+1", "0011111" },
            { "A+1", "0110111" },
            { "D-1", "0001110" },
            { "A-1", "0110010" },
            { "D+A", "0000010" },
            { "D-A", "0010011" },
            { "A-D", "0000111" },
            { "D&A", "0000000" },
            { "D|A", "0010101" },
            { "M", "1110000" },
            { "!M", "1110001" },
            { "-M", "1110011" },
            { "M+1", "1110111" },
            { "M-1", "1110010" },
            { "D+M", "1000010" },
            { "M+D", "1000010" },
            { "D-M", "1010011" },
            { "M-D", "1000111" },
            { "D&M", "1000000" },
            { "D|M", "1010101" }
        };

        //the dest table for C instructions
        private readonly Dictionary<string, string> destLookUp = new Dictionary<string, string>
        {
            { "", "000" },
            { "M", "001" },
            { "D", "010" },
            { "MD", "011" },
            { "A", "100" },
            { "AM", "101" },
            { "AD", "110" },
            { "AMD", "111" }
        };

        //the jump table for C instructions
        private readonly Dictionary<string, string> jumpLookUp = new Dictionary<string, string>
        {
            { "", "000" },
            { "JGT", "001" },
            { "JEQ", "010" },
            { "JGE", "011" },
            { "JLT", "100" },
            { "JNE", "101" },
            { "JLE", "110" },
            { "JMP", "111" }
        };

        /// <summary>
        /// Converts list of string to binary
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public List<string> ConvertListToMachineCode(List<string> strings)
        {
            // Remove counter
            int remcount = 0;
            for (int i = 0; i < strings.Count; i++)
            {
                // Gets label and add them to the lookup
                if (strings[i].StartsWith("("))
                {
                    string tempstring = strings[i].Replace("(", string.Empty).Replace(")", string.Empty);
                    symbolTable.Add(tempstring, (i - remcount).ToString());
                    remcount++;
                }
            }
            // Removes all labels
            strings.RemoveAll(x => x.StartsWith("("));
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].StartsWith("@"))
                {
                    strings[i] = strings[i].Replace("@", string.Empty);
                    strings[i] = CreateAInstruction(strings[i]);
                }
                else
                {
                    strings[i] = CreateCInstruction(strings[i]);
                }
            }
            return strings;
        }

        public string CreateAInstruction(string instruction)
        {
            //It will try to parse the string to int if it fails
            try
            {
                int value = int.Parse(instruction);
                return AInstructionStartValue + Convert.ToString(value, 2).PadLeft(15, '0');
            }
            catch (Exception) //It will lookup in the symbol table and send that value
            {
                if (!symbolTable.ContainsKey(instruction))//If the symbol table does not contain it. it creates it
                {
                    symbolTable.Add(instruction, ramindex.ToString());
                    ramindex++;
                }
                return AInstructionStartValue + Convert.ToString(Int32.Parse(symbolTable[instruction]), 2).PadLeft(15, '0');//Returns the value
            }
        }

        /// <summary>
        /// Create a Cinstruction from a string
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public string CreateCInstruction(string instruction)
        {
            //Prepare values for configuring
            string dest, comp, jump;
            //Prepare array
            string[] ins = new string[3] { "", "", "" };
            
            //Split up the instruction string
            string[] insParts = instruction.Split(new string[2] { "=", ";" }, StringSplitOptions.None);
            for (var i = 0; i < insParts.Length; i++) { ins[i] = insParts[i]; };//Move it to the array
            if (instruction.Contains("=")) // check if instruction contains =. if it does  it is a dest instruction
            {
                dest = destLookUp[ins[0]];
                comp = compLookup[ins[1]];
                jump = jumpLookUp[ins[2]];
            }
            else // return a non dest instruction
            {
                dest = destLookUp[""];
                comp = compLookup[ins[0]];
                jump = jumpLookUp[ins[1]];
            }

            return CInstructionStartValue + comp + dest + jump; //return cinstruction
        }
    }
}
