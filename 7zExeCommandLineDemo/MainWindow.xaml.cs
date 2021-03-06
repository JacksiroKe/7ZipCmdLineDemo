﻿//using iBot.Activities.PDF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _7zExeCommandLineDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var filelist = SevenZipBot.GetFileList(@"D:\User Directory\Download\dotnet451.zip");
            var filelist = SevenZipBot.Instance.GetFileList(@"D:\User Directory\Download\0723.zip");
            MessageBox.Show($" document list [{string.Join(";", filelist.Data.Select(x => x.InnerFullPath))}]");
            var duplicatefile = filelist.Data.Select(x => System.IO.Path.Combine(@"D:\TEMP\", x.InnerFullPath)).Where(x => File.Exists(x));
            if (duplicatefile.Count() <= 0)
            {
                //SevenZipBot.DeCompress(@"D:\User Directory\Download\0723.zip", @"D:\TEMP");
                MessageBox.Show($" There are duplicate files [{string.Join(";", duplicatefile)}]");
            }
            else
            {
                MessageBox.Show($" There are duplicate files [{string.Join(";", duplicatefile)}]");
                //throw new Exception($" There are duplicate files [{string.Join(";", duplicatefile)}]");
            }

        }
    }
}
