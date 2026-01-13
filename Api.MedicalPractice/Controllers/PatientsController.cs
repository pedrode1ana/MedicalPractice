using Api.MedicalPractice.Repositories;
using Library.MedicalPractice.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.MedicalPractice.Controllers;

[ApiController]
[Route("patients")]
public class PatientsController : ControllerBase
{
    private readonly PatientRepository _repo;

    public PatientsController(PatientRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Patient>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Patient> GetById(int id)
    {
        var patient = _repo.GetById(id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public ActionResult<Patient> Create([FromBody] Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName) || string.IsNullOrWhiteSpace(patient.LastName))
            return BadRequest("First and last name are required.");

        var created = _repo.Create(patient);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Patient> Update(int id, [FromBody] Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName) || string.IsNullOrWhiteSpace(patient.LastName))
            return BadRequest("First and last name are required.");

        var updated = _repo.Update(id, patient);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var deleted = _repo.Delete(id);
        return deleted is null ? NotFound() : NoContent();
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<Patient>> Search([FromQuery] string? q)
    {
        var results = _repo.Search(q ?? string.Empty);
        return Ok(results);
    }
}
