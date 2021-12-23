namespace FeedzCleaner.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using FeedzCleaner.ViewModels;
    using Orchestra;

    public partial class MainWindow
    {
        private bool _isUpdatingCheckboxes;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(EventArgs e)
        {
            base.OnLoaded(e);

            var shellWindow = this;

            shellWindow.WindowState = System.Windows.WindowState.Normal;
            shellWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            shellWindow.Width = 1200;
            shellWindow.Height = 800;

            shellWindow.SetMaximumWidthAndHeight();

            //var windowCommands = new WindowCommands();
            //windowCommands.Items.Add(new WindowCommandsView());
            //shellWindow.WindowCommands = windowCommands;
        }

        private void OnCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingCheckboxes)
            {
                return;
            }

            _isUpdatingCheckboxes = true;

            var initiatingCheckBox = (CheckBox)sender;
            var isChecked = initiatingCheckBox.IsChecked ?? false;

            var selectedCells = dataGrid.SelectedCells;

            foreach (var selectedCell in selectedCells)
            {
                var item = (Package)selectedCell.Item;
                item.ToBeRemoved = isChecked;
            }

            var vm = ViewModel as MainViewModel;
            vm?.UpdateSizeSaved();

            _isUpdatingCheckboxes = false;
        }
    }
}
