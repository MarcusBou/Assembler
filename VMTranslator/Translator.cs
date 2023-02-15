using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VMTranslator
{
    public class Translator
    {
        private int labelcount;
        private int tempStartScope = 5;
        private Stack<string> stack = new Stack<string>();
        public List<string> TranslateMultiple(List<string> vmCode)
        {
            List<string> result = new List<string>();
            result.AddRange(new string[]
            {
                "@256",
                "D=A",
                "@0",
                "M=D"
            });

            vmCode.Insert(0, "call Sys.init 0");
            result.AddRange(TranslateToASM(vmCode));
            return result;
        }
        public List<string> TranslateToASM(List<string> vmCode)
        {
            stack.Push("static");
            List<string> HDLCode = new List<string>();
            labelcount = 0;
            for (int i = 0; i < vmCode.Count; i++)
            {
                string[] line = vmCode[i].Split(" ");
                string[] command = new string[0];
                if (line[0].Equals("push"))
                {
                    command = PushCommand(line[1], int.Parse(line[2]));
                }
                else if (line[0].Equals("pop"))
                {
                    command = PopCommand(line[1], int.Parse(line[2]));
                }
                else if (line[0].Equals("label"))
                {
                    command = new string[] { $"({line[1]})" };
                }
                else if (line[0].Contains("goto"))
                {
                    command = GotoCommand(line[0], line[1]);
                }
                else if (line[0].Equals("function"))
                {
                    command = FunctionCommand(line[1], int.Parse(line[2]));
                }
                else if (line[0].Equals("return"))
                {
                    command = ReturnCommand();

                }
                else if (line[0].Equals("call"))
                {
                    command = CallCommand(line[1], line[2]);
                }
                else
                {
                    command = ActionCommand(line[0]);
                }
                HDLCode.AddRange(command);
                HDLCode.Add("");
            }
            HDLCode.AddRange(new string[] { "(END)", "@END", "0;JMP" });
            return HDLCode;
        }

        public string[] PushCommand(string ramslot, int value)
        {
            switch (ramslot) {
                case "temp":
                    return new string[]
                    {
                        "// Push Temp" + value,
                        "@" + (tempStartScope + value),
                        "D=M",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "static":
                    return new string[]
                    {
                        "// Push static" + value,
                        $"@{stack.Peek()}.{value}",
                        "D=M",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "local":
                    return new string[] 
                    {
                        "// Push Local" + value,
                        "@" + value,
                        "D=A",
                        "@LCL",
                        "A=M+D",
                        "D=M",
                        "@R13",
                        "M=D",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "argument":
                    return new string[]
                    {
                        "// Push argument" + value,
                        "@" + value,
                        "D=A",
                        "@ARG",
                        "A=M+D",
                        "D=M",
                        "@R13",
                        "M=D",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "this":
                    return new string[]
                    {
                        "// Push this" + value,
                        "@" + value,
                        "D=A",
                        "@THIS",
                        "A=M+D",
                        "D=M",
                        "@R13",
                        "M=D",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "that":
                    return new string[]
                    {
                        "// Push that" + value,
                        "@" + value,
                        "D=A",
                        "@THAT",
                        "A=M+D",
                        "D=M",
                        "@R13",
                        "M=D",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "constant":
                    return new string[]
                    {
                        "// Push constant " + value,
                        "@" + value,
                        "D=A",
                        "@SP",
                        "A=M",
                        "M=D",
                        "@SP",
                        "M=M+1"
                    };
                case "pointer":
                    if (value == 1)
                    {
                        return new string[]
                        {
                            "// Push pointer 1",
                            "@THAT",
                            "D=M",
                            "@SP",
                            "A=M",
                            "M=D",
                            "@SP",
                            "M=M+1"
                        };
                    }
                    else
                    {
                        return new string[]
                        {
                            "// Push pointer 0",
                            "@THIS",
                            "D=M",
                            "@SP",
                            "A=M",
                            "M=D",
                            "@SP",
                            "M=M+1"
                        };
                    }
                default:
                    return new string[0];
            }
        }

        public string[] PopCommand(string ramslot, int value)
        {
            switch (ramslot)
            {
                case "temp":
                    return new string[]
                    {
                        "// Pop temp " + value,
                        "@" + value,
                        "D=A",
                        "@5",
                        "D=D+A",
                        "@R13",
                        "M=D",
                        "@SP",
                        "M=M-1",
                        "A=M ",
                        "D=M",
                        "@R13",
                        "A=M",
                        "M=D",
                    };
                case "static":
                    return new string[]
                    {
                        "// Pop static " + value,
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        $"@{stack.Peek()}.{value}",
                        "M=D",
                    };
                case "local":
                    return new string[]
                    {
                        "// Pop local " + value,
                        "@LCL",
                        "D=M",
                        "@" + value,
                        "D=D+A",
                        "@R13",
                        "M=D",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "@R13",
                        "A=M",
                        "M=D",
                    };
                case "argument":
                    return new string[]
                    {
                        "// Pop argument     " + value,
                        "@ARG",
                        "D=M",
                        "@" + value,
                        "D=D+A",
                        "@R13",
                        "M=D",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "@R13",
                        "A=M",
                        "M=D",
                    };
                case "this":
                    return new string[]
                    {
                        "// Pop this " + value,
                        "@THIS",
                        "D=M",
                        "@" + value,
                        "D=D+A",
                        "@R13",
                        "M=D",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "@R13",
                        "A=M",
                        "M=D",
                    };
                case "that":
                    return new string[]
                    {
                        "// Pop that " + value,
                        "@THAT",
                        "D=M",
                        "@"+value,
                        "D=D+A",
                        "@R13",
                        "M=D",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "@R13",
                        "A=M",
                        "M=D",
                    };
                case "pointer":
                    if (value == 1)
                    {
                        return new string[]
                        {
                            "// Pop pointer 1",
                            "@SP",
                            "AM=M-1",
                            "D=M",
                            "@THAT",
                            "M=D",
                        };
                    }
                    else
                    {
                        return new string[]
                            {
                            "// Pop pointer 0",
                            "@SP",
                            "AM=M-1",
                            "D=M",
                            "@THIS",
                            "M=D",
                            };
                    }
                    default:
                    return new string[] { "" };
            }
        }
        
        public string[] ActionCommand(string command)
        {
            switch (command)
            {
                case "add":
                    return new string[]
                    {
                        "// Add",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "A=A-1",
                        "M=M+D",
                    };
                case "sub":
                    return new string[] 
                    {
                        "// Sub",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "A=A-1",
                        "M=M-D",
                    };
                case "neg":
                    return new string[]
                    {
                        "// Neg",
                        "@SP",
                        "A=M-1",
                        "M=-M"
                    };
                case "eq":
                    labelcount++;
                    return new string[]
                    {
                        "// Equal",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "A=A-1",
                        "D=M-D",
                        "M=-1",
                        "@EQ" + labelcount,
                        "D; JEQ",
                        "@SP",
                        "A = M - 1",
                        "M = 0",
                        $"(EQ{labelcount})"
                    };
                case "gt":
                    labelcount++;
                    return new string[]
                    {
                        "// Greater than",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "A=A-1",
                        "D=M-D",
                        "M=-1",
                        "@GT" + labelcount,
                        "D; JGT",
                        "@SP",
                        "A = M - 1",
                        "M = 0",
                        $"(GT{labelcount})"
                    };
                case "lt":
                    labelcount++;
                    return new string[]
                    {
                        "// Less than",
                        "@SP",
                        "AM=M-1",
                        "D=M",
                        "A=A-1",
                        "D=M-D",
                        "M=-1",
                        "@LT" + labelcount,
                        "D; JLT",
                        "@SP",
                        "A = M - 1",
                        "M = 0",
                        $"(LT{labelcount})"
                    };
                case "and":
                    return new string[]
                    {
                        "// And",
                        "@SP" ,
                        "AM=M-1" ,
                        "D=M" ,
                        "A=A-1",
                        "M=M&D"
                    };
                case "or":
                    return new string[]
                    {
                        "// Or",
                        "@SP" ,
                        "AM=M-1" ,
                        "D=M" ,
                        "A=A-1" ,
                        "M=M|D"
                    };
                case "not":
                    return new string[]
                    {
                        "// Not",
                        "@SP" ,
                        "A=M-1" ,
                        "M=!M"
                    };
                default:
                    return new string[0];
            }
        }

        public string[] GotoCommand(string command, string label)
        {
            if (command.Equals("if-goto"))
            {
                return new string[]{
                    "@SP",
                    "M=M-1",
                    "A=M",
                    "D=M",
                    "@" + label,
                    "D; JNE"
                };
            }
            else
            {
                return new string[] { 
                    "@" + label,
                    "0;JMP"
                };
            }
        }

        public string[] FunctionCommand(string command, int argsAmount)
        {
            List<string> result = new List<string>{$"({command})"};
            stack.Push(command.Split(".")[0]);
            for (int i = 0; i < argsAmount; i++)
            {
                result.AddRange(new string[]
                {
                    "@0",
                    "D=A",
                    "@SP",
                    "A=M",
                    "M=D",
                    "@SP",
                    "M=M+1",
                });
            }
            return result.ToArray();
        }

        public string[] CallCommand(string command, string args)
        {
            List<string> tempstring = new List<string>();
            string[] temp = new string[] { "@LCL", "@ARG", "@THIS", "@THAT" };
            tempstring.AddRange(new string[]
            {
                $"@RETURN_ADDR" + labelcount,
                "D=A",
                "@SP",
                "A=M",
                "M=D",
                "@SP",
                "M=M+1",
            });
            for (int i = 0; i < temp.Length; i++)
            {
                tempstring.AddRange(new string[]
                {
                    temp[i],
                    "D=M",
                    "@SP",
                    "A=M",
                    "M=D",
                    "@SP",
                    "M=M+1",
                });
            }
            tempstring.AddRange(new string[] {
                "// ARG = SP - 5 -nArgs",
                "@SP",
                "D=M",
                "@5",
                "D=D-A",
                "@" + args,
                "D=D-A",
                "@ARG",
                "M=D",
                "// LCL = SP",
                "@SP",
                "D=M",
                "@LCL",
                "M=D",
                "@" + command,
                "0;JMP",
                $"(RETURN_ADDR{labelcount})",
            });
            labelcount++;
            return tempstring.ToArray();
        }

        public string[] ReturnCommand()
        {
            if (stack.Count > 1)
            {
                stack.Pop();
            }
            return new string[]{
                "// Return",
                "@LCL",
                "D=M",
                "@R13",
                "M=D",
                "@5",
                "D=A",
                "@R13",
                "D=M-D",
                "A=D",
                "D=M",
                "@R14",
                "M=D",
                "@SP",
                "M=M-1",
                "@SP",
                "A=M ",
                "D=M",
                "@ARG",
                "A=M",
                "M=D",
                "@ARG",
                "D=M",
                "@SP",
                "M=D+1",
                "@1",
                "D=A",
                "@R13",
                "D=M-D",
                "A=D",
                "D=M",
                "@THAT",
                "M=D",
                "@2",
                "D=A",
                "@R13",
                "D=M-D",
                "A=D",
                "D=M",
                "@THIS",
                "M=D",
                "@3",
                "D=A",
                "@R13",
                "D=M-D",
                "A=D",
                "D=M",
                "@ARG",
                "M=D",
                "@4",
                "D=A",
                "@R13",
                "D=M-D",
                "A=D",
                "D=M",
                "@LCL",
                "M=D",
                "@R14",
                "A=M",
                "0;JMP",
            };
        }
    }
}
