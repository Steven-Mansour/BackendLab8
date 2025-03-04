using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DemoLab7.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;
    [JsonIgnore]
    public virtual List<Class> Classes { get; set; } = new List<Class>();
}
