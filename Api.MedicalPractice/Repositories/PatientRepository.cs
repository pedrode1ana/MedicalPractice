using System.Reflection;
using Library.MedicalPractice.Models;
using Library.MedicalPractice.Serialization;
using Newtonsoft.Json;

namespace Api.MedicalPractice.Repositories;

public class PatientRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly List<Patient> _patients = new();
    private int _nextId = 1;

    public PatientRepository(IHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "patients.json");
        Load();
    }

    public IEnumerable<Patient> GetAll() => _patients.OrderBy(p => p.Id).ToList();

    public Patient? GetById(int id) => _patients.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Patient> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAll();

        query = query.ToLowerInvariant();
        return _patients.Where(p =>
            p.FirstName.ToLowerInvariant().Contains(query) ||
            p.LastName.ToLowerInvariant().Contains(query) ||
            (p.Address ?? string.Empty).ToLowerInvariant().Contains(query));
    }

    public Patient Create(Patient patient) => AddOrUpdateInternal(null, patient)!;

    public Patient? Update(int id, Patient patient) => AddOrUpdateInternal(id, patient);

    public Patient? Delete(int id)
    {
        _mutex.Wait();
        try
        {
            var existing = _patients.FirstOrDefault(p => p.Id == id);
            if (existing is null) return null;

            _patients.Remove(existing);
            Persist();
            return existing;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private Patient? AddOrUpdateInternal(int? id, Patient incoming)
    {
        _mutex.Wait();
        try
        {
            Patient? target = null;
            if (id.HasValue)
            {
                target = _patients.FirstOrDefault(p => p.Id == id.Value);
                if (target is null) return null;
            }

            if (target is null)
            {
                target = new Patient();
                SetId(target, _nextId++);
                _patients.Add(target);
            }

            target.FirstName = incoming.FirstName?.Trim() ?? string.Empty;
            target.LastName = incoming.LastName?.Trim() ?? string.Empty;
            target.Address = incoming.Address?.Trim() ?? string.Empty;
            target.Race = incoming.Race?.Trim();
            target.Gender = incoming.Gender?.Trim();
            target.Birthdate = incoming.Birthdate;

            Persist();
            return target;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        var json = File.ReadAllText(_filePath);
        var loaded = JsonConvert.DeserializeObject<List<Patient>>(json, new DateOnlyJsonConverter()) ?? new List<Patient>();

        _patients.Clear();
        _patients.AddRange(loaded);
        _nextId = (_patients.Count == 0) ? 1 : _patients.Max(p => p.Id) + 1;
    }

    private void Persist()
    {
        var json = JsonConvert.SerializeObject(_patients, Formatting.Indented, new DateOnlyJsonConverter());
        File.WriteAllText(_filePath, json);
    }

    private static void SetId(Patient patient, int id)
    {
        var prop = typeof(Patient).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(patient, id);
    }
}
