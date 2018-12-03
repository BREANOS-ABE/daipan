//-----------------------------------------------------------------------

// <copyright file="MainWindow.xaml.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace HanoiWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public HanoiModel Model { get; set; }
        private bool isFinished;
        public MainWindow()
        {
            StackSize = "3";
            Model = new HanoiModel(0);
            this.DataContext = this;
            this.Loaded += MainWindow_Loaded;
            InitializeComponent();

            Model.Finished += Mod_Finished;
            
        }
        public string StackSize { get; set; }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }



        private void Mod_Finished(object sender, EventArgs e)
        {
            isFinished = true;
            btnRound.Content = "Finished";
            //btnRound.IsEnabled = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Model.RestartCommand(int.Parse(StackSize));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Model"));
        }

    }
}
