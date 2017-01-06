using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadSql
{
    public class Student : IComparable
    {
        public int Id { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public Student(int id, string firstName, string lastName, int age, string email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Email = email;
        }
        public Student(int id)
        {
            Id = id;
        }
        public int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(Student))
            {
                return Id.CompareTo(((Student)obj).Id);
            }
            return 0;
        }
    }
}
