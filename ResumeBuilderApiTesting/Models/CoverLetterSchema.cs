namespace ResumeBuilderApiTesting.Models
{
    public class CoverLetterSchema
    {
        public Applicant Applicant { get; set; }

        public Company Company { get; set; } 

        public Letter Letter { get; set; } 
    }

    public class Applicant
    {
        public string Name { get; set; } 

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }
    }

    public class Company
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string HiringManager { get; set; }
    }

    public class Letter
    {
        public string Date { get; set; }

        public string Subject { get; set; }

        public string Salutation { get; set; } 

        public string Opening { get; set; }

        public List<string> Body { get; set; }
        public string Closing { get; set; }

        public string SignatureName { get; set; }

    }
}
