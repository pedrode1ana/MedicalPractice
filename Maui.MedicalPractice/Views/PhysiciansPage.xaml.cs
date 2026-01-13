using Maui.MedicalPractice.ViewModels;

namespace Maui.MedicalPractice.Views;

public partial class PhysiciansPage : ContentPage
{
    private readonly PhysiciansViewModel _vm;
    public PhysiciansPage()
    {
        InitializeComponent();
        BindingContext = _vm = new PhysiciansViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
        _vm.NewForm();
    }

    private void NewClicked(object sender, EventArgs e) => _vm.NewForm();
    private void SaveClicked(object sender, EventArgs e) => _vm.Save();
    private void DeleteClicked(object sender, EventArgs e) => _vm.Delete();
}
