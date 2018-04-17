using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

public class UserDefinedTimeZone
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Owner { get; set; }
    public string OwnerId { get; set; }
    public int GmtOffset { get; set; }
}