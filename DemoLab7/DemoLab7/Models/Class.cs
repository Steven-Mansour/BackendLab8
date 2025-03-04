using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DemoLab7.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public int? CourseId { get; set; }

    public int? TeacherId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Teacher? Teacher { get; set; }
    [JsonIgnore]
    public virtual List<Student> Students { get; set; } = new List<Student>();
}
