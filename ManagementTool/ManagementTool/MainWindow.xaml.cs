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
using System.Windows;
using System.Collections.Generic;
using Microsoft.Win32;
using BreanosConnectors;
using BreanosConnectors.ActiveMqConnector_FW;
using BreanosConnectors.Kpu.Communication.Common;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;

namespace WpfTutorialSamples.ListBox_control
{
    public partial class ListBoxSelectionSample : Window
    {
        public string ConnectionString { get; private set; } = "activemq:tcp://localhost:61616";
        public string User { get; private set; } = "admin";
        public string Password { get; private set; } = "admin";

        public readonly string AssistentQueueString = "queue://AssistantQueue";

        public readonly string KpuQueueString = "queue://KpuQueue";

        public readonly string LineQueueString = "queue://LineTopic_1";

        public readonly string KPUID = "Hanoi";

        Connector _ManagementActiveMqConnector;       

        private Connector lineConnector;

        private string _amqEndpoint = "activemq:tcp://localhost:61616";
        private string _amqUser = "admin";
        private string _amqPassword = "admin";

        List<TodoItem> items = new List<TodoItem>();
        private void _activeMqConnector_Message(object sender, BreanosConnectors.Interface_FW.OnMessageEventArgs e)
        {
            bool unpackOk = BreanosConnectors.SerializationHelper.TryUnpack(e.Content, out TellKPURequest tell);
            
           // items.Add(new TodoItem() { Title = tell.KPUId, Completion = 0 });

            var AddItem = new Action(() => items.Add(new TodoItem() { Title = tell.KPUId, Completion = 0 }));
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, AddItem);

            var RefreshItem = new Action(() => lbTodoList.Items.Refresh());
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, RefreshItem);
           
        }
        public ListBoxSelectionSample()
        {
            InitializeComponent();

          

            lbTodoList.ItemsSource = items;
           

            //Task.Delay(5000);
        }       

        private void lbTodoList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lbTodoList.SelectedItem != null)
                this.Title = (lbTodoList.SelectedItem as TodoItem).Title;
        }

        private async void SendZipFile(string filePath)
        {
            byte[] array = System.IO.File.ReadAllBytes(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string packedSerialized = SerializationHelper.Pack(array);

            var connector = new Connector();
            bool b = await connector.ConnectAsync(ConnectionString, User, Password);
            connector.SendAsync(packedSerialized, KpuQueueString, BrokerCommands.KPU_DEPLOYMENT, new (string, object)[] { ("KpuId", fileName) }).Wait();
        }

        private void btnDeployment_Click(object sender, RoutedEventArgs e)
        {
            foreach (object o in lbTodoList.SelectedItems)
                MessageBox.Show($"Deploying " + this.Title);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zip Files|*.zip";
            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(openFileDialog.FileName);

                SendZipFile(openFileDialog.FileName);
            }

        }       

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            foreach (object o in lbTodoList.SelectedItems)
                MessageBox.Show($"Starting " + this.Title);

            ExecuteRequest request = new ExecuteRequest()
            {
                Action = "start",
                KpuId = this.Title,
                Parameters = null
            };
            var content = BreanosConnectors.SerializationHelper.Pack(request);
            lineConnector.SendAsync(content, LineQueueString, BrokerCommands.EXECUTE_REQUEST);

        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            foreach (object o in lbTodoList.SelectedItems)
                MessageBox.Show($"Stopping " + this.Title);

            ExecuteRequest request = new ExecuteRequest()
            {
                Action = "stop",
                KpuId = this.Title,
                Parameters = null
            };
            var content = BreanosConnectors.SerializationHelper.Pack(request);
            lineConnector.SendAsync(content, LineQueueString, BrokerCommands.EXECUTE_REQUEST);
        }                    

        void OnClickExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void LoadedM(object sender, RoutedEventArgs e)
        {
            lineConnector = new Connector();
            Task<bool> b = lineConnector.ConnectAsync(ConnectionString, User, Password);//Connect to the LineTopic
            if (b.Result == true)
            {
                _ManagementActiveMqConnector = new Connector();
                var isConnected = _ManagementActiveMqConnector.ConnectAsync(_amqEndpoint, _amqUser, _amqPassword);
                _ManagementActiveMqConnector.Message += _activeMqConnector_Message;
                var isListening = _ManagementActiveMqConnector.ListenAsync("ManagementQueue");

                RequestKPUIdRequest reqKPU = new RequestKPUIdRequest();//client Id is default
                string packedSerialized = SerializationHelper.Pack(reqKPU);
                lineConnector.SendAsync(packedSerialized, LineQueueString, BrokerCommands.REQUESTKPUID);
            }

        }
    }

    public class TodoItem
    {
        public string Title { get; set; }
        public int Completion { get; set; }
    }
}
