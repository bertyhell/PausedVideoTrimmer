using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Drawing;
using System.Linq;
using System.Windows.Documents;
using Color = System.Drawing.Color;

namespace PausedVideoTrimmer
{
    public partial class MainWindow
    {
        private readonly MainController _controller;
        private string _sourceImagesPath = @"C:\Users\verhe\Downloads\dark souls speedup\img";
        private string _outputVideoPath;
        string[] _files;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _controller = new MainController(this);
        }

        private void SelectSourceImagesPath(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select source folder",
                UseDescriptionForTitle = true
            };
            bool? showDialog = dialog.ShowDialog(this);
            if (showDialog != null && (bool)showDialog)
            {
                _sourceImagesPath = dialog.SelectedPath;
            }
        }

        private void SelectOutputVideoFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? userClickedOk = openFileDialog.ShowDialog();
            if (userClickedOk == true)
            {
                _outputVideoPath = openFileDialog.FileName;
            }
        }

        private void BtnAnalyse(object sender, RoutedEventArgs e)
        {
            _files = Directory.GetFiles(_sourceImagesPath);
            BackgroundImageConverter converter = new BackgroundImageConverter(_files);
            converter.RunWorkerCompleted += Converter_RunWorkerCompleted;
            converter.RunWorkerAsync();
        }

        private void Converter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Delete rejects");

            int motionMargin = 3;
            int[] diffs = (int[]) e.Result;
            bool[] keepImages = new bool[_files.Length];

            string diffString = "";
            for (int i = 0; i < diffs.Length; i++)
            {
                diffString += ("" + diffs[i]).Replace(",", ".") + "\n";
                if (diffs[i] > 22000)
                {
                    for (int j = i - motionMargin; j <= i+motionMargin; j++)
                    {
                        if (j >= 0 && j < diffs.Length)
                        {
                            keepImages[j] = true;
                        }
                    }

                }
            }

            // Rename to 00000001, 00000002 ...
            int imgNumber = 0;
            for (int i = 0; i < diffs.Length; i++)
            {
                if (!keepImages[i])
                {
                    File.Delete(_files[i]);
                }
                else
                {
                    File.Move(_files[i], Path.GetDirectoryName(_files[i]) + "\\" + imgNumber.ToString().PadLeft(8, '0') + ".jpg");
                    imgNumber++;
                }
            }
            
            // Output diffs to textfile
            File.WriteAllText(_sourceImagesPath + "/diffs.txt", diffString);


            Console.WriteLine("Generate Video file");
            // ffmpeg -framerate 30 -i %08d.jpg video.webm 

            Console.WriteLine("done");
        }
    }
}
