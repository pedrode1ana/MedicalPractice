using System.Linq;
using System.Threading.Tasks;
using Maui.MedicalPractice.ViewModels;

namespace Maui.MedicalPractice.Views;

public partial class MedicalNotesPage : ContentPage
{
    private readonly MedicalNotesViewModel _vm;
    public MedicalNotesPage()
    {
        InitializeComponent();
        BindingContext = _vm = new MedicalNotesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshAsync();
        await _vm.NewFormAsync();
    }

    private async void SelectPatientTapped(object sender, TappedEventArgs e) => await ShowPatientPickerAsync();
    private async void SelectPhysicianTapped(object sender, TappedEventArgs e) => await ShowPhysicianPickerAsync();

    private async Task ShowPatientPickerAsync()
    {
        if (_vm.Patients.Count == 0)
        {
            await DisplayAlert("Patients", "Add a patient before writing a note.", "OK");
            return;
        }

        var options = _vm.Patients.Select(p => $"{p.Id}: {p.LastName}, {p.FirstName}").ToList();
        var choice = await DisplayActionSheet("Select patient", "Cancel", null, options.ToArray());
        if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
            return;

        var selectedIndex = options.IndexOf(choice);
        if (selectedIndex >= 0)
            _vm.SelectedPatient = _vm.Patients[selectedIndex];
    }

    private async Task ShowPhysicianPickerAsync()
    {
        if (_vm.Physicians.Count == 0)
        {
            await DisplayAlert("Physicians", "Add a physician before writing a note.", "OK");
            return;
        }

        var options = _vm.Physicians.Select(d => $"{d.Id}: Dr. {d.FirstName} {d.LastName}").ToList();
        var choice = await DisplayActionSheet("Select physician", "Cancel", null, options.ToArray());
        if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
            return;

        var selectedIndex = options.IndexOf(choice);
        if (selectedIndex >= 0)
            _vm.SelectedPhysician = _vm.Physicians[selectedIndex];
    }

    private async void NewClicked(object sender, EventArgs e) => await _vm.NewFormAsync();
    private async void SaveClicked(object sender, EventArgs e) => await _vm.SaveAsync();
    private async void DeleteClicked(object sender, EventArgs e) => await _vm.DeleteAsync();
}
