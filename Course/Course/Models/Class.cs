using System;
using System.Collections.Generic;

namespace Course.Models;

public partial class Class
{
    public int Classid { get; set; }

    public int? Teacherid { get; set; }

    public int? Courseid { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
