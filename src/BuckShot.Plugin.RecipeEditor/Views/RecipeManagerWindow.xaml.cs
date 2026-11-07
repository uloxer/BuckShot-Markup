using System.Windows;
using BuckShot.Plugin.RecipeEditor.ViewModels;

namespace BuckShot.Plugin.RecipeEditor.Views
{
    /// <summary>
    /// Interaction logic for RecipeManagerWindow.xaml
    /// </summary>
    public partial class RecipeManagerWindow : Window
    {
        public RecipeManagerViewModel ViewModel => DataContext as RecipeManagerViewModel;

        public RecipeManagerWindow(RecipeManagerViewModel viewModel = null)
        {
            InitializeComponent();
            var vm = viewModel ?? new RecipeManagerViewModel();
            DataContext = vm;
            vm.RequestClose += Close;
        }

        private void OnFilterAllClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "All";
        }

        private void OnFilterActiveClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "Active";
        }

        private void OnFilterDeactivatedClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "Deactivated";
        }

        private void OnFilterBuiltInClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "Built-in";
        }

        private void OnFilterCustomClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "Custom";
        }

        private void OnFilterAiClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) ViewModel.SelectedFilterCategory = "AI";
        }
    }
}
