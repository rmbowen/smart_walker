﻿using SmartWalker;
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

        // Settings for connecting to Arduino
        private const string portName = @"COM8";
        private const int baudRate = 9600;

        private int initialDirectionDegree;

        public NavigationCommand(int initialDirectionDegree)
        {
            this.initialDirectionDegree = initialDirectionDegree;
        }


        public void start()
        {
            // Start the Kinect Program Piece
            startKinect();

            port = COMConnection.COMConnection.Instance;

            int startingDegree = getAverageCurrentLocationInDegrees();

            int degreeDifference = getDegreeAddition(startingDegree, initialDirectionDegree);
            //int degreeDifference = startingDegree - initialDirectionDegree;

            Console.WriteLine("Degree IMU Needs to get to: " + degreeDifference);

            
            while (true)
            {
                if (walkerKinect.isBlocked())
                {

                    Console.WriteLine("START TURNING SLOWLY!");
                }
                else if (walkerKinect.isEmergency())
                {
                    Console.WriteLine("STOP, START TURNING!");
                }
                else
                {

                }
            }
            

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
            System.Threading.Thread.Sleep(200);

            int degree = int.Parse(port.readLineString());
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
            while (endProgram) { }
             * */
            Console.WriteLine("Kinect is Waiting:");
            /*if (Console.ReadLine() != null)
            {
                walkerKinect.printMap();
                walkerKinect.stopKinect();
            }
             * /
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            walkerKinect.printMap();
            walkerKinect.stopKinect();
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
