using Library.MedicalPractice.Models;


namespace Library.MedicalPractice.Services;


public class PatientServiceProxy
{
    // private backing store
    private readonly List<Patient?> _patients;
    private bool _isLoaded;
    private Task? _refreshTask;
    private readonly WebRequestHandler _http = new();

    // one instance
    private static PatientServiceProxy? _instance;
    public static PatientServiceProxy Current => _instance ??= new PatientServiceProxy();

    // private ctor
    private PatientServiceProxy()
    {
        _patients = new List<Patient?>();
    }

    // expose list 
    public List<Patient?> Patients
    {
        get
        {
            if (!_isLoaded) RefreshFromApi();
            return _patients;
        }
    }

    public async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        await RefreshFromApiAsync().ConfigureAwait(false);
    }

    public async Task RefreshFromApiAsync()
    {
        if (_refreshTask is { IsCompleted: false })
        {
            await _refreshTask.ConfigureAwait(false);
            return;
        }

        _refreshTask = RefreshInternalAsync();
        await _refreshTask.ConfigureAwait(false);
    }

    private async Task RefreshInternalAsync()
    {
        try
        {
            var patients = await _http.GetAsync<List<Patient>>("patients").ConfigureAwait(false);
            if (patients is not null)
            {
                _patients.Clear();
                _patients.AddRange(patients);
            }
        }
        catch
        {
            // if the API is unavailable, keep whatever is in memory
        }
        finally
        {
            _isLoaded = true;
            _refreshTask = null;
        }
    }

    public void RefreshFromApi() => RefreshFromApiAsync().GetAwaiter().GetResult();

    public async Task<Patient?> AddOrUpdateAsync(Patient? patient)
    {
        if (patient is null) return null;

        if (!_isLoaded) await RefreshFromApiAsync().ConfigureAwait(false);

        try
        {
            Patient? result;
            var exists = _patients.Any(p => p?.Id == patient.Id);

            if (!exists)
            {
                result = await _http.PostAsync<Patient>("patients", patient).ConfigureAwait(false);
            }
            else
            {
                result = await _http.PutAsync<Patient>($"patients/{patient.Id}", patient).ConfigureAwait(false);
            }

            if (result != null) UpsertLocal(result);
        }
        catch
        {
            // fall back handled below
        }

        // fall back to in-memory behavior when API is unavailable or returned null
        var existing = _patients.FirstOrDefault(p => p?.Id == patient.Id);
        if (existing is null)
        {
            _patients.Add(patient);
        }
        return patient;
    }

    public Patient? AddOrUpdate(Patient? patient) => AddOrUpdateAsync(patient).GetAwaiter().GetResult();

    public async Task<Patient?> DeleteAsync(int id)
    {
        if (!_isLoaded) await RefreshFromApiAsync().ConfigureAwait(false);

        var toDelete = _patients.FirstOrDefault(p => p?.Id == id);
        if (toDelete != null) _patients.Remove(toDelete);

        try
        {
            await _http.DeleteAsync($"patients/{id}").ConfigureAwait(false);
        }
        catch
        {
            // ignore remote failures; local list is already updated
        }

        return toDelete;
    }

    public Patient? Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();

    public async Task<IEnumerable<Patient?>> SearchAsync(string query)
    {
        if (!_isLoaded) await RefreshFromApiAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(query))
            return Patients;

        try
        {
            var results = await _http.GetAsync<List<Patient>>($"patients/search?q={Uri.EscapeDataString(query)}").ConfigureAwait(false);
            return results ?? Enumerable.Empty<Patient>();
        }
        catch
        {
            return _patients.Where(p =>
                p != null &&
                ((p.FirstName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (p.LastName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (p.Address?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)));
        }
    }

    public IEnumerable<Patient?> Search(string query) => SearchAsync(query).GetAwaiter().GetResult();

    private void UpsertLocal(Patient patient)
    {
        var existing = _patients.FirstOrDefault(p => p?.Id == patient.Id);
        if (existing is not null) _patients.Remove(existing);
        _patients.Add(patient);
    }
}

public class PhysicianServiceProxy
{
    private readonly List<Physician?> _physicians;

    private static PhysicianServiceProxy? _instance;
    public static PhysicianServiceProxy Current => _instance ??= new PhysicianServiceProxy();

    private PhysicianServiceProxy()
    {
        _physicians = new List<Physician?>();
    }

    public List<Physician?> Physicians => _physicians;

    public Physician? AddOrUpdate(Physician? physician)
    {
        if (physician is null) return null;

        // License number must be unique 
        var duplicate = _physicians.Any(ph =>
            ph != null &&
            ph!.Id != physician.Id &&
            string.Equals(ph.LicenseNumber, physician.LicenseNumber, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            throw new InvalidOperationException("License number must be unique.");

        var existing = _physicians.FirstOrDefault(ph => ph?.Id == physician.Id);
        if (existing is null)
        {
            _physicians.Add(physician);
        }

        return physician;
    }

    public Physician? Delete(int id)
    {
        var toDelete = _physicians.FirstOrDefault(ph => ph?.Id == id);
        if (toDelete != null) _physicians.Remove(toDelete);
        return toDelete;
    }
}


public class MedicalNoteServiceProxy
{
    private readonly List<MedicalNote?> _notes;

    private static MedicalNoteServiceProxy? _instance;
    public static MedicalNoteServiceProxy Current => _instance ??= new MedicalNoteServiceProxy();

    private MedicalNoteServiceProxy()
    {
        _notes = new List<MedicalNote?>();
    }

    public List<MedicalNote?> Notes => _notes;

    public MedicalNote? AddOrUpdate(MedicalNote? note)
    {
        if (note is null) return null;

        EnsurePatientPhysicianExist(note.PatientId, note.PhysicianId);

        var existing = _notes.FirstOrDefault(n => n?.Id == note.Id);
        if (existing is null)
        {
            _notes.Add(note);
        }

        return note;
    }

    public MedicalNote? Delete(int id)
    {
        var toDelete = _notes.FirstOrDefault(n => n?.Id == id);
        if (toDelete != null) _notes.Remove(toDelete);
        return toDelete;
    }

    private static void EnsurePatientPhysicianExist(int patientId, int physicianId)
    {
        var pExists = PatientServiceProxy.Current.Patients.Any(p => p?.Id == patientId);
        var dExists = PhysicianServiceProxy.Current.Physicians.Any(ph => ph?.Id == physicianId);

        if (!pExists) throw new InvalidOperationException("Patient not found.");
        if (!dExists) throw new InvalidOperationException("Physician not found.");
    }
}


// Appointments M–F 8–5, no double-booking 

public class AppointmentServiceProxy
{
    private readonly List<Appointment?> _appointments;

    private static AppointmentServiceProxy? _instance;
    public static AppointmentServiceProxy Current => _instance ??= new AppointmentServiceProxy();

    private AppointmentServiceProxy()
    {
        _appointments = new List<Appointment?>();
    }

    public List<Appointment?> Appointments => _appointments;

    public Appointment? AddOrUpdate(Appointment? appt)
    {
        if (appt is null) return null;

        // make sure tgeres both
        EnsurePatientPhysicianExist(appt.PatientId, appt.PhysicianId);

        // checks for times
        if (!IsWithinBusinessHours(appt.StartLocal, appt.EndLocal))
            throw new InvalidOperationException("Appointments must be Monday–Friday between 8:00 and 17:00 (same day).");

        if (IsDoubleBooked(appt))
            throw new InvalidOperationException("Physician is already booked for that time.");

        // Add if not present 
        var existing = _appointments.FirstOrDefault(a => a?.Id == appt.Id);
        if (existing is null)
        {
            _appointments.Add(appt);
        }

        return appt;
    }

    public Appointment? Delete(int id)
    {
        var toDelete = _appointments.FirstOrDefault(a => a?.Id == id);
        if (toDelete != null) _appointments.Remove(toDelete);
        return toDelete;
    }

    //helpers
    private static void EnsurePatientPhysicianExist(int patientId, int physicianId)
    {
        var pExists = PatientServiceProxy.Current.Patients.Any(p => p?.Id == patientId);
        var dExists = PhysicianServiceProxy.Current.Physicians.Any(ph => ph?.Id == physicianId);

        if (!pExists) throw new InvalidOperationException("Patient not found.");
        if (!dExists) throw new InvalidOperationException("Physician not found.");
    }

    private static bool IsWithinBusinessHours(DateTime start, DateTime end)
    {
        if (start >= end) return false;
        if (start.Date != end.Date) return false;
        if (start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return false;
        if (end.DayOfWeek   is DayOfWeek.Saturday or DayOfWeek.Sunday) return false;

        var open  = start.Date.AddHours(8);  // 08:00
        var close = start.Date.AddHours(17); // 17:00
        return start >= open && end <= close;
    }

    private bool IsDoubleBooked(Appointment appt)
    {
        return _appointments
            .Where(a => a != null && a!.PhysicianId == appt.PhysicianId && a.Id != appt.Id)
            .Any(a => Overlaps(a!.StartLocal, a.EndLocal, appt.StartLocal, appt.EndLocal));
    }

    private static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        => aStart < bEnd && bStart < aEnd;
}
