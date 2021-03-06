﻿//Author -- Steve Smith
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Diagnostics; 
namespace SmartWalker
{
    public class SmartWalkerKinect
    {
        // convert value from inches to millimeters
        const double walkerHeight = 39.0 * 2.54 * 10.0; // Height of the walker - 38 inches (added an inch to give walker more space
        const double walkerWidth = 27.0 * 2.54 * 10.0;// Width of the walker - 25 inches (added two inches to give walker more space)
        const double kinectHeight = 19.0 * 2.54 * 10.0; // Height of the Kinect camera from the ground - 19 inches

        //Distance is in millimeters
        const int obstacleThreshold = 1850;  // Distance when objects are considered obstacles
        const int mapThreshold = 2000;  // Distance up to which the Kinect will map
        const int emergencyThreshold = 1100; // Distance when objects are considered too close to safely go forward

        static KinectSensor sensor; // The Kinect

        int kinectUpCount = 0; // Counter for how many frames the Kinect will face up
        int kinectDownCount = 0; // Counter for how many frames the Kinect will face down

        // The map of the room. 0 = unknown, 1 = obstacle, 2 = free space, 3 = unknown behind obstacle
        public int[,] floorMap = new int[2000, 2000];

        // Shows which columns of the frame are blocked (0 = free, 1 = blocked), and the minimum obstacle depth in each column
        int[,] columnMins = new int[320, 2];
        int[,] columnMinsCopy = new int[320, 2];

        //NOTE: These are not in the code anywhere at the moment but they could be added where the map is altered instead of 
        // hard-coding the starting positions to the middle index values of the map
        double xStartIndex = 1000.0; // Starting X index in map (middle of map)
        double yStartIndex = 1000.0; // Starting Y index in map (middle of map)

        double angle = 0.0; // Current angle of walker in relation to starting angle
        double xPos = 0.0; // Current x position of walker in relation to starting x position
        double yPos = 0.0; // Current y position of walker in relation to starting y position

        double thetaHFull = 0.0; // Horizontal angle from left of view to current pixel (0 to 57 degrees)
        double thetaH = 0.0; // Horizontal angle from center to current pixel (0 to 28.5 degrees)
        double thetaV = 0.0;  // Vertical angle from center to current pixel (0 to 21.5 degrees)

        int FrameRateDivide = 2; // How much to divide the default 30 fps rate by
        int FrameRateCount = 1; // FrameRateDivide - 1, so that the first frame will be used
        Byte[] pixelsCopy; // This will replicate the most recent accepted frames in place of the frames that are dropped

        //These are used to save the 4 most previous frames, because something seen by the 
        // Kinect must be there for 4 frames to be considered an obstacle
        int frameCount = 1;
        int[,] frame1 = new int[320, 240];
        int[,] frame2 = new int[320, 240];
        int[,] frame3 = new int[320, 240];
        int[,] frame4 = new int[320, 240];

        bool leftBlocked = false; //Is an obstacle blocking the walker on the left side, but not at a critical distance
        bool rightBlocked = false; //Is an obstacle blocking the walker on the right side, but not at a critical distance
        bool allFramesInitialized = false; //Have the first four frames been initialized

        bool kinectIsUp = true; //Is the Kinect facing up?
        bool kinectisDown = false; //Is the Kinect facing down?
        bool kinectIsLevel = false; //Is the Kinect at its level position (+9 degrees)

        bool emergencyBlocked = false; //Is there a blocking obstacle within the emergency threshold?

        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        int levelPositionAngle = 9; //The angle that the Kinect will go to for normal drive mode
        double levelPositionAngleDouble = 9.0; //The angle that the Kinect will go to for normal drive mode

        public SmartWalkerKinect()
        {
            sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();
        }

        public void startKinect()
        {
            // Find the first connected sensor

            if (sensor == null)
            {
                Console.WriteLine("No Kinect sensor found!");
                return;
            }

            //turn on features that you need
            sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            sensor.DepthStream.Range = DepthRange.Near;
            //sign up for events if you want to get at API directly
            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);

            // Start the sensor
            sensor.Start();

        }

        public void stopKinect()
        {
            // Stop the sensor
            if (sensor.IsRunning)
            {
                //stop sensor 
                sensor.Stop();

                //stop audio if not null
                if (sensor.AudioSource != null)
                {
                    sensor.AudioSource.Stop();
                }


            }
        }

        //Automatically called whenever the Kinect finishes a frame
        public void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }
                // The variable 'pixels' is not actualy used for anything. It is left over from the original Depth Image Program from the Windows SDK
                byte[] pixels = GenerateColoredBytes(depthFrame);
            }
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            int rowCounter = 0; // Current Row in Frame
            int columnCounter = 0; // Current Column in Frame
            int columnCnt = 0; // Another column counter

            bool leftFrameBlocked = false; //Is there an obstacle on the left side of this frame?
            bool rightFrameBlocked = false;  //Is there an obstacle on the right side of this frame?
            bool emergencyFrameBlocked = false; //Is there an obstacle within a critical distance in this frame?

            int neighbor1, neighbor2, neighbor3 = 0; //Neighboring pixels
            int pastFrame1Pixel1, pastFrame1Pixel2, pastFrame1Pixel3, pastFrame1Pixel4; //Neighboring pixels from 1 frame ago
            int pastFrame2Pixel1, pastFrame2Pixel2, pastFrame2Pixel3, pastFrame2Pixel4; //Neighboring pixels from 2 frame ago
            int pastFrame3Pixel1, pastFrame3Pixel2, pastFrame3Pixel3, pastFrame3Pixel4; //Neighboring pixels from 3 frame ago


            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            FrameRateCount++;
            //If this is one of the frames to keep according to the frame rate, then keep it. Otherwise, the frame is dropped.
            if (FrameRateCount == FrameRateDivide)
            {
                // NOTE: This commented code can be used to tilt the Kinect up and run the camera for a certain number of frames,
                // tilt the Kinect down and run the camera for a certain number of frames, and the put the Kinect at its "level"
                // position before allowing the camera to run again
                /*
                if (kinectUpCount == 0)
                {
                    sensor.ElevationAngle = 27; //Set the Kinect to MaxElevation
                }

                //Keep the Kinect up for 75 frames
                if (kinectUpCount < 75)
                {
                    kinectUpCount++;
                }
                else
                {
                    if (kinectDownCount == 0)
                    {
                        kinectIsUp = false;
                        kinectisDown = true;
                        sensor.ElevationAngle = -27; //Set the kinect to MinElevation
                    }

                    //Keep the Kinect down for 75 frames
                    if (kinectDownCount < 75)
                    {
                        kinectDownCount++;
                    }
                    else if (kinectIsLevel == false)
                    {
                        kinectisDown = false;
                        sensor.ElevationAngle = levelPositionAngle; //Set the Kinect to 9 degrees, its level position
                        kinectIsLevel = true;
                    }
                }*/
                
                FrameRateCount = 0;

                //loop through all distances
                for (int depthIndex = 0, colorIndex = 0;
                    depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                    depthIndex++, colorIndex += 4)
                {
                    //get the player (requires skeleton tracking enabled for values)
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                    //gets the depth value
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    //Keep track of the four most recent frames
                    switch (frameCount)
                    {
                        case 1:
                            frame1[columnCounter, rowCounter] = depth;
                            break;
                        case 2:
                            frame2[columnCounter, rowCounter] = depth;
                            break;
                        case 3:
                            frame3[columnCounter, rowCounter] = depth;
                            break;
                        default:
                            frame4[columnCounter, rowCounter] = depth;
                            break;
                    }

                    columnCounter++;
                    // If at the end of a row
                    if (columnCounter >= 320)
                    {
                        columnCounter = 0;
                        if (rowCounter < 239)
                        {
                            rowCounter++;
                        }
                        //If at the end of a column (also at the end of the frame)
                        else
                        {
                            rowCounter = 0;
                            columnMinsCopy = columnMins;
                            columnMins = new int[320, 2];

                            //If the frame is blocked, set the appropriate global variables
                            if (leftFrameBlocked)
                            {
                                leftBlocked = true;
                            }
                            else
                            {
                                leftBlocked = false;
                            }
                            if (rightFrameBlocked)
                            {
                                rightBlocked = true;
                            }
                            else
                            {
                                rightBlocked = false;
                            }
                            if (emergencyFrameBlocked)
                            {
                                emergencyBlocked = true;
                            }
                            else
                            {
                                emergencyBlocked = false;
                            }

                            //Keep track of last four frames
                            if (frameCount == 4)
                            {
                                frameCount = 1;
                                allFramesInitialized = true;
                            }
                            else
                            {
                                frameCount++;
                            }
                        }
                    }

                    //Calculate the horizontal and vertical angles of the current pixel (USE RADIANS!!!)
                    thetaHFull = ((double)columnCounter / 320.0) * 57.0 / 180.0 * Math.PI;
                    thetaV = ((double)rowCounter / 240.0) * 43.0 / 180.0 * Math.PI;

                    //If the pixel is one the right side of the field of view
                    if (columnCounter >= 160)
                    {
                        thetaH = thetaHFull - (28.5 / 180.0 * Math.PI);
                    }
                    //If the pixel is one the left side of the field of view
                    else
                    {
                        thetaH = (28.5 / 180.0 * Math.PI) - thetaHFull;
                    }
                    //If the pixel is one the bottom half of the field of view
                    if (rowCounter >= 120)
                    {
                        thetaV -= 21.5 / 180.0 * Math.PI;
                    }
                    //If the pixel is one the top half of the field of view
                    else
                    {
                        thetaV = (21.5 / 180.0 * Math.PI) - thetaV;
                    }

                    //If the pixel is withing the emergency threshold
                    if (depth <= emergencyThreshold && depth >= 0 && allFramesInitialized)
                    {
                        //If the pixel is within the width of the walker
                        if ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0))
                        {
                            //If the pixel is within the height of the walker
                            if (((rowCounter < 120) && ((depth * Math.Sin((thetaV + (levelPositionAngleDouble / 180 * Math.PI)))) < (walkerHeight - kinectHeight))) || (rowCounter >= 120))
                            {
                                if (columnCounter != 0 && rowCounter != 0)
                                {
                                    //Set the nieghboring pixels and the corresponding pixels of the four most recent frames, including this one
                                    switch (frameCount)
                                    {
                                        case 1:
                                            neighbor1 = frame1[columnCounter - 1, rowCounter];
                                            neighbor2 = frame1[columnCounter, rowCounter - 1];
                                            neighbor3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame4[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame3[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame2[columnCounter, rowCounter];
                                            break;
                                        case 2:
                                            neighbor1 = frame2[columnCounter - 1, rowCounter];
                                            neighbor2 = frame2[columnCounter, rowCounter - 1];
                                            neighbor3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame1[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame4[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame3[columnCounter, rowCounter];
                                            break;
                                        case 3:
                                            neighbor1 = frame3[columnCounter - 1, rowCounter];
                                            neighbor2 = frame3[columnCounter, rowCounter - 1];
                                            neighbor3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame2[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame1[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame4[columnCounter, rowCounter];
                                            break;
                                        default:
                                            neighbor1 = frame4[columnCounter - 1, rowCounter];
                                            neighbor2 = frame4[columnCounter, rowCounter - 1];
                                            neighbor3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame3[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame2[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame1[columnCounter, rowCounter];
                                            break;
                                    }

                                    if (neighbor1 <= obstacleThreshold && neighbor1 >= 0)
                                    {
                                        if (neighbor2 <= obstacleThreshold && neighbor2 >= 0)
                                        {
                                            if (neighbor3 <= obstacleThreshold && neighbor3 >= 0)
                                            {
                                                if (pastFrame1Pixel1 <= obstacleThreshold && pastFrame1Pixel1 >= 0)
                                                {
                                                    if (pastFrame1Pixel2 <= obstacleThreshold && pastFrame1Pixel2 >= 0)
                                                    {
                                                        if (pastFrame1Pixel3 <= obstacleThreshold && pastFrame1Pixel3 >= 0)
                                                        {
                                                            if (pastFrame1Pixel4 <= obstacleThreshold && pastFrame1Pixel4 >= 0)
                                                            {
                                                                if (pastFrame2Pixel1 <= obstacleThreshold && pastFrame2Pixel1 >= 0)
                                                                {
                                                                    if (pastFrame2Pixel2 <= obstacleThreshold && pastFrame2Pixel2 >= 0)
                                                                    {
                                                                        if (pastFrame2Pixel3 <= obstacleThreshold && pastFrame2Pixel3 >= 0)
                                                                        {
                                                                            if (pastFrame2Pixel4 <= obstacleThreshold && pastFrame2Pixel4 >= 0)
                                                                            {
                                                                                if (pastFrame3Pixel1 <= obstacleThreshold && pastFrame3Pixel1 >= 0)
                                                                                {
                                                                                    if (pastFrame3Pixel2 <= obstacleThreshold && pastFrame3Pixel2 >= 0)
                                                                                    {
                                                                                        if (pastFrame3Pixel3 <= obstacleThreshold && pastFrame3Pixel3 >= 0)
                                                                                        {
                                                                                            if (pastFrame3Pixel4 <= obstacleThreshold && pastFrame3Pixel4 >= 0)
                                                                                            {
                                                                                                //If all of the neighboring pixels and corresponding pixels from previous
                                                                                                // frames are also within the emergency threshold, we have found an obstacle
                                                                                                emergencyFrameBlocked = true;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }

                    //If the pixel is within the normal threshold
                    else if (depth > emergencyThreshold && depth < obstacleThreshold)
                    {
                        if ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0))
                        {
                            if (((rowCounter < 120) && ((depth * Math.Sin((thetaV + (levelPositionAngleDouble / 180 * Math.PI)))) < (walkerHeight - kinectHeight))) || (rowCounter >= 120))
                            {
                                if (columnCounter != 0 && rowCounter != 0)
                                {
                                    switch (frameCount)
                                    {
                                        case 1:
                                            neighbor1 = frame1[columnCounter - 1, rowCounter];
                                            neighbor2 = frame1[columnCounter, rowCounter - 1];
                                            neighbor3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame4[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame3[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame2[columnCounter, rowCounter];
                                            break;
                                        case 2:
                                            neighbor1 = frame2[columnCounter - 1, rowCounter];
                                            neighbor2 = frame2[columnCounter, rowCounter - 1];
                                            neighbor3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame1[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame4[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame3[columnCounter, rowCounter];
                                            break;
                                        case 3:
                                            neighbor1 = frame3[columnCounter - 1, rowCounter];
                                            neighbor2 = frame3[columnCounter, rowCounter - 1];
                                            neighbor3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame2[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame1[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame4[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame4[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame4[columnCounter, rowCounter];
                                            break;
                                        default:
                                            neighbor1 = frame4[columnCounter - 1, rowCounter];
                                            neighbor2 = frame4[columnCounter, rowCounter - 1];
                                            neighbor3 = frame4[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel1 = frame3[columnCounter - 1, rowCounter];
                                            pastFrame1Pixel2 = frame3[columnCounter, rowCounter - 1];
                                            pastFrame1Pixel3 = frame3[columnCounter - 1, rowCounter - 1];
                                            pastFrame1Pixel4 = frame3[columnCounter, rowCounter];
                                            pastFrame2Pixel1 = frame2[columnCounter - 1, rowCounter];
                                            pastFrame2Pixel2 = frame2[columnCounter, rowCounter - 1];
                                            pastFrame2Pixel3 = frame2[columnCounter - 1, rowCounter - 1];
                                            pastFrame2Pixel4 = frame2[columnCounter, rowCounter];
                                            pastFrame3Pixel1 = frame1[columnCounter - 1, rowCounter];
                                            pastFrame3Pixel2 = frame1[columnCounter, rowCounter - 1];
                                            pastFrame3Pixel3 = frame1[columnCounter - 1, rowCounter - 1];
                                            pastFrame3Pixel4 = frame1[columnCounter, rowCounter];
                                            break;
                                    }

                                    if (neighbor1 <= obstacleThreshold && neighbor1 >= 0)
                                    {
                                        if (neighbor2 <= obstacleThreshold && neighbor2 >= 0)
                                        {
                                            if (neighbor3 <= obstacleThreshold && neighbor3 >= 0)
                                            {
                                                if (pastFrame1Pixel1 <= obstacleThreshold && pastFrame1Pixel1 >= 0)
                                                {
                                                    if (pastFrame1Pixel2 <= obstacleThreshold && pastFrame1Pixel2 >= 0)
                                                    {
                                                        if (pastFrame1Pixel3 <= obstacleThreshold && pastFrame1Pixel3 >= 0)
                                                        {
                                                            if (pastFrame1Pixel4 <= obstacleThreshold && pastFrame1Pixel4 >= 0)
                                                            {
                                                                if (pastFrame2Pixel1 <= obstacleThreshold && pastFrame2Pixel1 >= 0)
                                                                {
                                                                    if (pastFrame2Pixel2 <= obstacleThreshold && pastFrame2Pixel2 >= 0)
                                                                    {
                                                                        if (pastFrame2Pixel3 <= obstacleThreshold && pastFrame2Pixel3 >= 0)
                                                                        {
                                                                            if (pastFrame2Pixel4 <= obstacleThreshold && pastFrame2Pixel4 >= 0)
                                                                            {
                                                                                if (pastFrame3Pixel1 <= obstacleThreshold && pastFrame3Pixel1 >= 0)
                                                                                {
                                                                                    if (pastFrame3Pixel2 <= obstacleThreshold && pastFrame3Pixel2 >= 0)
                                                                                    {
                                                                                        if (pastFrame3Pixel3 <= obstacleThreshold && pastFrame3Pixel3 >= 0)
                                                                                        {
                                                                                            if (pastFrame3Pixel4 <= obstacleThreshold && pastFrame3Pixel4 >= 0)
                                                                                            {
                                                                                                if (columnCounter < 160)
                                                                                                {
                                                                                                    leftFrameBlocked = true;
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    rightFrameBlocked = true;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    //Calculate the X and Y position of the Kinect in the map
                    double xMapTemp = 1000.0 + xPos + (Math.Sin(angle + thetaHFull) * (depth / 20.0));
                    int xMap = (int)xMapTemp;
                    double yMapTemp = 1000.0 + yPos + (Math.Cos(angle + thetaHFull) * (depth / 20.0));
                    int yMap = (int)yMapTemp;

                    int depthCounter = 0; //counter variable

                    //If the current pixel is within the mapping threshold
                    if (depth <= mapThreshold && depth >= 0 && allFramesInitialized)
                    {
                        floorMap[xMap, yMap] = 1; //Set as an obstacle
                        if (columnMins[columnCounter, 0] == 0)
                        {
                            columnMins[columnCounter, 0] = 1; //Say that there is an obstacle in this column
                            columnMins[columnCounter, 1] = depth; //Set the minimum obstacle distance for this column
                        }
                        else if (depth < columnMins[columnCounter, 1])
                        {
                            columnMins[columnCounter, 1] = depth; //Set the minimum obstacle distance for this column
                        }
                    }

                    //If we have reached the end of the frame
                    if (rowCounter == 239 && columnCounter == 319 && allFramesInitialized)
                    {
                        //For each column in the field of view
                        for (columnCnt = 0; columnCnt < 320; columnCnt++)
                        {
                            //For each depth in the current column (each possible map coordinate)
                            for (depthCounter = 0; depthCounter < mapThreshold; depthCounter += 20)
                            {
                                double thetaHFullLoop = ((double)columnCnt / 320.0) * 57.0 / 180.0 * Math.PI; //Angle of this depth and column

                                //X and Y coordinate in the map fro this depth and column
                                double xMapTempLoop = 1000.0 + xPos + (Math.Sin(angle + thetaHFullLoop) * (depthCounter / 20.0));
                                int xMapLoop = (int)xMapTempLoop;
                                double yMapTempLoop = 1000.0 + yPos + (Math.Cos(angle + thetaHFullLoop) * (depthCounter / 20.0));
                                int yMapLoop = (int)yMapTempLoop;

                                //If the current column has an obstacle in it
                                if (columnMins[columnCnt, 0] == 1)
                                {
                                    //If the current depth is shorter than the minimum obstacle depth, it is free space
                                    if ((depthCounter / 20) < (columnMins[columnCnt, 1] / 20))
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 2;
                                    }
                                    //If the current depth is the minimum obstacle depth, it is an obstacle
                                    else if ((depthCounter / 20) == (columnMins[columnCnt, 1] / 20))
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 1;
                                    }
                                    else
                                    {
                                        //If the current location is unknown, mark it as unknown space behind an object
                                        if (floorMap[xMapLoop, yMapLoop] == 0)
                                        {
                                            floorMap[xMapLoop, yMapLoop] = 3;
                                        }
                                    }
                                }
                                else
                                {
                                    //If the current location is not unknown space behind an object, mark it as free space
                                    if (floorMap[xMapLoop, yMapLoop] != 3)
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 2;
                                    }
                                }
                            }
                        }
                    }
                }
                pixelsCopy = pixels; //Make a copy of the current frame to replace the subsequent frames that will be dropped
                return pixels;
            }

            //Frame was dropped, use the pixels that were copied from a valid frame
            if (pixelsCopy != null)
            {
                return pixelsCopy;
            }
            return pixels;
        }

        // Return whether or not there is an obstacle in the way fo the walker
        public bool isBlocked()
        {
            return (leftBlocked || rightBlocked);
        }

        // Return whether or not there is an obstacle within critical distance in front of the walker
        public bool isEmergency()
        {
            return emergencyBlocked;
        }

        // Set the angle of the walker
        public void setAngle(double newAngle)
        {
            angle = (newAngle / 180) * Math.PI;
        }

        //Set the x position of the walker
        public void setXPos(double newXPos)
        {
            xPos = newXPos;
        }

        //Set the y position of the walker
        public void setYPos(double newYPos)
        {
            yPos = newYPos;
        }

        // Return if the Kinect is at its level position
        public bool isKinectLevel()
        {
            return kinectIsLevel;
        }

        //This method returns an integer that corresponds to which direction the walker should travel next
        //If the center 160 columns are unblocked, continue straight
        //If the center is blocked and the left 80 columns( minus the 7 edge pixels, to reduce false obstacles from noise) are free, turn left 
        //If the center is blocked and the left is blocked, and the right 80 columns( minus the 7 edge pixels, to reduce false obstacles from noise) are free, turn right
        //Otherwise, the walker has no where to go and it should swivel
        //
        //NOTE: The name of the method is outdated, because it no longer decides between left and right
        public int isRightTurnBetter()
        {
            int i = 0;
            bool centerIsBlocked = false;
            bool leftIsBlocked = false;
            bool rightIsBlocked = false;

            for (i = 0; i < 320; i++)
            {
                if (columnMinsCopy[i, 0] == 1)
                {
                    if (i < 80 && i > 7)
                    {
                        //If within 1000mm
                        if (columnMinsCopy[i,1] < 1000){
                            rightIsBlocked = true;
                        }
                    }
                    else if (i < 240)
                    {
                        //If within 1000mm
                        if (columnMinsCopy[i, 1] < 1000)
                        {
                            centerIsBlocked = true;
                        }
                    }
                    else if (i < 312)
                    {
                        //If within 1000mm
                        if (columnMinsCopy[i,1] < 1000){
                            leftIsBlocked = true;
                        }
                    }
                }
            }
            if (!centerIsBlocked)
            {
                return 1; // Go Straight
            }
            else if (!leftIsBlocked)
            {
                return 2; // Go left
            }
            else if (!rightIsBlocked)
            {
                return 3; // Go right
            }
            else
            {
                return 4; // Swivel
            }
        }

        //Print the map to a file
        public void printMap()
        {
            string line = "";

            // Loop counters
            int ii;
            int jj;
            int hh;
            int ww;

            //The top-most, bottom-most, left-most, and right-most pixels that are not unknown in the map
            int topPixel = -1;
            int bottomPixel = -1;
            int leftPixel = -1;
            int rightPixel = -1;

            // Find the boundary pixels
            for (jj = 0; jj < 2000; jj++)
            {
                for (ii = 0; ii < 2000; ii++)
                {
                    if (floorMap[ii, jj] != 0)
                    {
                        if (topPixel == -1)
                        {
                            topPixel = jj;
                        }
                        bottomPixel = jj;
                        if (leftPixel == -1)
                        {
                            leftPixel = ii;
                        }
                        else if (ii < leftPixel)
                        {
                            leftPixel = ii;
                        }
                        if (rightPixel == -1)
                        {
                            rightPixel = ii;
                        }
                        else if (ii > rightPixel)
                        {
                            rightPixel = ii;
                        }
                    }
                }
            }

            // Write map to file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\Development\mapTest1.txt"))
            {
                if ((topPixel == bottomPixel) || (leftPixel == rightPixel) || (leftPixel == -1) || (rightPixel == -1) || (topPixel == -1) || (bottomPixel == -1))
                {
                    // If the boundary pixels are invalid, print error message to the file
                    line = "Invalid map";
                    file.WriteLine(line);
                }
                else
                {
                    //For each column
                    for (ww = 0; ww < 2000; ww++)
                    {
                        //For each row
                        for (hh = 0; hh < 2000; hh++)
                        {
                            //If the current pixel is supposed to be mapped
                            if (hh >= topPixel && hh <= bottomPixel && ww >= leftPixel && ww <= rightPixel)
                            {
                                switch (floorMap[ww, hh])
                                {
                                    case 0:
                                        line += "|"; //Unknown
                                        break;
                                    case 1:
                                        line += "0"; //Obstacle
                                        break;
                                    case 2:
                                        line += " "; //Open space
                                        break;
                                    default:
                                        line += "|"; //Unknown, behind a visible obstacle
                                        break;
                                }
                            }
                        }
                        if (line != "")
                        {
                            file.WriteLine(line);
                            line = "";
                        }
                    }
                }
            }
            //Write the map to a file again, just print it in a different orientation this time
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\Development\mapTest2.txt"))
            {
                if ((topPixel == bottomPixel) || (leftPixel == rightPixel) || (leftPixel == -1) || (rightPixel == -1) || (topPixel == -1) || (bottomPixel == -1))
                {
                    line = "Invalid map";
                    file.WriteLine(line);
                }
                else
                {
                    for (ww = 0; ww < 2000; ww++)
                    {
                        for (hh = 0; hh < 2000; hh++)
                        {
                            if (hh >= topPixel && hh <= bottomPixel && ww >= leftPixel && ww <= rightPixel)
                            {
                                switch (floorMap[ww, hh])
                                {
                                    case 0:
                                        line += "|";
                                        break;
                                    case 1:
                                        line += "0";
                                        break;
                                    case 2:
                                        line += " ";
                                        break;
                                    default:
                                        line += "|";
                                        break;
                                }
                            }
                        }
                        if (line != "")
                        {
                            file.WriteLine(line);
                            line = "";
                        }
                    }
                }
            }
        }

    }
}
