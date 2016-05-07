using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PausedVideoTrimmer
{
    class BackgroundImageConverter : BackgroundWorker
    {
        private readonly string[] _files;

        public BackgroundImageConverter(string[] files)
        {
            _files = files;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            int counter = 0;
            int lastCounter = 0;
            int lastPercentage = 0;

            int [] diffs = new int[_files.Length];
            
            Parallel.For(0, _files.Length - 1, i => {
                                                       diffs[i] = Compare(_files[i], _files[i + 1]);
                                                       counter++;
            if (counter > lastCounter + 100)
            {
                    lastCounter = counter;
                    int percentage = (int)Math.Round((double)((double)counter / (double)_files.Length) * 100);
                
                if (percentage > lastPercentage)
                                                           {
                                                               lastPercentage = percentage;
                                                               Console.WriteLine(percentage);
                                                           }
                }
            });
            e.Result = diffs;
        }

        private int Compare(string image1, string image2)
        {
            int borderTop = 56;
            int borderRight = 250;
            int differentPixels = 0;
            using (Bitmap first = new Bitmap(image1))
            {
                using (Bitmap second = new Bitmap(image2))
                {
                    for (int i = 0; i < first.Width - borderRight; i += 20)
                    {
                        for (int j = borderTop; j < first.Height; j += 20)
                        {
                            Color secondPixel = second.GetPixel(i, j);
                            Color firstPixel = first.GetPixel(i, j);


                            int r1 = secondPixel.R;
                            int g1 = secondPixel.G;
                            int b1 = secondPixel.B;

                            int r2 = firstPixel.R;
                            int g2 = firstPixel.G;
                            int b2 = firstPixel.B;

                            differentPixels += Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
                        }
                    }
                }
            }
            return differentPixels;
        }
    }
}
