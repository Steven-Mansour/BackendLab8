using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DemoLab7.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string ProfilePictureUrl { get; set; }
    [JsonIgnore]
    public virtual List<Class> Classes { get; set; } = new List<Class>();
}
