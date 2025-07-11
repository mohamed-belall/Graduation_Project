namespace Graduation_Project.Core.Models.Clinics
{
    public class Secretary: Person
    {

        //[ForeignKey("ApplicationUser")]
        //public string UserId { get; set; }
        //public ApplicationUser? ApplicationUser { get; set; }

        //[Required(ErrorMessage = "National ID is required.")]
        [StringLength(14, ErrorMessage = "National ID must be 14 characters.")]
        public string? NationalID { get; set; } // National ID of the pharmacist

        // Relationships

        [ForeignKey("clinic")]
        public int clinicId { get; set; }
        public Clinic? clinic { get; set; }
    }
}

