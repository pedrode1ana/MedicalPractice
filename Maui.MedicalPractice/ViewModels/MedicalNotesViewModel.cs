using System.Collections.ObjectModel;
using Library.MedicalPractice.Models;
using Library.MedicalPractice.Services;

namespace Maui.MedicalPractice.ViewModels;

public class MedicalNotesViewModel : BaseViewModel
{
    public ObservableCollection<MedicalNote> Notes { get; } = new();
    public ObservableCollection<Patient> Patients { get; } = new();
    public ObservableCollection<Physician> Physicians { get; } = new();

    private MedicalNote? _selectedNote;
    public MedicalNote? SelectedNote
    {
        get => _selectedNote;
        set => SetProperty(ref _selectedNote, value, onChanged: LoadSelected);
    }

    private Patient? _selectedPatient;
    public Patient? SelectedPatient
    {
        get => _selectedPatient;
        set => SetProperty(ref _selectedPatient, value, onChanged: () => OnPropertyChanged(nameof(SelectedPatientDisplay)));
    }
    public string SelectedPatientDisplay => SelectedPatient?.ToString() ?? "Tap to pick a patient";

    private Physician? _selectedPhysician;
    public Physician? SelectedPhysician
    {
        get => _selectedPhysician;
        set => SetProperty(ref _selectedPhysician, value, onChanged: () => OnPropertyChanged(nameof(SelectedPhysicianDisplay)));
    }
    public string SelectedPhysicianDisplay => SelectedPhysician?.ToString() ?? "Tap to pick a physician";

    private string _diagnoses = "";
    public string Diagnoses { get => _diagnoses; set => SetProperty(ref _diagnoses, value); }

    private string _prescriptions = "";
    public string Prescriptions { get => _prescriptions; set => SetProperty(ref _prescriptions, value); }

    public async Task RefreshAsync()
    {
        await RefreshRosterAsync();
        Notes.Clear();
        foreach (var n in MedicalNoteServiceProxy.Current.Notes.Where(x => x != null))
            Notes.Add(n!);

        StatusMessage = $"{Notes.Count} note(s)";
    }

    public async Task NewFormAsync()
    {
        await RefreshRosterAsync();
        SelectedNote = null;
        SelectedPatient = Patients.FirstOrDefault();
        SelectedPhysician = Physicians.FirstOrDefault();
        Diagnoses = "";
        Prescriptions = "";
        StatusMessage = "Ready to add a new note.";
    }

    public async Task SaveAsync()
    {
        try
        {
            if (SelectedPatient is null || SelectedPhysician is null)
            {
                StatusMessage = "Select a patient and physician.";
                return;
            }

            if (SelectedNote is null)
            {
                var note = new MedicalNote
                {
                    PatientId = SelectedPatient.Id,
                    PhysicianId = SelectedPhysician.Id,
                    Diagnoses = Diagnoses.Trim(),
                    Prescriptions = Prescriptions.Trim()
                };

                MedicalNoteServiceProxy.Current.AddOrUpdate(note);
                StatusMessage = $"Added note #{note.Id}.";
            }
            else
            {
                SelectedNote.PatientId = SelectedPatient.Id;
                SelectedNote.PhysicianId = SelectedPhysician.Id;
                SelectedNote.Diagnoses = Diagnoses.Trim();
                SelectedNote.Prescriptions = Prescriptions.Trim();

                MedicalNoteServiceProxy.Current.AddOrUpdate(SelectedNote);
                StatusMessage = $"Updated note #{SelectedNote.Id}.";
            }

            await RefreshAsync();
            await NewFormAsync();
        }
        catch (Exception ex)
        {
            CaptureError(ex);
        }
    }

    public async Task DeleteAsync()
    {
        if (SelectedNote is null)
        {
            StatusMessage = "Select a note to delete.";
            return;
        }

        MedicalNoteServiceProxy.Current.Delete(SelectedNote.Id);
        StatusMessage = $"Deleted note #{SelectedNote.Id}.";
        await RefreshAsync();
        await NewFormAsync();
    }

    private async Task RefreshRosterAsync()
    {
        await PatientServiceProxy.Current.RefreshFromApiAsync();
        Patients.Clear();
        foreach (var p in PatientServiceProxy.Current.Patients.Where(x => x != null))
            Patients.Add(p!);

        Physicians.Clear();
        foreach (var d in PhysicianServiceProxy.Current.Physicians.Where(x => x != null))
            Physicians.Add(d!);
    }

    private void LoadSelected()
    {
        if (SelectedNote is null)
            return;

        SelectedPatient = Patients.FirstOrDefault(p => p.Id == SelectedNote.PatientId);
        SelectedPhysician = Physicians.FirstOrDefault(d => d.Id == SelectedNote.PhysicianId);
        Diagnoses = SelectedNote.Diagnoses ?? "";
        Prescriptions = SelectedNote.Prescriptions ?? "";
        StatusMessage = $"Editing note #{SelectedNote.Id}.";
    }
}
