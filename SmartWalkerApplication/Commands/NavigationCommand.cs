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

        private string myString = "51010";

        // Settings for connecting to Arduino
        private const string portName = @"COM8";
        private const int baudRate = 9600;

        private double angle = 0.0;

        private int initialDirectionDegree;

        public NavigationCommand(int initialDirectionDegree)
        {
            this.initialDirectionDegree = initialDirectionDegree;
        }

        private bool withinDegrees(int initialReading, int currentReading)
        {
            if ((currentReading <= initialReading + 7) && (currentReading >= initialReading - 7))
            {
                return true;
            }
            return false;
        }


        public void start()
        {
            // Start the Kinect Program Piece
            startKinect();

            port = COMConnection.COMConnection.Instance;
            
            
            int startingDegree = getAverageCurrentLocationInDegrees();
            int degreeDifference = getDegreeAddition(startingDegree, initialDirectionDegree);

            Console.WriteLine("Degree IMU Needs to get to: " + degreeDifference);
            
            // Start dat swivel

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
            
            while (!(withinDegrees(degreeDifference, startingDegree)))
            {
                startingDegree = getAverageCurrentLocationInDegrees();
                Console.WriteLine("New Degree: " + startingDegree);
            }
            
            // Stop, wait 1 second, start moving
            port.sendString("N");
            port.sendString("51010");
            System.Threading.Thread.Sleep(500); // Let the wheels get going
            
            // Go Forward
            
            //port.sendString("N");
            //port.sendString("11010");
            //System.Threading.Thread.Sleep(500);

            while (true)
            {
                //System.Threading.Thread.Sleep(5000);
                // send mode
                //port.sendString(Console.ReadLine());

                port.sendString("N");

                //System.Threading.Thread.Sleep(500);

               // string myString = Console.ReadLine();
                // send right ticks
               // port.sendString(Console.ReadLine());

                // send left ticks
                //port.sendString(Console.ReadLine());
                
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
                            swivelRight();
                            break;
                    }
                    //if (walkerKinect.isRightTurnBetter())
                    //{
                    //    Console.WriteLine("Stop, start turning RIGHT!");

                    //    pivotRight();
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Stop, start turning Left!");

                    //    pivotLeft();
                    //}
                    //goStraight();

                }
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
                            swivelRight();
                            break;
                    }
                    //if (walkerKinect.isRightTurnBetter())
                    //{
                    //    Console.WriteLine("Start turning slowly RIGHT!");
                    //    glideRight();
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Start turning slowly LEFT!");

                    //    glideLeft();
                    //}

                }
                else
                {

                    Console.WriteLine("KEEP GOING STRAIGHT!");
                    goStraight();
                }
                
                port.sendString(myString);
                System.Threading.Thread.Sleep(500);

            }
            
            //while (true)
            //{
            //    if (walkerKinect.isEmergency())
            //    {
            //        Console.WriteLine("STOP, START TURNING!");
            //        swivelRight();

            //    }
            //    else if (walkerKinect.isBlocked())
            //    {
            //        Console.WriteLine("START TURNING SLOWLY!");
            //        turnRight();
            //    }
            //    else
            //    {
            //        goStraight();
            //    }
            //}
            

         /*   if (!SmartWalkerKinect.isBlocked())
            {

            }
            */
            
            // Send data to IMU?
            //Console.WriteLine("Send:");

            //port.WriteLine("L");
            //port.WriteLine("1100");

            /*
            for (; ; )
            {
                Console.WriteLine(" ");
                Console.WriteLine("> ");
                port.WriteLine(Console.ReadLine());
            }
             * */
            port.closeConnection();
            //port.Close();
        }

        private void swivelRight()
        {
            myString = "30707";
        }

        private void pivotRight()
        {
            myString = "11305";
        }

        private void glideRight()
        {
            //Stop motors
            myString = "11307";

        }

        private void pivotLeft()
        {
            myString = "10512";
        }

        private void glideLeft()
        {
            //Stop motors
            myString = "10712";

        }

        private void goStraight()
        {
            myString = "10909";
        }

        private void goStraightSlowly()
        {
            myString = "10707";
        } 

        private int getDegreeAddition(int startDegree, int endDegree)
        {
            int result = startDegree + endDegree;
            if (result > 360)
            {
                result = result - 360;
            }
            return result;
        }

        private int getAverageCurrentLocationInDegrees()
        {
            int startingDegree1 = getCurrentLocationInDegrees();
            int startingDegree2 = getCurrentLocationInDegrees();
            int startingDegree3 = getCurrentLocationInDegrees();

            return ((startingDegree1 + startingDegree2 + startingDegree3) / 3);

        }

        private int getCurrentLocationInDegrees()
        {
            port.sendString("D");
             //Delay a bit for the serial to catch up
            System.Threading.Thread.Sleep(100);
            string degreeString = port.readLineString();
            Console.WriteLine("Recieved Value: " + degreeString);
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
            Console.WriteLine("Kinect is Waiting:");
            /*if (Console.ReadLine() != null)
            {
                walkerKinect.printMap();
                walkerKinect.stopKinect();
            }
             */
        }

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

        // Function to get the baud rate
        static int GetBaudRate()
        {
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Invalid integer.  Please try again:");
                return GetBaudRate();
            }
        }
    }
}
