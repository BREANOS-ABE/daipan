//-----------------------------------------------------------------------

// <copyright file="ApplicationServicesProvider.cs" company="Breanos GmbH">
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ActivityViewModelInterfaces
{

    /// <summary>
    /// This interface defines a UI controller which can be used to display dialogs
    /// in either modal form from a ViewModel.
    /// </summary>
    public interface IUIVisualizerService
    {
        /// <summary>
        /// This method displays a modal dialog associated with the given key.
        /// </summary>
        /// <param name="dataContextForPopup">Object state to associate with the dialog</param>
        /// <returns>True/False if UI is displayed.</returns>
        bool? ShowDialog(object dataContextForPopup);
    }
    /// <summary>
    /// Available Button options. 
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// Available Icon options.
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogIcons
    {
        None,
        Information,
        Question,
        Exclamation,
        Stop,
        Warning
    }

    /// <summary>
    /// Available DialogResults options.
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogResults
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }

    /// <summary>
    /// This interface defines a interface that will allow 
    /// a ViewModel to show a messagebox
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="message">The error message</param>
        void ShowError(string message);

        /// <summary>
        /// Shows an error message with a custom caption
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="caption">The caption</param>
        void ShowError(string message, string caption);

        /// <summary>
        /// Shows an information message
        /// </summary>
        /// <param name="message">The information message</param>
        void ShowInformation(string message);

        /// <summary>
        /// Shows an information message with a custom caption
        /// </summary>
        /// <param name="message">The information message</param>
        /// <param name="caption">The caption</param>
        void ShowInformation(string message, string caption);

        /// <summary>
        /// Shows an warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        void ShowWarning(string message);

        /// <summary>
        /// Shows an warning message with a custom caption
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="caption">The caption</param>
        void ShowWarning(string message, string caption);

        /// <summary>
        /// Displays a Yes/No dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNo(string message, CustomDialogIcons icon);

        /// <summary>
        /// Displays a Yes/No dialog with a custom caption, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNo(string message, string caption, CustomDialogIcons icon);

        /// <summary>
        /// Displays a Yes/No/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNoCancel(string message, CustomDialogIcons icon);

        /// <summary>
        /// Displays a Yes/No dialog with a default button selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNo(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult);

        /// <summary>
        /// Displays a Yes/No/Cancel dialog with a custom caption and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNoCancel(string message, string caption, CustomDialogIcons icon);

        /// <summary>
        /// Displays a Yes/No/Cancel dialog with a default button selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowYesNoCancel(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult);

        /// <summary>
        /// Displays a OK/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowOkCancel(string message, CustomDialogIcons icon);

        /// <summary>
        /// Displays a OK/Cancel dialog with a custom caption and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowOkCancel(string message, string caption, CustomDialogIcons icon);

        /// <summary>
        /// Displays a OK/Cancel dialog with a default button selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        CustomDialogResults ShowOkCancel(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult);

    }
    /// <summary>
    /// Simple service interface
    /// </summary>
    public interface IServiceProvider
    {
        IUIVisualizerService VisualizerService { get; }
        IMessageBoxService MessageBoxService { get; }
    }

    /// <summary>
    /// This class implements the IMessageBoxService for WPF purposes.
    /// </summary>
    public class WPFMessageBoxService : IMessageBoxService
    {
        #region IMessageBoxService Members

        /// <summary>
        /// Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public void ShowError(string message)
        {
            ShowMessage(message, "Error", CustomDialogIcons.Stop);
        }

        /// <summary>
        /// Displays an error dialog with a given message and caption.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        public void ShowError(string message, string caption)
        {
            ShowMessage(message, caption, CustomDialogIcons.Stop);
        }

        /// <summary>
        /// Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public void ShowInformation(string message)
        {
            ShowMessage(message, "Information", CustomDialogIcons.Information);
        }

        /// <summary>
        /// Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        public void ShowInformation(string message, string caption)
        {
            ShowMessage(message, caption, CustomDialogIcons.Information);
        }

        /// <summary>
        /// Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        public void ShowWarning(string message)
        {
            ShowMessage(message, "Warning", CustomDialogIcons.Warning);
        }

        /// <summary>
        /// Displays an error dialog with a given message.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        public void ShowWarning(string message, string caption)
        {
            ShowMessage(message, caption, CustomDialogIcons.Warning);
        }

        /// <summary>
        /// Displays a Yes/No dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNo(string message, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, icon, CustomDialogButtons.YesNo);
        }

        /// <summary>
        /// Displays a Yes/No dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNo(string message, string caption, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.YesNo);
        }

        /// <summary>
        /// Displays a Yes/No dialog with a default button selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNo(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.YesNo, defaultResult);
        }

        /// <summary>
        /// Displays a Yes/No/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNoCancel(string message, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, icon, CustomDialogButtons.YesNoCancel);
        }


        /// <summary>
        /// Displays a Yes/No/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNoCancel(string message, string caption, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.YesNoCancel);
        }

        /// <summary>
        /// Displays a Yes/No/Cancel dialog with a default button selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowYesNoCancel(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.YesNoCancel, defaultResult);
        }

        /// <summary>
        /// Displays a OK/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowOkCancel(string message, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, icon, CustomDialogButtons.OKCancel);
        }

        /// <summary>
        /// Displays a OK/Cancel dialog and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowOkCancel(string message, string caption, CustomDialogIcons icon)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.OKCancel);
        }

        /// <summary>
        /// Displays a OK/Cancel dialog with a default result selected, and returns the user input.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>User selection.</returns>
        public CustomDialogResults ShowOkCancel(string message, string caption, CustomDialogIcons icon, CustomDialogResults defaultResult)
        {
            return ShowQuestionWithButton(message, caption, icon, CustomDialogButtons.OKCancel, defaultResult);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Shows a standard System.Windows.MessageBox using the parameters requested
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The heading to be displayed</param>
        /// <param name="icon">The icon to be displayed.</param>
        private void ShowMessage(string message, string caption, CustomDialogIcons icon)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, GetImage(icon));
        }


        /// <summary>
        /// Shows a standard System.Windows.MessageBox using the parameters requested
        /// but will return a translated result to enable adhere to the IMessageBoxService
        /// implementation required. 
        /// 
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="button"></param>
        /// <returns>CustomDialogResults results to use</returns>
        private CustomDialogResults ShowQuestionWithButton(string message,
            CustomDialogIcons icon, CustomDialogButtons button)
        {
            MessageBoxResult result = MessageBox.Show(message, "Please confirm...",
                GetButton(button), GetImage(icon));
            return GetResult(result);
        }


        /// <summary>
        /// Shows a standard System.Windows.MessageBox using the parameters requested
        /// but will return a translated result to enable adhere to the IMessageBoxService
        /// implementation required. 
        /// 
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="button"></param>
        /// <returns>CustomDialogResults results to use</returns>
        private CustomDialogResults ShowQuestionWithButton(string message, string caption,
            CustomDialogIcons icon, CustomDialogButtons button)
        {
            MessageBoxResult result = MessageBox.Show(message, caption,
                GetButton(button), GetImage(icon));
            return GetResult(result);
        }

        /// <summary>
        /// Shows a standard System.Windows.MessageBox using the parameters requested
        /// but will return a translated result to enable adhere to the IMessageBoxService
        /// implementation required. 
        /// 
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="caption">The caption of the message box window</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="button"></param>
        /// <param name="defaultResult">Default result for the message box</param>
        /// <returns>CustomDialogResults results to use</returns>
        private CustomDialogResults ShowQuestionWithButton(string message, string caption,
            CustomDialogIcons icon, CustomDialogButtons button, CustomDialogResults defaultResult)
        {
            MessageBoxResult result = MessageBox.Show(message, caption,
                GetButton(button), GetImage(icon), GetResult(defaultResult));
            return GetResult(result);
        }


        /// <summary>
        /// Translates a CustomDialogIcons into a standard WPF System.Windows.MessageBox MessageBoxImage.
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="icon">The icon to be displayed.</param>
        /// <returns>A standard WPF System.Windows.MessageBox MessageBoxImage</returns>
        private MessageBoxImage GetImage(CustomDialogIcons icon)
        {
            MessageBoxImage image = MessageBoxImage.None;

            switch (icon)
            {
                case CustomDialogIcons.Information:
                    image = MessageBoxImage.Information;
                    break;
                case CustomDialogIcons.Question:
                    image = MessageBoxImage.Question;
                    break;
                case CustomDialogIcons.Exclamation:
                    image = MessageBoxImage.Exclamation;
                    break;
                case CustomDialogIcons.Stop:
                    image = MessageBoxImage.Stop;
                    break;
                case CustomDialogIcons.Warning:
                    image = MessageBoxImage.Warning;
                    break;
            }
            return image;
        }


        /// <summary>
        /// Translates a CustomDialogButtons into a standard WPF System.Windows.MessageBox MessageBoxButton.
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="btn">The button type to be displayed.</param>
        /// <returns>A standard WPF System.Windows.MessageBox MessageBoxButton</returns>
        private MessageBoxButton GetButton(CustomDialogButtons btn)
        {
            MessageBoxButton button = MessageBoxButton.OK;

            switch (btn)
            {
                case CustomDialogButtons.OK:
                    button = MessageBoxButton.OK;
                    break;
                case CustomDialogButtons.OKCancel:
                    button = MessageBoxButton.OKCancel;
                    break;
                case CustomDialogButtons.YesNo:
                    button = MessageBoxButton.YesNo;
                    break;
                case CustomDialogButtons.YesNoCancel:
                    button = MessageBoxButton.YesNoCancel;
                    break;
            }
            return button;
        }


        /// <summary>
        /// Translates a standard WPF System.Windows.MessageBox MessageBoxResult into a
        /// CustomDialogIcons.
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="result">The standard WPF System.Windows.MessageBox MessageBoxResult</param>
        /// <returns>CustomDialogResults results to use</returns>
        private CustomDialogResults GetResult(MessageBoxResult result)
        {
            CustomDialogResults customDialogResults = CustomDialogResults.None;

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    customDialogResults = CustomDialogResults.Cancel;
                    break;
                case MessageBoxResult.No:
                    customDialogResults = CustomDialogResults.No;
                    break;
                case MessageBoxResult.None:
                    customDialogResults = CustomDialogResults.None;
                    break;
                case MessageBoxResult.OK:
                    customDialogResults = CustomDialogResults.OK;
                    break;
                case MessageBoxResult.Yes:
                    customDialogResults = CustomDialogResults.Yes;
                    break;
            }
            return customDialogResults;
        }

        /// <summary>
        /// Translates a CustomDialogResults into a standard WPF System.Windows.MessageBox MessageBoxResult
        /// This abstraction allows for different frameworks to use the same ViewModels but supply
        /// alternative implementations of core service interfaces
        /// </summary>
        /// <param name="result">The CustomDialogResults</param>
        /// <returns>The standard WPF System.Windows.MessageBox MessageBoxResult results to use</returns>
        private MessageBoxResult GetResult(CustomDialogResults result)
        {
            MessageBoxResult customDialogResults = MessageBoxResult.None;

            switch (result)
            {
                case CustomDialogResults.Cancel:
                    customDialogResults = MessageBoxResult.Cancel;
                    break;
                case CustomDialogResults.No:
                    customDialogResults = MessageBoxResult.No;
                    break;
                case CustomDialogResults.None:
                    customDialogResults = MessageBoxResult.None;
                    break;
                case CustomDialogResults.OK:
                    customDialogResults = MessageBoxResult.OK;
                    break;
                case CustomDialogResults.Yes:
                    customDialogResults = MessageBoxResult.Yes;
                    break;
            }
            return customDialogResults;
        }
        #endregion

    }
    public class WPFUIVisualizerService : IUIVisualizerService
    {

        #region Public Methods
        /// <summary>
        /// This method displays a modal dialog associated with the given key.
        /// </summary>
        /// <param name="dataContextForPopup">Object state to associate with the dialog</param>
        /// <returns>True/False if UI is displayed.</returns>
        public bool? ShowDialog(object dataContextForPopup)
        {
            Window win = new PopupWindow();
            win.DataContext = dataContextForPopup;
            win.Owner = Application.Current.MainWindow;
            if (win != null)
                return win.ShowDialog();

            return false;
        }
        #endregion


    }
    /// <summary>
    /// Simple service locator
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private IUIVisualizerService visualizerService = new WPFUIVisualizerService();
        private IMessageBoxService messageBoxService = new WPFMessageBoxService();

        public IUIVisualizerService VisualizerService
        {
            get { return visualizerService; }
        }

        public IMessageBoxService MessageBoxService
        {
            get { return messageBoxService; }
        }
    }



    /// <summary>
    /// Simple service locator helper
    /// </summary>
    public class ApplicationServicesProvider
    {
        private static Lazy<ApplicationServicesProvider> instance = new Lazy<ApplicationServicesProvider>(() => new ApplicationServicesProvider());
        private IServiceProvider serviceProvider = new ServiceProvider();

        private ApplicationServicesProvider()
        {

        }

        static ApplicationServicesProvider()
        {

        }

        public void SetNewServiceProvider(IServiceProvider provider)
        {
            serviceProvider = provider;
        }

        public IServiceProvider Provider
        {
            get { return serviceProvider; }
        }

        public static ApplicationServicesProvider Instance
        {
            get { return instance.Value; }
        }
    }
}
