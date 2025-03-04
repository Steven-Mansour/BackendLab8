using System;
using System.Collections.Generic;

namespace Course.Models;

public partial class Course
{
    public string CourseName { get; set; } = null!;

    public int CourseId { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
