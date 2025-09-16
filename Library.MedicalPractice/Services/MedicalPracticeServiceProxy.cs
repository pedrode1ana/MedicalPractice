using Library.MedicalPractice.Models;


namespace Library.MedicalPractice.Services;


public class PatientServiceProxy
{
    // private backing store
    private readonly List<Patient?> _patients;

    // one instance
    private static PatientServiceProxy? _instance;
    public static PatientServiceProxy Current => _instance ??= new PatientServiceProxy();

    // private ctor
    private PatientServiceProxy()
    {
        _patients = new List<Patient?>();
    }

    // expose list 
    public List<Patient?> Patients => _patients;

    public Patient? AddOrUpdate(Patient? patient)
    {
        if (patient is null) return null;

        // patient model assigns Id if non just add 
        var existing = _patients.FirstOrDefault(p => p?.Id == patient.Id);
        if (existing is null)
        {
            _patients.Add(patient);
        }
        // else: already in list; caller mutates fields directly
        return patient;
    }

    public Patient? Delete(int id)
    {
        var toDelete = _patients.FirstOrDefault(p => p?.Id == id);
        if (toDelete != null)
        {
            _patients.Remove(toDelete);
        }
        return toDelete;
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


