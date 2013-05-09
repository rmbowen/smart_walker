//
// NavigationCommand.cs
//
// Handles the creation of the NavigationCommand, interaction
// with the Kinect, and communicating to both the motors and
// the IMU
//
// @author - Thomas DeMeo 
//

using SmartWalker;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Diagnostics;
using SmartWalkerApplication.Commands.COMConnection;

namespace SmartWalkerApplication.Commands
{
    class NavigationCommand
    {
        static SmartWalkerKinect walkerKinect;
        private COMConnection.COMConnection port;
        private string motorControlString = "51010";
        private double angle = 0.0;

        private const double mmPerEncoder = 0.2659;
        private long leftEncoderTick = 0;
        private long rightEncoderTick = 0;

        // Initial Direction Provided to the NavigationCommand
        private int initialDirectionDegree;

        public NavigationCommand(int initialDirectionDegree)
        {
            this.initialDirectionDegree = initialDirectionDegree;
        }

        /**
         * withinDegrees - Check to see if the two readings provided are within 7
         *                  degrees of each other
         * 
         * @return - none
         **/

        private bool withinDegrees(int initialReading, int currentReading)
        {
            if ((currentReading <= initialReading + 7) && (currentReading >= initialReading - 7))
            {
                return true;
            }
            return false;
        }

        /**
         * start - Begins taking in CommandLine arguments and parses them
         *          into valid or invalid commands
         * 
         * @return - none
         **/

        public void start()
        {
            // Start the Kinect
            startKinect();

            // Get the Arduino Port Object
            port = COMConnection.COMConnection.Instance;
            /*
            // Get the current relative angle from the IMU
            int startingDegree = getAverageCurrentLocationInDegrees();
            int degreeDifference = getDegreeAddition(startingDegree, initialDirectionDegree);

            Console.WriteLine("Degree IMU Needs to get to: " + degreeDifference);
            
            // Perform the initial Swivel to get to the appropriate starting direction
            // If less than 180 swivel right
            if (initialDirectionDegree <= 180)
            {
                port.sendString("N");
                port.sendString("30707");
                System.Threading.Thread.Sleep(500); // Let the wheels get going
            }
            else
            {
                port.sendString("N");
                port.sendString("40707");
                System.Threading.Thread.Sleep(500); // Let the wheels get going
            }
            
            // Until we get to the right direction keep swiveling
            while (!(withinDegrees(degreeDifference, startingDegree)))
            {
                startingDegree = getAverageCurrentLocationInDegrees();
                Console.WriteLine("New Degree: " + startingDegree);
            }
            */
            // Stop, wait 0.5 seconds 
            port.sendString("N");
            port.sendString("51010");
            System.Threading.Thread.Sleep(500);


            while (true)
            {
                //System.Threading.Thread.Sleep(5000);
                // send mode
                //port.sendString(Console.ReadLine());

                // Make sure we are in Navigation Mode
                port.sendString("N");

                //System.Threading.Thread.Sleep(500);

               // string myString = Console.ReadLine();
                // send right ticks
               // port.sendString(Console.ReadLine());

                // send left ticks
                //port.sendString(Console.ReadLine());
                
                // Check for first threshhold
                if (walkerKinect.isEmergency())
                {
                    int turnResult = walkerKinect.isRightTurnBetter();
                    switch (turnResult)
                    {
                        case 1:
                            Console.WriteLine("Go Straight Slowly...");
                            goStraightSlowly();
                            break;
                        case 2:
                            Console.WriteLine("Stop, start turning LEFT!");
                            pivotLeft();
                            break;
                        case 3:
                            Console.WriteLine("Stop, start turning RIGHT!");
                            pivotRight();
                            break;
                        default:
                            Console.WriteLine("SWIVEL!");
                            //walkerKinect.setYPos(((leftEncoderTick + rightEncoderTick / 2) * mmPerEncoder) / 20);
                            //walkerKinect.printMap();

                            swivelRight();
                            break;
                    }
                }
                // Check for Second Threshold
                else if (walkerKinect.isBlocked())
                {
                    int turnResult = walkerKinect.isRightTurnBetter();
                    switch (turnResult)
                    {
                        case 1:
                            Console.WriteLine("Go Straight Slowly...");
                            goStraightSlowly();
                            break;
                        case 2:
                           Console.WriteLine("Start turning slowly LEFT!");
                            glideLeft();
                            break;
                        case 3:
                            Console.WriteLine("Start turning slowly RIGHT!");
                            glideRight();
                            break;
                        default:
                            Console.WriteLine("SWIVEL!");
                            //walkerKinect.setYPos(((leftEncoderTick + rightEncoderTick / 2) * mmPerEncoder) / 20);
                            //walkerKinect.printMap();

                            swivelRight();
                            break;
                    }
                }
                else
                {
                    // If there are no problems just keep going straight
                    //Console.WriteLine("KEEP GOING STRAIGHT!");
                    goStraight();
                }
                //port.readLineString();
                // Send the determined string to the Arduino for the motors
                port.sendString(motorControlString);
                System.Threading.Thread.Sleep(500);

                //port.clearStream();
                /*
                string leftTick = "";
                string rightTick = "";
                
                port.sendString("F");
                    leftTick = port.readLineString();
                    //Console.WriteLine("LEFT: " + leftTick);
                    //leftEncoderTick = tick;
                    /*byte[] asciiBytes = Encoding.ASCII.GetBytes(leftTick);
                    Console.WriteLine("Left ASCII Values");
                    foreach (byte b in asciiBytes)
                    {
                        Console.Write(b);
                    }
                     * */
                    //Console.WriteLine();

                //System.Threading.Thread.Sleep(200);

               /* port.sendString("H");

                    rightTick = port.readLineString();
                    
                Console.WriteLine("Right Ticks: " + rightTick);
                Console.WriteLine();
                
                try
                {
                    leftEncoderTick = long.Parse(leftTick);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR LEFT");
                }
                
                try
                {
                    rightEncoderTick = long.Parse(rightTick);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR RIGHT");
                }
                
                Console.WriteLine("NEW TICKS");
                Console.WriteLine("Left New: " + leftEncoderTick);
                Console.WriteLine("Right New: " + rightEncoderTick);
                
               // System.Threading.Thread.Sleep(500);
                */
            }
 
            // Close the connection to the Arduino
            port.closeConnection();
        }

        private void swivelRight()
        {
            motorControlString = "30707";
        }

        private void pivotRight()
        {
            motorControlString = "11205";
        }

        private void glideRight()
        {
            motorControlString = "11207";
        }

        private void pivotLeft()
        {
            motorControlString = "10512";
        }

        private void glideLeft()
        {
            motorControlString = "10712";
        }

        private void goStraight()
        {
            motorControlString = "10909";
        }

        private void goStraightSlowly()
        {
            motorControlString = "10707";
        } 

        /**
         * getDegreeAddition - this methods adds two degrees together
         *                      taking into account if this number gets over 360
         * 
         * @return - The result of the two degrees added together
         **/

        private int getDegreeAddition(int startDegree, int endDegree)
        {
            int result = startDegree + endDegree;
            if (result > 360)
            {
                result = result - 360;
            }
            return result;
        }

        /**
         * getAverageCurrentLocationInDegrees - This method gets three values from the IMU
         *                                      and averages them together to get a current
         *                                      position
         * 
         * @return - The result the three location values averaged together
         **/

        private int getAverageCurrentLocationInDegrees()
        {
            int startingDegree1 = getCurrentLocationInDegrees();
            int startingDegree2 = getCurrentLocationInDegrees();
            int startingDegree3 = getCurrentLocationInDegrees();

            return ((startingDegree1 + startingDegree2 + startingDegree3) / 3);

        }

        /**
        * getCurrentLocationInDegrees - This method gets an IMU value from the Arduino
        * 
        * @return - The resulting value as an integer
        **/

        private int getCurrentLocationInDegrees()
        {
            port.sendString("D");
             //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(100);

            // Read the IMU value being sent back from the arduino
            string degreeString = port.readLineString();

            int degree = 0;
            try
            {
                degree = int.Parse(degreeString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
            return degree;
        }

        /**
        * startKinect - Initializes the Kinect and any other
        *              subcomponents that neede to be
        * 
        * @return - none
        **/

        private void startKinect()
        {
            walkerKinect = new SmartWalkerKinect();

            walkerKinect.startKinect();

            /*
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Set the Interval to 10 seconds.
            aTimer.Interval = 10000;
            aTimer.Enabled = true;
            //while (endProgram) { }
            */

            /*if (Console.ReadLine() != null)
            {
                walkerKinect.printMap();
                walkerKinect.stopKinect();
            }
             */
        }

        // Currently unused method
        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            walkerKinect.printMap();
            angle += 45;
            Console.WriteLine("New Angle: " + angle );
            walkerKinect.setAngle(angle);
            //walkerKinect.stopKinect();
            //endProgram = true;
        }
    }
}
