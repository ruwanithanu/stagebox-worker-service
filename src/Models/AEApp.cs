using System;

namespace Models
{
    public class AEApp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdentityId { get; set; }
        public int AppTypeId { get; set; }
        public int EnvironmentId { get; set; }
        public int Status { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
