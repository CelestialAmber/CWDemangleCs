using System;
using System.Text.RegularExpressions;
using CWDemangleCs;

namespace CWDemangleCs.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length >= 1)
            {
                string symbol = Regex.Replace(args[0], @"['""]", "");
                bool keepVoid = false;
                bool mwExtensions = true;
                bool displayHelp = false;

                if(args.Length > 1)
                {
                    for(int i = 1; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "--keep_void":
                                keepVoid = true;
                                break;
                            case "--mw_extensions":
                                mwExtensions = true;
                                break;
                            case "--help":
                            case "-h":
                                displayHelp = true;
                                break;
                        }
                    }
                }

                if (displayHelp)
                {
                    DisplayHelp();
                }
                else {
                    DemangleOptions options = new DemangleOptions(!keepVoid, mwExtensions);
                    string result = CWDemangler.demangle(symbol, options);
                    if(result != null)
                    {
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("Failed to demangle symbol");
                    }
                }
            }
            else
            {
                DisplayHelp();
            }
        }

        public static void DisplayHelp()
        {
            Console.WriteLine("Usage: CWDemangleCs.CLI symbol [--keep_void] [--mw_extensions]");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("  --keep_void");
            Console.WriteLine("  --mw_extensions");
            Console.WriteLine("  --help");
        }
    }
}
