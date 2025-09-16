using System;
using System.Globalization;

using Library.MedicalPractice.Models;
using Library.MedicalPractice.Services;

namespace CLI.MedicalPractice
{
    public class Program
    {
        //int main
        static void Main(string[] args)
        {
            Console.WriteLine("Medical Practice CLI");

            bool cont = true;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Main Menu");
                Console.WriteLine("1. Patients Menu");
                Console.WriteLine("2. Physicians Menu");
                Console.WriteLine("3. Appointments Menu");
                Console.WriteLine("4. Medical Notes Menu");
                Console.WriteLine("0. Quit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        PatientsMenu();
                        break;
                    case "2":
                        PhysiciansMenu();
                        break;
                    case "3":
                        AppointmentsMenu();
                        break;

                    case "4":
                        MedicalNotes();
                        break;
                    //MAKE SURE TO RETURN HERE FOR THE REST OF THE PHYS MENU APT MENU AND NOTES MENU
                    case "0":
                        cont = false;
                        break;
                }
            } while (cont);
        }

//________________________Patients______________________________________
        private static void PatientsMenu()
        {
            bool cont = true;

            do
            {
                Console.WriteLine();
                Console.WriteLine("---------Patients---------");
                Console.WriteLine("C. Create New Patient");
                Console.WriteLine("R. List All Patients");
                Console.WriteLine("U. Update a Patient");
                Console.WriteLine("D. Delete a Patient");
                Console.WriteLine("B. Back");
                Console.WriteLine("Select: ");

                var input = Console.ReadLine();

                switch (input?.ToLowerInvariant())
                {
                    case "c":
                        CreatePatient(); break;
                    case "r":
                        ListPatients(); break;
                    case "u":
                        UpdatePatient(); break;
                    case "d":
                        DeletePatient(); break;
                    case "b":
                        cont = false;
                        break;
                        
                        ////RETURN HERE FOR THE REST OF THE CASES                       
                }


            } while (cont);
        }
        private static void CreatePatient()
        {
            var p = new Patient();
            Console.Write("First Name: ");
            p.FirstName = Console.ReadLine() ?? "";

            Console.Write("Last Name: ");
            p.LastName = Console.ReadLine() ?? "";

            Console.Write("Race: ");
            p.Race = Console.ReadLine() ?? "";

            Console.Write("Address: ");
            p.Address = Console.ReadLine() ?? "";

            Console.Write("Gender: ");
            p.Gender = Console.ReadLine() ?? "";

            p.Birthdate = PromptDateOnly("Birthdate (YYYY-MM-DD): ");

            PatientServiceProxy.Current.AddOrUpdate(p);
            Console.WriteLine("Created: ");
            Console.WriteLine(p);


        }

        private static void ListPatients()
        {
            var list = PatientServiceProxy.Current.Patients;
            if (!list.Any())
            {
                Console.WriteLine("(No Patients)");
                return;
            }
            list.ForEach(Console.WriteLine);
        }

        private static void UpdatePatient()
        {
            ListPatients();
            Console.Write("Patient ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var p = PatientServiceProxy.Current.Patients.FirstOrDefault(x => x?.Id == id);

            if (p is null)
            {
                Console.WriteLine("Not Found.");
                return;
            }

            Console.WriteLine("---If you want to leave what is originally there just hit enter, to not overwrite fields---");

            Console.Write($"First Name [{p.FirstName}]: ");
            var fn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(fn))
                p.FirstName = fn;

            Console.Write($"Last Name [{p.LastName}]: ");
            var ln = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ln))
                p.LastName = ln;

            Console.Write($"Address [{p.Address}]: ");
            var addr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(addr))
                p.Address = addr;

            Console.Write($"Birthdate[{p.Birthdate} YYYY-MM-DD]: ");
            var bd = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(bd) && DateOnly.TryParse(bd, out var dto))
                p.Birthdate = dto;

            Console.Write($"Race [{p.Race}]: ");
            var race = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(race))
                p.Race = race;

            Console.Write($"Gender [{p.Gender}]: ");
            var gen = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(gen))
                p.Gender = gen;

            PatientServiceProxy.Current.AddOrUpdate(p);
            Console.WriteLine("Update: ");
            Console.WriteLine(p);
        }

        private static void DeletePatient()
        {
            ListPatients();
            Console.Write("Patient Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var deleted = PatientServiceProxy.Current.Delete(id);
            Console.WriteLine(deleted is null ? "Not found. " : $"Deleted: {deleted}");
        }

//__________________________Physicians________________________________________
        private static void PhysiciansMenu()
        {
             bool cont = true;

            do
            {
                Console.WriteLine();
                Console.WriteLine("---------Physicians---------");
                Console.WriteLine("C. Create New Physicians");
                Console.WriteLine("R. List All Physicians");
                Console.WriteLine("U. Update a Physician");
                Console.WriteLine("D. Delete a Physician");
                Console.WriteLine("B. Back");
                Console.WriteLine("Select: ");

                var input = Console.ReadLine();

                switch (input?.ToLowerInvariant())
                {
                    case "c":
                        CreatePhysician(); break;
                    case "r":
                        ListPhysicians(); break;
                    case "u":
                        UpdatePhysician(); break;
                    case "d":
                        DeletePhysician(); break;
                    case "b":
                        cont = false;
                        break;
                        
                        ////RETURN HERE FOR THE REST OF THE CASES                       
                }


            } while (cont);
        }





        private static void CreatePhysician()
        {
            var d = new Physician();

            Console.Write("First Name: ");
            d.FirstName = Console.ReadLine() ?? "";

            Console.Write("Last Name: ");
            d.LastName = Console.ReadLine() ?? "";

            Console.Write("License Number: ");
            d.LicenseNumber = Console.ReadLine() ?? "";

            d.Graduation = PromptDateOnly("Graduation Date (YYYY-MM-DD): ");

            Console.Write("Specialization: ");
            d.Specialization = Console.ReadLine() ?? "";

            try
            {
                PhysicianServiceProxy.Current.AddOrUpdate(d);
                Console.WriteLine("Created: ");
                Console.WriteLine(d);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            

        }

        private static void ListPhysicians()
        {
            var list = PhysicianServiceProxy.Current.Physicians;
            if (!list.Any())
            {
                Console.WriteLine("(No Physicians)");
                return;
            }
            list.ForEach(Console.WriteLine);
            
        }

        private static void UpdatePhysician()
        {
            ListPhysicians();
            Console.Write("Physician Id to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var d = PhysicianServiceProxy.Current.Physicians.FirstOrDefault(x => x?.Id == id);
            if (d is null)
            {
                Console.WriteLine("Not found.");
                return;
            }
        
            Console.WriteLine("---If you want to leave what is originally there just hit enter, to not overwrite fields---");

            Console.Write($"First Name [{d.FirstName}]: ");
            var fn = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(fn))
                d.FirstName = fn;

            Console.Write($"Last Name [{d.LastName}]: ");
            var ln = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ln))
                d.LastName = ln;

            Console.Write($"License Numb er [{d.LicenseNumber}]: ");
            var lic = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(lic))
                d.LicenseNumber = lic;

            Console.Write($"Graduation Date[{d.Graduation} YYYY-MM-DD]: ");
            var gd = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(gd) && DateOnly.TryParse(gd, out var dto))
                d.Graduation = dto;

            Console.Write($"Specialization [{d.Specialization}]: ");
            var spec = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(spec))
                d.Specialization = spec;

            try
            {
                PhysicianServiceProxy.Current.AddOrUpdate(d);
                Console.WriteLine("Updated: ");
                Console.WriteLine(d);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        

        }

        private static void DeletePhysician()
        {
            ListPhysicians();
            Console.Write("Physician Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var deleted = PhysicianServiceProxy.Current.Delete(id);
            Console.WriteLine(deleted is null ? "Not found. " : $"Deleted: {deleted}");
        }



        //---------------Appointments------------------------
        private static void AppointmentsMenu()
        {
             bool cont = true;

            do
            {
                Console.WriteLine();
                Console.WriteLine("---------Appointment---------");
                Console.WriteLine("C. Create New Appointment");
                Console.WriteLine("R. List All Appointments");
                Console.WriteLine("U. Update an Appointment");
                Console.WriteLine("D. Delete an Appointment");
                Console.WriteLine("B. Back");
                Console.WriteLine("Select: ");

                var input = Console.ReadLine();

                switch (input?.ToLowerInvariant())
                {
                    case "c":
                        CreateAppointment(); break;
                    case "r":
                        ListAppointment(); break;
                    case "u":
                        UpdateAppointment(); break;
                    case "d":
                        DeleteAppointment(); break;
                    case "b":
                        cont = false;
                        break;
                        
                        ////RETURN HERE FOR THE REST OF THE CASES                       
                }


            } while (cont);
        }
        private static void CreateAppointment()
        {
            if (!EnsureOneAndOne())
                return;

            Console.WriteLine("Select Patient: "); ;
            ListPatients();
            Console.Write("Patient Id: ");
            if (!int.TryParse(Console.ReadLine(), out int pid))
                return;

            Console.WriteLine("Select Physician: ");
            ListPhysicians();
            Console.WriteLine("Physician Id: ");
            if (!int.TryParse(Console.ReadLine(), out int did))
                return;

            var date = PromptDateOnly("Appointment Date (YYYY-MM-DD, Mon - Fri): ");

            var start = PromptTime("Start Time (HH:mm, 24h, after 8:00)");
            var end = PromptTime("End Time (HH:mm, 24h, <= 17:00): ");

            var startDt = date.ToDateTime(start);
            var endDt = date.ToDateTime(end);

            var a = new Appointment
            {
                PatientId = pid,
                PhysicianId = did,
                StartLocal = startDt,
                EndLocal = endDt

            };

            try
            {
                AppointmentServiceProxy.Current.AddOrUpdate(a);
                Console.WriteLine("Created: ");
                Console.WriteLine(a);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ListAppointment()
        {
            var list = AppointmentServiceProxy.Current.Appointments;
            if (!list.Any())
            {
                Console.WriteLine("(no appointments)");
                return;
            }
            list.ForEach(Console.WriteLine);
        }
        
        private static void UpdateAppointment()
        {
            ListAppointment();
            Console.Write("Appointment Id to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var a = AppointmentServiceProxy.Current.Appointments.FirstOrDefault(x => x?.Id == id);
            if (a is null)
            {
                Console.WriteLine("Not found.");
                return;
            }

            Console.WriteLine($"Current: {a}");
            var date = PromptDateOnly($"New Date [{a.StartLocal:yyyy-MM-dd}] (blank=keep): ", allowBlank: true);
            var start = PromptTime($"New Start Time [{a.StartLocal:HH:mm}] (blank=keep): ");
            var end = PromptTime($"New End Time [{a.EndLocal:HH:mm}] (blank=keep): ");

            var useDate = date == default ? DateOnly.FromDateTime(a.StartLocal) : date;
            var useStart = start;        
            var useEnd = end;

            a.StartLocal = useDate.ToDateTime(useStart);
            a.EndLocal = useDate.ToDateTime(useEnd);

            try
            {
                AppointmentServiceProxy.Current.AddOrUpdate(a);
                Console.WriteLine("Updated:");
                Console.WriteLine(a);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void DeleteAppointment()
        {
            ListAppointment();
            Console.Write("Appointment Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            var deleted = AppointmentServiceProxy.Current.Delete(id);
            Console.WriteLine(deleted is null ? "Not found." : $"Deleted: {deleted}");
        }

        //__________________Medical Notes________________________________

        private static void MedicalNotes()
        {
            bool cont = true;
            do
            {
                Console.WriteLine();
                Console.WriteLine("---Medical Notes----");
                Console.WriteLine("C. Create a New Note");
                Console.WriteLine("R. List All Notes");
                Console.WriteLine("F. List by Patient");
                Console.WriteLine("D. Delete Notes");
                Console.WriteLine("B. Back");
                Console.Write("Select: ");
                var input = Console.ReadLine();

                switch (input?.ToLowerInvariant())
                {
                    case "c":
                        CreateNote();
                        break;
                    case "r":
                        ListNotes();
                        break;
                    case "f":
                        ListNotesByPatient();
                        break;
                    case "d":
                        DeleteNote();
                        break;
                    case "b":
                        cont = false;
                        break;
                    default: Console.WriteLine("Invalid command"); break;
                }
            } while (cont);
        
        }

        private static void CreateNote()
        {
            if (!EnsureOneAndOne())
                return;

            Console.WriteLine("Select Patient:");
            ListPatients();
            Console.Write("Patient Id: ");
            if (!int.TryParse(Console.ReadLine(), out int pid)) return;

            Console.WriteLine("Select Physician:");
            ListPhysicians();
            Console.Write("Physician Id: ");
            if (!int.TryParse(Console.ReadLine(), out int did)) return;

            Console.Write("Diagnoses: "); var dx = Console.ReadLine() ?? "";
            Console.Write("Prescriptions: "); var rx = Console.ReadLine() ?? "";

            var note = new MedicalNote
            {
                PatientId = pid,
                PhysicianId = did,
                Diagnoses = dx,
                Prescriptions = rx
            };

            try
            {
                MedicalNoteServiceProxy.Current.AddOrUpdate(note);
                Console.WriteLine("Created:");
                Console.WriteLine(note);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ListNotes()
        {
            var list = MedicalNoteServiceProxy.Current.Notes;
            if (!list.Any())
            {
                Console.WriteLine("(no notes)");
                return;
            }
            list.ForEach(Console.WriteLine);
        }

        private static void ListNotesByPatient()
        {
            Console.Write("Patient Id: ");
            if (!int.TryParse(Console.ReadLine(), out int pid))
                return;

            var list = MedicalNoteServiceProxy.Current.Notes
                .Where(n => n?.PatientId == pid)
                .ToList();

            if (!list.Any())
            {
                Console.WriteLine("(no notes for that patient)");
                return;
            }
            list.ForEach(Console.WriteLine);
        }

        private static void DeleteNote()
        {
            ListNotes();
            Console.Write("Note Id to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
                return;

            var deleted = MedicalNoteServiceProxy.Current.Delete(id);
            Console.WriteLine(deleted is null ? "Not found." : $"Deleted: {deleted}");
        }

        
        //_____________________Helper Funcs______________________________

        private static bool EnsureOneAndOne()
        {
            if (!PatientServiceProxy.Current.Patients.Any())
            {
                Console.WriteLine("No Patients exist. Create a patient first.");
                return false;
            }
            if (!PhysicianServiceProxy.Current.Physicians.Any())
            {
                Console.WriteLine("No Physicians exist. Create a Physician First.");
                return false;
            }

            return true;
        }

        private static TimeOnly PromptTime(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();

                if (TimeOnly.TryParseExact(s, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) || TimeOnly.TryParse(s, out t))
                {
                    return t;
                }
                    
                Console.WriteLine("INvalid time. Expected HH:mm (24-hour).");
            }
        }
        private static DateOnly PromptDateOnly(string prompt, bool allowBlank = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (allowBlank && string.IsNullOrWhiteSpace(s))
                    return default;

                if (DateOnly.TryParse(s, out var dto))
                    return dto;

                Console.WriteLine("Invalid date. Expected YYYY-MM-DD.");
            }
        }
    }
}