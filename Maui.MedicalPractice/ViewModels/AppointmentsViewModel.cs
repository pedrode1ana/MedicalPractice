using System.Collections.ObjectModel;
using Library.MedicalPractice.Models;
using Library.MedicalPractice.Services;

namespace Maui.MedicalPractice.ViewModels;

public class AppointmentsViewModel : BaseViewModel
{
    public ObservableCollection<Appointment> Appointments { get; } = new();
    public ObservableCollection<Patient> Patients { get; } = new();
    public ObservableCollection<Physician> Physicians { get; } = new();

    private Appointment? _selectedAppointment;
    public Appointment? SelectedAppointment
    {
        get => _selectedAppointment;
        set => SetProperty(ref _selectedAppointment, value, onChanged: LoadSelected);
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

    private DateTime _appointmentDate = NextWeekday(DateTime.Today.AddDays(1));
    public DateTime AppointmentDate { get => _appointmentDate; set => SetProperty(ref _appointmentDate, value); }

    private TimeSpan _startTime = new(9, 0, 0);
    public TimeSpan StartTime { get => _startTime; set => SetProperty(ref _startTime, value); }

    private TimeSpan _endTime = new(10, 0, 0);
    public TimeSpan EndTime { get => _endTime; set => SetProperty(ref _endTime, value); }

    public async Task RefreshAsync()
    {
        await RefreshRosterAsync();
        Appointments.Clear();
        foreach (var a in AppointmentServiceProxy.Current.Appointments.Where(x => x != null))
            Appointments.Add(a!);

        StatusMessage = $"{Appointments.Count} appointment(s)";
    }

    public async Task NewFormAsync()
    {
        await RefreshRosterAsync();
        SelectedAppointment = null;
        AppointmentDate = NextWeekday(DateTime.Today.AddDays(1));
        StartTime = new TimeSpan(9, 0, 0);
        EndTime = new TimeSpan(10, 0, 0);
        SelectedPatient = Patients.FirstOrDefault();
        SelectedPhysician = Physicians.FirstOrDefault();
        StatusMessage = "Ready to add a new appointment.";
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

            var start = AppointmentDate.Date + StartTime;
            var end = AppointmentDate.Date + EndTime;

            if (SelectedAppointment is null)
            {
                var appt = new Appointment
                {
                    PatientId = SelectedPatient.Id,
                    PhysicianId = SelectedPhysician.Id,
                    StartLocal = start,
                    EndLocal = end
                };

                AppointmentServiceProxy.Current.AddOrUpdate(appt);
                StatusMessage = $"Added appointment #{appt.Id}.";
            }
            else
            {
                SelectedAppointment.PatientId = SelectedPatient.Id;
                SelectedAppointment.PhysicianId = SelectedPhysician.Id;
                SelectedAppointment.StartLocal = start;
                SelectedAppointment.EndLocal = end;

                AppointmentServiceProxy.Current.AddOrUpdate(SelectedAppointment);
                StatusMessage = $"Updated appointment #{SelectedAppointment.Id}.";
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
        if (SelectedAppointment is null)
        {
            StatusMessage = "Select an appointment to delete.";
            return;
        }

        AppointmentServiceProxy.Current.Delete(SelectedAppointment.Id);
        StatusMessage = $"Deleted appointment #{SelectedAppointment.Id}.";
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
        if (SelectedAppointment is null)
            return;

        SelectedPatient = Patients.FirstOrDefault(p => p.Id == SelectedAppointment.PatientId);
        SelectedPhysician = Physicians.FirstOrDefault(d => d.Id == SelectedAppointment.PhysicianId);
        AppointmentDate = SelectedAppointment.StartLocal.Date;
        StartTime = SelectedAppointment.StartLocal.TimeOfDay;
        EndTime = SelectedAppointment.EndLocal.TimeOfDay;
        StatusMessage = $"Editing appointment #{SelectedAppointment.Id}.";
    }

    private static DateTime NextWeekday(DateTime date)
    {
        var candidate = date;
        while (candidate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            candidate = candidate.AddDays(1);
        }
        return candidate;
    }
}
