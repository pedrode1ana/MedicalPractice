namespace Library.MedicalPractice.Models;

public class Patient
{
    // Ids are assigned by the API; default to 0 when creating locally.
    public int Id { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Address { get; set; } = "";
    public DateOnly Birthdate { get; set; }
    public string? Race { get; set; }
    public string? Gender { get; set; }

    public override string ToString() =>
        $"{Id}. {LastName}, {FirstName} | {Birthdate} | {Race}| {Gender} |{Address}";
}

public class Physician
{
    private static int _nextId = 1;
    public int Id { get; private set; } = _nextId++;

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public DateOnly Graduation { get; set; }

    public string? Specialization { get; set; }

    public override string ToString() =>
    $"{Id}. Dr. {FirstName} {LastName} | Lic: {LicenseNumber}| Graduation Date: {Graduation} | Specialization {Specialization} ";
}

public class Appointment
{
    private static int _nextId = 1;
    public int Id { get; private set; } = _nextId++;

    public int PatientId { get; set; }
    public int PhysicianId { get; set; }

    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }

    public override string ToString() =>
    $"{Id}. Patient: {PatientId} with Physician {PhysicianId} | {StartLocal:g}–{EndLocal:t}";
}
public class MedicalNote
{
    private static int _nextId = 1;
    public int Id { get; private set; } = _nextId++;

    public int PatientId { get; set; }
    public int PhysicianId { get; set; }

    public DateTime CreatedLocal { get; set; } = DateTime.Now;
    public string? Diagnoses { get; set; }
    public string? Prescriptions { get; set; }

    public override string ToString() =>
    $"{Id}. Note for Patient: {PatientId} by Physician:{PhysicianId}@{CreatedLocal:g}| Dx {Diagnoses}|Rx: {Prescriptions}";  



}
