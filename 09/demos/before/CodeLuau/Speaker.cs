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

            var emps = new List<string>() { "Pluralsight", "Microsoft", "Google" };

            bool speakerAppearsQualified = YearsExperience > 10 || HasBlog || Certifications.Count() > 3 || emps.Contains(Employer);

            if (!speakerAppearsQualified)
            {
                var domains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };

                string emailDomain = Email.Split('@').Last();

                if (!domains.Contains(emailDomain) && (!(Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)))
                {
                    speakerAppearsQualified = true;
                }
            }

            if (speakerAppearsQualified)
            {
                var approved = false;
                if (Sessions.Count() != 0)
                {
                    foreach (var session in Sessions)
                    {
                        var ot = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

                        foreach (var tech in ot)
                        {
                            if (session.Title.Contains(tech) || session.Description.Contains(tech))
                            {
                                session.Approved = false;
                                break;
                            }
                            else
                            {
                                session.Approved = true;
                                approved = true;
                            }
                        }
                    }
                }
                else
                {
                    return new RegisterResponse(RegisterError.NoSessionsProvided);
                }

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
            }
            else
            {
                return new RegisterResponse(RegisterError.SpeakerDoesNotMeetStandards);
            }

            return new RegisterResponse((int)speakerId);
        }

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                return RegisterError.FirstNameRequired;
            }
            else if (string.IsNullOrWhiteSpace(LastName))
            {
                return RegisterError.LastNameRequired;
            }
            else if (string.IsNullOrWhiteSpace(Email))
            {
                return RegisterError.EmailRequired;
            }
            else
            {
                return null;
            }
        }
    }
}