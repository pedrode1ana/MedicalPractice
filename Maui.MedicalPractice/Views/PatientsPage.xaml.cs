using Maui.MedicalPractice.ViewModels;

namespace Maui.MedicalPractice.Views;

public partial class PatientsPage : ContentPage
{
    private readonly PatientsViewModel _vm;
    public PatientsPage()
    {
        InitializeComponent();
        BindingContext = _vm = new PatientsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
        _vm.NewForm();
    }

    private void NewClicked(object sender, EventArgs e) => _vm.NewForm();
    private async void SaveClicked(object sender, EventArgs e) => await _vm.SaveAsync();
    private async void DeleteClicked(object sender, EventArgs e) => await _vm.DeleteAsync();
}
