using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Drawing;
using AForge.Video.FFMPEG;


namespace MotionDetection
{
    public class VideoAnalyser
    {
        public static List<double> GetByteArrayDifferencesAlternative(string fileName, Action<int> progress)
        {

            var listToReturn = new List<double>();
            byte[] previous = new byte[] { };
            var videoFileReader = new AForge.Video.FFMPEG.VideoFileReader();
            

            videoFileReader.Open(fileName);
            Console.WriteLine("frameCount: " + videoFileReader.FrameCount);
            var fps = videoFileReader.FrameRate;

            var totalSecondsGuess = (double)videoFileReader.FrameCount / (double)videoFileReader.FrameRate;
            var sequentialErrors = 0;
            for (int i = 0; i < videoFileReader.FrameCount; i++)
            {
               

                var skipErrorCode = videoFileReader.ReadVideoFrameAndSkipDecode(i);
                if (skipErrorCode == 0)
                {
                    sequentialErrors = 0;
                    var frame = videoFileReader.ReadVideoFrame();
                    var byteArray = GetBigMapArray(frame, i);
                    if (i > 0)
                    {
                        var avDis = GetAverageDifferenceFromPrevious(byteArray, previous, 20);
                        listToReturn.Add(avDis);
                    }
                    previous = byteArray;
                    

                    var fractionChanges = (double)i / (double)totalSecondsGuess;
                    var percentChanges = 100 * fractionChanges;
                    progress((int)(Math.Floor(percentChanges)));
                }
                else
               {
                    if (sequentialErrors <= 5)
                    {
                        sequentialErrors++;
                        skipErrorCode = videoFileReader.ReadVideoFrameAndSkipDecode(i + 1);
                        listToReturn.Add(0);
                    }
                    else
                    {
                        break;
                    }
                }


            }
            progress(100);
            return listToReturn;
        }

        private static byte[] GetBigMapArray(Bitmap image, int i)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            var bmpData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            image.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * image.Height;
            byte[] rgbValues = new byte[bytes];
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            return rgbValues;
        }

        private static double GetAverageDifferenceFromPrevious(byte[] withThis, byte[] previous, double threshold)
        {
            double sumSquareDifferences = 0.0;
            var count = 0;
            var countOfConsecutiveBreachers = 1;
            for (int i = 0; i < withThis.Length; i++)
            {
                var differenceSquared = (withThis[i] - previous[i]) * (withThis[i] - previous[i]);
                if (differenceSquared > threshold)
                {
                    if (countOfConsecutiveBreachers > 10)
                    {
                        sumSquareDifferences += countOfConsecutiveBreachers * (withThis[i] - previous[i]) * (withThis[i] - previous[i]);

                    }
                    countOfConsecutiveBreachers++;

                }
                else
                {
                    countOfConsecutiveBreachers = 1;
                }
                count++;
            }
            return Math.Sqrt(sumSquareDifferences / count);
        }



    }
}
