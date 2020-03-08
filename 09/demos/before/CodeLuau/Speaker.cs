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

            var error = this.ValidateData();
            if (error != null)
            {
                return new RegisterResponse(error);
            }

            bool speakerAppearsQualified = AppearsExceptional() || !HasRedFlags();

            if (!speakerAppearsQualified) return new RegisterResponse(RegisterError.SpeakerDoesNotMeetStandards);

            bool approved = ClassifiesAsOldTechnology();

            if (approved)
            {
                //if we got this far, the speaker is approved
                //let's go ahead and register him/her now.
                //First, let's calculate the registration fee. 
                //More experienced speakers pay a lower fee.
                if (YearsExperience <= 1)
                {
                    RegistrationFee = 500;
                }
                else if (YearsExperience >= 2 && YearsExperience <= 3)
                {
                    RegistrationFee = 250;
                }
                else if (YearsExperience >= 4 && YearsExperience <= 5)
                {
                    RegistrationFee = 100;
                }
                else if (YearsExperience >= 6 && YearsExperience <= 9)
                {
                    RegistrationFee = 50;
                }
                else
                {
                    RegistrationFee = 0;
                }

                try
                {
                    speakerId = repository.SaveSpeaker(this);
                }
                catch (Exception e)
                {
                    throw new Exception("Db save failed");
                }
            }
            else
            {
                return new RegisterResponse(RegisterError.NoSessionsApproved);
            }

            return new RegisterResponse((int)speakerId);
        }

        private bool ClassifiesAsOldTechnology()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !SessionIsOldTechnology(session);
            }

            return Sessions.Any(s=>s.Approved);
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