namespace KolayCAR.Broker.Domain.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public bool Active { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string PersonalNumber { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public UserTypes UserType { get; set; }
        public MemberTypes MemberType { get; set; }
        public int AgencyId { get; set; }
        public bool DefaultMember { get; set; }
        public string Birthday { get; set; }
    }

    public enum UserTypes
    {
        Individual,
        Corporate
    }

    public enum MemberTypes
    {
        WebSite,
        Agency
    }
}
