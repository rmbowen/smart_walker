//
// CommandParser.cs
//
// This class takes in an input from the command line and
// parses it out into a number of valid commands which are
// then executed
//
// @author - Thomas DeMeo 
//

using SmartWalkerApplication.Commands.HUB;
using SmartWalkerApplication.Commands.Vitals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalkerApplication.Commands
{
    class CommandParser
    {

        /**
         * start - Begins taking in CommandLine arguments and parses them
         *          into valid or invalid commands
         * 
         * @return - none
         **/

        public void start()
        {
            // Loop forever
            while (true)
            {
                // Take in User input
                Console.WriteLine("Enter a command: ");
                string input = Console.ReadLine();
                Console.WriteLine();

                // Parse input and store in string array
                string[] words = input.Split(' ');
                bool invalidCommand = true;

                // Check for each of the valid command types
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

                        // Create NavCommand with initial direction to head in
                        NavigationCommand nc = new NavigationCommand(int.Parse(words[1]));
                        nc.start();
                        //nc.startKinect();
                        //SmartWalkerKinect
                        //nc.getIMUData();

                    }
                }
                else if (words[0].Equals("Force"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Force Command Entered");
                        invalidCommand = false;

                        ForceCommand fc = new ForceCommand();
                        int leftForce = fc.getLeftForce();
                        int rightForce = fc.getRightForce();

                        double leftPercent = ((leftForce - 300.0) / 723.0)*100;
                        double rightPercent = ((rightForce - 300.0) / 723.0)*100;

                        if (leftPercent < 0)
                        {
                            leftPercent = 0;
                        }

                        if (rightPercent < 0)
                        {
                            rightPercent = 0;
                        }

                        Console.WriteLine("Left Percent: " + Convert.ToInt32(leftPercent));
                        Console.WriteLine("Right Percent: " + Convert.ToInt32(rightPercent));
                        Force force = new Force();
                    }
                }
                else if (words[0].Equals("Heart"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Heart Rate Command Entered");
                        invalidCommand = false;

                        HeartRateCommand hrc = new HeartRateCommand();
                        hrc.start();
                    }
                }
                else if (words[0].Equals("Thermal"))
                {
                    if (words.Length == 1)
                    {
                        Console.WriteLine("Thermal Command Entered");
                        invalidCommand = false;

                        ThermalCommand tc = new ThermalCommand();
                        string temperature = tc.start();

                        Console.WriteLine(temperature);
                        Thermal thermal = new Thermal(temperature);
                        XMLStore xml = XMLStore.Instance;
                        xml.thermal.AddLast(thermal);
                    }
                }
                else if (words[0].Equals("Wireless"))
                {
                    if (words.Length == 2)
                    {
                        Console.WriteLine("Wireless Command Entered");
                        invalidCommand = false;

                        WirelessCommand wc = new WirelessCommand();
                        // Send Email to entered email address
                        wc.sendEmail("thomasdemeo@gmail.com", words[1], "Smart Walker Data", null);
                    }
                }
                else if(words[0].Equals("Strain"))
                {
                    if(words.Length == 1)
                    {
                        Console.WriteLine("Strain Command Entered");
                        invalidCommand = false;

                        StrainCommand sc = new StrainCommand();
                        int leftStrain = sc.getLeftStrain();
                        int rightStrain = sc.getRightStrain();
                    }
                }
                else if(words[0].Equals("Vitals"))
                {
                    Console.WriteLine("Vitals Command Entered");
                    invalidCommand = false;

                    int[] rightStrain = new int[5];
                    int[] leftStrain = new int[5];

                    int[] leftForce = new int[5]; 
                    int[] rightForce = new int[5]; 

                    StrainCommand sc = new StrainCommand();
                    ForceCommand fc = new ForceCommand();

                    // run the force and strain 5 times
                    for (int i = 0; i < 5; ++i)
                    {
                        leftForce[i] = fc.getLeftForce();
                        rightForce[i] = fc.getRightForce();

                        System.Threading.Thread.Sleep(500);

                        leftStrain[i] = sc.getLeftStrain();
                        rightStrain[i] = sc.getRightStrain();
                    }

                    double forceAvgLeft = 0;
                    double forceAvgRight = 0;

                    int strainAvgLeft = 0;
                    int strainAvgRight = 0;

                    // Average the 5 readings for force and strain
                    for (int i = 0; i < 5; ++i)
                    {

                        double leftPercent = ((leftForce[i] - 300.0) / 723.0) * 100;
                        double rightPercent = ((rightForce[i] - 300.0) / 723.0) * 100;

                        if (leftPercent < 0)
                        {
                            leftPercent = 0;
                        }

                        if (rightPercent < 0)
                        {
                            rightPercent = 0;
                        }

                        forceAvgLeft += leftPercent;
                        forceAvgRight += rightPercent;

                        strainAvgLeft += leftStrain[i];
                        strainAvgRight += rightStrain[i];
                    }

                    forceAvgLeft /= 5;
                    forceAvgRight /= 5;

                    strainAvgLeft /= 5;
                    strainAvgRight /= 5;

                    HeartRateCommand tc = new HeartRateCommand();
                    string results = tc.startBoth();

                    var parts = results.Split(' ');

                    string temperature = parts[0];
                    string HR = parts[1];

                    Console.WriteLine(Convert.ToInt32(forceAvgLeft));
                    Console.WriteLine(Convert.ToInt32(forceAvgRight));
                    Console.WriteLine(strainAvgLeft);
                    Console.WriteLine(strainAvgRight);
                    Console.WriteLine(temperature);
                    Console.WriteLine(HR);

                }
                else if (words[0].Equals("Auto"))
                {
                    invalidCommand = false;
                    bool strainThresholdNotReached = true;

                    StrainCommand sc = new StrainCommand();
                    int rightStrainTest;

                    int[] rightStrain = new int[5];
                    int[] leftStrain = new int[5];

                    int[] leftForce = new int[5];
                    int[] rightForce = new int[5]; 

                    while(strainThresholdNotReached) {

                        rightStrainTest = sc.getRightStrain();
                        if (rightStrainTest > 1)
                        {
                            strainThresholdNotReached = false;
                        }
                    }

                    ForceCommand fc = new ForceCommand();

                    // run the force and strain 5 times
                    for (int i = 0; i < 5; ++i)
                    {
                        leftForce[i] = fc.getLeftForce();
                        rightForce[i] = fc.getRightForce();

                        System.Threading.Thread.Sleep(500);

                        leftStrain[i] = sc.getLeftStrain();
                        rightStrain[i] = sc.getRightStrain();
                    }

                    double forceAvgLeft = 0;
                    double forceAvgRight = 0;

                    int strainAvgLeft = 0;
                    int strainAvgRight = 0;

                    // Average the 5 readings for force and strain
                    for (int i = 0; i < 5; ++i)
                    {
                        
                        double leftPercent = ((leftForce[i] - 300.0) / 723.0) * 100;
                        double rightPercent = ((rightForce[i] - 300.0) / 723.0) * 100;

                        if (leftPercent < 0)
                        {
                            leftPercent = 0;
                        }

                        if (rightPercent < 0)
                        {
                            rightPercent = 0;
                        }

                        forceAvgLeft += leftPercent;
                        forceAvgRight += rightPercent;

                        strainAvgLeft += leftStrain[i];
                        strainAvgRight += rightStrain[i];
                    }

                    forceAvgLeft /= 5;
                    forceAvgRight /= 5;

                    strainAvgLeft /= 5;
                    strainAvgRight /= 5;

                    HeartRateCommand tc = new HeartRateCommand();
                    string results = tc.startBoth();

                    var parts = results.Split(' ');

                    string temperature = parts[1];
                    string HR = parts[0];

                    XMLStore xml = XMLStore.Instance;
                    Thermal th = new Thermal(temperature);
                    xml.thermal.AddLast(th);

                    Force fo = new Force();
                    fo.leftForce = forceAvgLeft;
                    fo.rightForce = forceAvgRight;
                    fo.leftStrain = strainAvgLeft;
                    fo.rightStrain = strainAvgRight;
                    xml.force.AddLast(fo);

                    HeartRate hr = new HeartRate(HR);
                    xml.heartRate.AddLast(hr);

                    Console.WriteLine(Convert.ToInt32(forceAvgLeft));
                    Console.WriteLine(Convert.ToInt32(forceAvgRight));
                    Console.WriteLine(strainAvgLeft);
                    Console.WriteLine(strainAvgRight);
                    Console.WriteLine(temperature);
                    Console.WriteLine(HR);
                }
                // If no valid command was entered print out list of possible commands
                if (invalidCommand) {
                    Console.WriteLine(input + " is not a valid command");
                    Console.WriteLine("Valid Commands Include:");
                    Console.WriteLine("Mic (read microphone array)");
                    Console.WriteLine("Navigation [degree] (start navigation system)");
                    Console.WriteLine("Force (read force sensors)");
                    Console.WriteLine("Strain (read strain gauges");
                    Console.WriteLine("Thermal (measure temperature)");
                    Console.WriteLine("Heart (measure heart rate)");
                }
            }
        }
    }
}
