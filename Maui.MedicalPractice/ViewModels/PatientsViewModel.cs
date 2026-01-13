using System.Collections.ObjectModel;
using Library.MedicalPractice.Models;
using Library.MedicalPractice.Services;

namespace Maui.MedicalPractice.ViewModels;

public class PatientsViewModel : BaseViewModel
{
    public ObservableCollection<Patient> Patients { get; } = new();

    private Patient? _selectedPatient;
    public Patient? SelectedPatient
    {
        get => _selectedPatient;
        set => SetProperty(ref _selectedPatient, value, onChanged: LoadSelected);
    }

    private string _firstName = "";
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = "";
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _address = "";
    public string Address { get => _address; set => SetProperty(ref _address, value); }

    private string _race = "";
    public string Race { get => _race; set => SetProperty(ref _race, value); }

    private string _gender = "";
    public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

    private DateTime _birthdate = DateTime.Today.AddYears(-25);
    public DateTime Birthdate { get => _birthdate; set => SetProperty(ref _birthdate, value); }

    public async Task RefreshAsync()
    {
        await PatientServiceProxy.Current.RefreshFromApiAsync();
        Patients.Clear();
        foreach (var p in PatientServiceProxy.Current.Patients.Where(x => x != null))
        {
            Patients.Add(p!);
        }
        StatusMessage = $"{Patients.Count} patient(s)";
    }

    public void NewForm()
    {
        SelectedPatient = null;
        FirstName = LastName = Address = Race = Gender = "";
        Birthdate = DateTime.Today.AddYears(-25);
        StatusMessage = "Ready to add a new patient.";
    }

    public async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                StatusMessage = "First and last name are required.";
                return;
            }

            if (SelectedPatient is null)
            {
                var p = new Patient
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Address = Address.Trim(),
                    Race = Race.Trim(),
                    Gender = Gender.Trim(),
                    Birthdate = DateOnly.FromDateTime(Birthdate)
                };

                var saved = await PatientServiceProxy.Current.AddOrUpdateAsync(p);
                StatusMessage = $"Added patient #{saved?.Id ?? p.Id}.";
            }
            else
            {
                SelectedPatient.FirstName = FirstName.Trim();
                SelectedPatient.LastName = LastName.Trim();
                SelectedPatient.Address = Address.Trim();
                SelectedPatient.Race = Race.Trim();
                SelectedPatient.Gender = Gender.Trim();
                SelectedPatient.Birthdate = DateOnly.FromDateTime(Birthdate);

                var saved = await PatientServiceProxy.Current.AddOrUpdateAsync(SelectedPatient);
                StatusMessage = $"Updated patient #{saved?.Id ?? SelectedPatient.Id}.";
            }

            await RefreshAsync();
            NewForm();
        }
        catch (Exception ex)
        {
            CaptureError(ex);
        }
    }

    public async Task DeleteAsync()
    {
        if (SelectedPatient is null)
        {
            StatusMessage = "Select a patient to delete.";
            return;
        }

        await PatientServiceProxy.Current.DeleteAsync(SelectedPatient.Id);
        StatusMessage = $"Deleted patient #{SelectedPatient.Id}.";
        await RefreshAsync();
        NewForm();
    }

    private void LoadSelected()
    {
        if (SelectedPatient is null)
            return;

        FirstName = SelectedPatient.FirstName;
        LastName = SelectedPatient.LastName;
        Address = SelectedPatient.Address;
        Race = SelectedPatient.Race ?? "";
        Gender = SelectedPatient.Gender ?? "";
        Birthdate = SelectedPatient.Birthdate.ToDateTime(TimeOnly.MinValue);
        StatusMessage = $"Editing patient #{SelectedPatient.Id}.";
    }
}
