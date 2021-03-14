using System.Collections.Generic;

namespace doctor.Models.Consults.General
{
    public class CategoryStudies
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public List<Studies> StudiesList { get; set; }

        public class Studies
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}