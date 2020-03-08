using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeLuau
{
    /// <summary>
    /// Represents a single speaker
    /// </summary>
    public class Speaker
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? YearsExperience { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<Session> Sessions { get; set; }

        /// <summary>
        /// Register a speaker
        /// </summary>
        /// <returns>speakerID</returns>
        public RegisterResponse Register(IRepository repository)
        {
            int? speakerId = null;

            var error = ValidateRegistration();
            if (error != null) return new RegisterResponse(error);

            // We assume  calls the db to find pricing hence if else deleted
            try
            {
                speakerId = repository.SaveSpeaker(this);
            }
            catch (Exception e)
            {
                throw new Exception("Db save failed");
            }

            return new RegisterResponse((int)speakerId);
        }
        private RegisterError? ValidateRegistration()
        {

            var error = this.ValidateData();
            if (error != null)
            {
                return error;
            }

            bool speakerAppearsQualified = AppearsExceptional() || !HasRedFlags();

            if (!speakerAppearsQualified) return RegisterError.SpeakerDoesNotMeetStandards;

            bool approved = ClassifiesAsOldTechnology();

            if (!approved) return RegisterError.NoSessionsApproved;

            return null;
        }

        private bool ClassifiesAsOldTechnology()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !SessionIsOldTechnology(session);
            }

            return Sessions.Any(s => s.Approved);
        }

        private static bool SessionIsOldTechnology(Session session)
        {
            var oldTechnology = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

            foreach (var tech in oldTechnology)
            {
                if (session.Title.Contains(tech) || session.Description.Contains(tech)) return true;
            }

            return false;
        }

        private bool HasRedFlags()
        {
            var ancientDomains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };
            string emailDomain = Email.Split('@').Last();
            return ancientDomains.Contains(emailDomain) || (Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9);
        }

        private bool AppearsExceptional()
        {
            if (YearsExperience > 10) return true;
            if (HasBlog) return true;
            if (Certifications.Count() > 3) return true;

            var emps = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            if (emps.Contains(Employer)) return true;
            return false;
        }

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName)) return RegisterError.FirstNameRequired;
            if (string.IsNullOrWhiteSpace(LastName)) return RegisterError.LastNameRequired;
            if (string.IsNullOrWhiteSpace(Email)) return RegisterError.EmailRequired;
            if (!Sessions.Any()) return RegisterError.NoSessionsProvided;
            return null;
        }
    }
}