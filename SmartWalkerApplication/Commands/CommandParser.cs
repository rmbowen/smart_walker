using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class CommandParser
    {

        public CommandParser()
        {
        }

        public void start()
        {
            while (true)
            {
                Console.WriteLine("Enter a command: ");
                string input = Console.ReadLine();
                Console.WriteLine();
                string[] words = input.Split(' ');
                bool invalidCommand = true;
                if (words[0].Equals("Mic"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Mic Array Command Entered");
                        invalidCommand = false;
                    }
                }
                else if(words[0].Equals("Navigation"))
                {
                    if (words.Length == 2)
                    {
                        Console.WriteLine("Navigation Command Entered");
                        invalidCommand = false;
                    }
                }
                else if (words[0].Equals("Force"))
                {
                    if (words.Length == 2)
                    {
                        Console.WriteLine("Force Command Entered");
                        invalidCommand = false;
                    }
                }
                else if (words[0].Equals("Wireless"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Wireless Command Entered");
                        invalidCommand = false;

                        WirelessCommand wc = new WirelessCommand();
                        wc.sendEmail("thomasdemeo@gmail.com", "def2191@rit.edu", "High There", "Poop Poop");
                    }
                }
                if (invalidCommand) {
                    Console.WriteLine(input + " is not a valid command");
                    Console.WriteLine("Valid Commands Include:");
                    Console.WriteLine("Mic (read microphone array)");
                    Console.WriteLine("Navigation [degree] (start navigation system)");
                    Console.WriteLine("Force [Left/Right] (read force sensors)");
                    Console.WriteLine();

                }
            }
        }
    }

    
}
