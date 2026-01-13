using System.Collections.ObjectModel;
using Library.MedicalPractice.Models;
using Library.MedicalPractice.Services;

namespace Maui.MedicalPractice.ViewModels;

public class PhysiciansViewModel : BaseViewModel
{
    public ObservableCollection<Physician> Physicians { get; } = new();

    private Physician? _selectedPhysician;
    public Physician? SelectedPhysician
    {
        get => _selectedPhysician;
        set => SetProperty(ref _selectedPhysician, value, onChanged: LoadSelected);
    }

    private string _firstName = "";
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = "";
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _licenseNumber = "";
    public string LicenseNumber { get => _licenseNumber; set => SetProperty(ref _licenseNumber, value); }

    private string _specialization = "";
    public string Specialization { get => _specialization; set => SetProperty(ref _specialization, value); }

    private DateTime _graduation = DateTime.Today.AddYears(-5);
    public DateTime Graduation { get => _graduation; set => SetProperty(ref _graduation, value); }

    public void Refresh()
    {
        Physicians.Clear();
        foreach (var d in PhysicianServiceProxy.Current.Physicians.Where(x => x != null))
            Physicians.Add(d!);

        StatusMessage = $"{Physicians.Count} physician(s)";
    }

    public void NewForm()
    {
        SelectedPhysician = null;
        FirstName = LastName = LicenseNumber = Specialization = "";
        Graduation = DateTime.Today.AddYears(-5);
        StatusMessage = "Ready to add a new physician.";
    }

    public void Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(LicenseNumber))
            {
                StatusMessage = "First name, last name, and license number are required.";
                return;
            }

            if (SelectedPhysician is null)
            {
                var d = new Physician
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    LicenseNumber = LicenseNumber.Trim(),
                    Graduation = DateOnly.FromDateTime(Graduation),
                    Specialization = Specialization.Trim()
                };

                PhysicianServiceProxy.Current.AddOrUpdate(d);
                StatusMessage = $"Added physician #{d.Id}.";
            }
            else
            {
                SelectedPhysician.FirstName = FirstName.Trim();
                SelectedPhysician.LastName = LastName.Trim();
                SelectedPhysician.LicenseNumber = LicenseNumber.Trim();
                SelectedPhysician.Specialization = Specialization.Trim();
                SelectedPhysician.Graduation = DateOnly.FromDateTime(Graduation);

                PhysicianServiceProxy.Current.AddOrUpdate(SelectedPhysician);
                StatusMessage = $"Updated physician #{SelectedPhysician.Id}.";
            }

            Refresh();
            NewForm();
        }
        catch (Exception ex)
        {
            CaptureError(ex);
        }
    }

    public void Delete()
    {
        if (SelectedPhysician is null)
        {
            StatusMessage = "Select a physician to delete.";
            return;
        }

        PhysicianServiceProxy.Current.Delete(SelectedPhysician.Id);
        StatusMessage = $"Deleted physician #{SelectedPhysician.Id}.";
        Refresh();
        NewForm();
    }

    private void LoadSelected()
    {
        if (SelectedPhysician is null)
            return;

        FirstName = SelectedPhysician.FirstName;
        LastName = SelectedPhysician.LastName;
        LicenseNumber = SelectedPhysician.LicenseNumber;
        Specialization = SelectedPhysician.Specialization ?? "";
        Graduation = SelectedPhysician.Graduation.ToDateTime(TimeOnly.MinValue);
        StatusMessage = $"Editing physician #{SelectedPhysician.Id}.";
    }
}
