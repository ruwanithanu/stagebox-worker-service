using System;

namespace Models
{
    public class AEAppParameter
    {
        public int Id { get; set; }
        public int AppTypeId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string OrgValue { get; set; }
        public int Status { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
