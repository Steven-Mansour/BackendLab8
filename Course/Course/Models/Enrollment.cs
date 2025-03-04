using System;
using System.Collections.Generic;

namespace Course.Models;

public partial class Enrollment
{
    public int Enrollmentid { get; set; }

    public int? Classid { get; set; }

    public int? Studentid { get; set; }

    public virtual Class? Class { get; set; }
}
