namespace ResumeBuilderApiTesting.Models
{
    public class ResumeSchema
    {
        public string name { get; set; }
        public string title { get; set; }
        public Contact contact { get; set; }
        public string profile { get; set; }
        public List<string> skills { get; set; }
        public List<Language> languages { get; set; }
        public List<Education> education { get; set; }
        public List<WorkExperience> work_experience { get; set; }
        public List<Reference> references { get; set; }
    }

    public class Contact
    {
        public string phone { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string website { get; set; }
    }

    public class Language
    {
        public string language { get; set; }
        public string proficiency { get; set; }
    }

    public class Education
    {
        public string institution { get; set; }
        public string degree { get; set; }
        public string gpa { get; set; }
        public string years { get; set; }
    }

    public class WorkExperience
    {
        public string company { get; set; }
        public string role { get; set; }
        public string years { get; set; }
        public List<string> responsibilities { get; set; }
    }

    public class Reference
    {
        public string name { get; set; }
        public string company_role { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

}
