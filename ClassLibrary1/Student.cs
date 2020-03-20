
namespace ClassLibrary
{
    public class Student : Person
    {
        public string IndexNo { get; set; }
        public string Faculty { get; set; }
        public void SetStudent(int id, string name, string lastName, string indexNo, string faculty)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            IndexNo = indexNo;
            Faculty = faculty;
        }
    }
}
