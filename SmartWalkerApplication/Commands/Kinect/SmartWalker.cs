using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Diagnostics; 

namespace SmartWalker
{
    public class SmartWalkerKinect
    {
        const double walkerHeight = 38.0 * 2.54 * 10.0; // convert value from inches to millimeters
        const double walkerWidth = 27.0 * 2.54 * 10.0;
        const double kinectHeight = 19.0 * 2.54 * 10.0;
        const int obstacleThreshold = 1850;
        const int mapThreshold = 2000;
        const int emergencyThreshold = 1100;

        static KinectSensor sensor;

        int kinectUpCount = 0;
        int kinectDownCount = 0;

        public int[,] floorMap = new int[1000, 1000];
        int[,] columnMins = new int[320, 2];

        int xStartIndex = 500;
        int yStartIndex = 500;

        double angle = 0.0;
        double xPos = 0.0;
        double yPos = 0.0;

        double thetaHFull = 0.0;
        double thetaH = 0.0;
        double thetaV = 0.0;

        int FrameRateDivide = 3;
        int FrameRateCount = 2;

        int frameCount = 1;
        int[,] frame1 = new int[320, 240];
        int[,] frame2 = new int[320, 240];
        int[,] frame3 = new int[320, 240];
        int[,] frame4 = new int[320, 240];

        Byte[] pixelsCopy;

        bool leftBlocked = false;
        bool rightBlocked = false;
        bool allFramesInitialized = false;
        bool kinectIsUp = true;
        bool kinectisDown = false;
        bool kinectIsLevel = false;

        bool emergencyBlocked = false;

        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        const double pi = 3.141593;

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
            sensor.SkeletonStream.Enable();
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

        public void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            //int frameCount = 1;
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                byte[] pixels = GenerateColoredBytes(depthFrame);

                //number of bytes per row width * 4 (B,G,R,Empty)
                int stride = depthFrame.Width * 4;
            }
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            int rowCounter = 0;
            int columnCounter = 0;
            int columnCnt = 0;

            bool leftFrameBlocked = false;
            bool rightFrameBlocked = false;
            bool emergencyFrameBlocked = false;

            int neighbor1, neighbor2, neighbor3 = 0;
            int pastFrame1Pixel1, pastFrame1Pixel2, pastFrame1Pixel3, pastFrame1Pixel4;
            int pastFrame2Pixel1, pastFrame2Pixel2, pastFrame2Pixel3, pastFrame2Pixel4;
            int pastFrame3Pixel1, pastFrame3Pixel2, pastFrame3Pixel3, pastFrame3Pixel4;


            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            FrameRateCount++;
            if (FrameRateCount == FrameRateDivide)
            {
                /*
                if (kinectUpCount == 0)
                {
                    sensor.ElevationAngle = 27;
                }

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
                        sensor.ElevationAngle = -27;
                    }

                    if (kinectDownCount < 75)
                    {
                        kinectDownCount++;
                    }
                    else if (kinectIsLevel == false)
                    {
                        kinectisDown = false;
                        sensor.ElevationAngle = 9;
                        kinectIsLevel = true;
                    }
                }
             */
                FrameRateCount = 0;

                //Bgr32  - Blue, Green, Red, empty byte
                //Bgra32 - Blue, Green, Red, transparency 
                //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

                //hardcoded locations to Blue, Green, Red (BGR) index positions       
                const int BlueIndex = 0;
                const int GreenIndex = 1;
                const int RedIndex = 2;

                //loop through all distances
                //pick a RGB color based on distance
                for (int depthIndex = 0, colorIndex = 0;
                    depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                    depthIndex++, colorIndex += 4)
                {
                    //get the player (requires skeleton tracking enabled for values)
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                    //gets the depth value
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

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
                    if (columnCounter >= 320)
                    {
                        columnCounter = 0;
                        if (rowCounter < 239)
                        {
                            rowCounter++;
                        }
                        else
                        {
                            rowCounter = 0;

                            //if (angle != localAngle || xPos != localxPos || yPos != localyPos)
                            //{
                            columnMins = new int[320, 2];
                            //    localAngle = angle;
                            //    localxPos = xPos;
                            //    localyPos = yPos;
                            //}
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


                    thetaHFull = ((double)columnCounter / 320.0) * 57.0 / 180.0 * pi;
                    thetaV = ((double)rowCounter / 240.0) * 43.0 / 180.0 * pi;

                    if (columnCounter >= 160)
                    {
                        thetaH = thetaHFull - (28.5 / 180.0 * pi);
                    }
                    else
                    {
                        thetaH = (28.5 / 180.0 * pi) - thetaHFull;
                    }
                    if (rowCounter >= 120)
                    {
                        thetaV -= 21.5 / 180.0 * pi;
                    }
                    else
                    {
                        thetaV = (21.5 / 180.0 * pi) - thetaV;
                    }

                    if (kinectIsUp || kinectisDown)
                    {
                        thetaV += 21.5;
                    }

                    //.9M or 2.95'
                    if (depth <= emergencyThreshold && depth >= 0 && allFramesInitialized)
                    {
                        double temp = Math.Sin(thetaH);
                        double temp2 = temp * depth;
                        if ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0))
                        {
                            //double temp = Math.Sin(thetaH);
                            //double temp2 = temp * depth;
                            if (((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))) || ((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight))))
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

                        if (((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))))
                        {
                            double value1 = depth * Math.Sin(thetaV);
                        }
                        if (((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight))))
                        {
                            double value2 = depth * Math.Sin(thetaV);
                        }

                        //we are very close
                        if ((((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))) || ((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight - 19.0)))) && ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0)))
                        {
                            pixels[colorIndex + BlueIndex] = 255;
                            pixels[colorIndex + GreenIndex] = 0;
                            pixels[colorIndex + RedIndex] = 0;
                        }
                        else
                        {
                            pixels[colorIndex + BlueIndex] = 255;
                            pixels[colorIndex + GreenIndex] = 255;
                            pixels[colorIndex + RedIndex] = 0;
                        }

                    }


                    else if (depth > emergencyThreshold && depth < obstacleThreshold)
                    {
                        double temp = Math.Sin(thetaH);
                        double temp2 = temp * depth;
                        if ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0))
                        {
                            //double temp = Math.Sin(thetaH);
                            //double temp2 = temp * depth;
                            if (((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))) || ((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight))))
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

                        if (((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))))
                        {
                            double value1 = depth * Math.Sin(thetaV);
                        }
                        if (((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight))))
                        {
                            double value2 = depth * Math.Sin(thetaV);
                        }

                        //we are very close
                        if ((((rowCounter < 120) && ((depth * Math.Sin(thetaV)) < (walkerHeight - kinectHeight))) || ((rowCounter >= 120) && ((depth * Math.Sin(thetaV)) < (kinectHeight - 19.0)))) && ((depth * Math.Sin(thetaH)) < (walkerWidth / 2.0)))
                        {
                            pixels[colorIndex + BlueIndex] = 0;
                            pixels[colorIndex + GreenIndex] = 255;
                            pixels[colorIndex + RedIndex] = 255;
                        }
                        else
                        {
                            pixels[colorIndex + BlueIndex] = 50;
                            pixels[colorIndex + GreenIndex] = 150;
                            pixels[colorIndex + RedIndex] = 100;
                        }
                    }
                    //else if (depth < 0)
                    //{
                    //    currentRow += "_";
                    //}

                    // .9M - 2M or 2.95' - 6.56'
                    else if (depth > obstacleThreshold && depth < 2000)
                    {
                        //we are a bit further away
                        if (columnCounter < 160)
                        {
                            if (!leftBlocked)
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 255;
                                pixels[colorIndex + RedIndex] = 0;
                            }
                            else
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 100;
                                pixels[colorIndex + RedIndex] = 0;
                            }
                        }

                        if (columnCounter >= 160)
                        {
                            if (!rightBlocked)
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 255;
                                pixels[colorIndex + RedIndex] = 0;
                            }
                            else
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 100;
                                pixels[colorIndex + RedIndex] = 0;
                            }
                        }
                    }
                    // 2M+ or 6.56'+
                    else if (depth > 2000)
                    {
                        //we are the farthest
                        if (columnCounter < 160)
                        {
                            if (!leftBlocked)
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 0;
                                pixels[colorIndex + RedIndex] = 255;
                            }
                            else
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 0;
                                pixels[colorIndex + RedIndex] = 100;
                            }
                        }

                        if (columnCounter >= 160)
                        {
                            if (!rightBlocked)
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 0;
                                pixels[colorIndex + RedIndex] = 255;
                            }
                            else
                            {
                                pixels[colorIndex + BlueIndex] = 0;
                                pixels[colorIndex + GreenIndex] = 0;
                                pixels[colorIndex + RedIndex] = 100;
                            }
                        }
                    }
                    double sinThetaH = Math.Sin(thetaHFull);
                    double cosThetaH = Math.Cos(thetaHFull);

                    double xMapTemp = 500.0 + xPos + (Math.Sin(angle + thetaHFull) * (depth / 20.0));
                    int xMap = (int)xMapTemp;
                    double yMapTemp = 500.0 + yPos + (Math.Cos(angle + thetaHFull) * (depth / 20.0));
                    int yMap = (int)yMapTemp;

                    int depthCounter = 0;

                    //2.0 meters
                    if (depth <= mapThreshold && depth >= 0 && allFramesInitialized)
                    {
                        floorMap[xMap, yMap] = 1;
                        if (columnMins[columnCounter, 0] == 0)
                        {
                            columnMins[columnCounter, 0] = 1;
                            columnMins[columnCounter, 1] = depth;
                        }
                        else if (depth < columnMins[columnCounter, 1])
                        {
                            columnMins[columnCounter, 1] = depth;
                        }
                    }

                    if (rowCounter == 239 && columnCounter == 319 && allFramesInitialized)
                    {
                        for (columnCnt = 0; columnCnt < 320; columnCnt++)
                        {
                            for (depthCounter = 0; depthCounter < mapThreshold; depthCounter += 20)
                            {
                                double thetaHFullLoop = ((double)columnCnt / 320.0) * 57.0 / 180.0 * pi;

                                double xMapTempLoop = 500.0 + xPos + (Math.Sin(angle + thetaHFullLoop) * (depthCounter / 20.0));
                                int xMapLoop = (int)xMapTempLoop;
                                double yMapTempLoop = 500.0 + yPos + (Math.Cos(angle + thetaHFullLoop) * (depthCounter / 20.0));
                                int yMapLoop = (int)yMapTempLoop;

                                if (columnMins[columnCnt, 0] == 1)
                                {
                                    if ((depthCounter / 20) < (columnMins[columnCnt, 1] / 20))
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 2;
                                    }
                                    else if ((depthCounter / 20) == (columnMins[columnCnt, 1] / 20))
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 1;
                                    }
                                    else
                                    {
                                        if (floorMap[xMapLoop, yMapLoop] == 0)
                                        {
                                            floorMap[xMapLoop, yMapLoop] = 3;
                                        }
                                    }
                                }
                                else
                                {
                                    if (floorMap[xMapLoop, yMapLoop] != 3)
                                    {
                                        floorMap[xMapLoop, yMapLoop] = 2;
                                    }
                                }
                            }
                        }
                    }
                    ////equal coloring for monochromatic histogram
                    //byte intensity = CalculateIntensityFromDepth(depth);
                    //pixels[colorIndex + BlueIndex] = intensity;
                    //pixels[colorIndex + GreenIndex] = intensity;
                    //pixels[colorIndex + RedIndex] = intensity;

                }
                pixelsCopy = pixels;
                return pixels;
            }

            if (pixelsCopy != null)
            {
                return pixelsCopy;
            }
            return pixels;
        }

        public bool isBlocked()
        {
            return (leftBlocked || rightBlocked);
        }

        public bool isEmergency()
        {
            return emergencyBlocked;
        }

        public void setAngle(double newAngle)
        {
            angle = (newAngle / 180) * Math.PI;
        }

        public bool isKinectLevel()
        {
            return kinectIsLevel;
        }

        public bool isRightTurnBetter()
        {
            int i = 0;
            int rightCount = 0;
            int leftCount = 0;

            for (i = 0; i < 320; i++)
            {
                if (columnMins[i, 0] == 1)
                {
                    if (i < 160)
                    {
                        leftCount++;
                    }
                    else
                    {
                        rightCount++;
                    }
                }
            }

            if (leftCount > rightCount)
            {
                return false;
            }
            return true;
        }

        public void printMap()
        {
            string line = "";
            int ii;
            int jj;
            int hh;
            int ww;
            int topPixel = -1;
            int bottomPixel = -1;
            int leftPixel = -1;
            int rightPixel = -1;

            //System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\Users\\Public\\Desktop\\map.txt");


            for (jj = 0; jj < 1000; jj++)
            {
                for (ii = 0; ii < 1000; ii++)
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

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\tjd9961\Desktop\Kinect\mapTest1.txt"))
            {
                if ((topPixel == bottomPixel) || (leftPixel == rightPixel) || (leftPixel == -1) || (rightPixel == -1) || (topPixel == -1) || (bottomPixel == -1))
                {
                    line = "Invalid map";
                    file.WriteLine(line);
                }
                else
                {
                    for (hh = 0; hh < 1000; hh++)
                    {
                        for (ww = 0; ww < 1000; ww++)
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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\tjd9961\Desktop\Kinect\mapTest2.txt"))
            {
                if ((topPixel == bottomPixel) || (leftPixel == rightPixel) || (leftPixel == -1) || (rightPixel == -1) || (topPixel == -1) || (bottomPixel == -1))
                {
                    line = "Invalid map";
                    file.WriteLine(line);
                }
                else
                {
                    for (ww = 0; ww < 1000; ww++)
                    {
                        for (hh = 0; hh < 1000; hh++)
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
