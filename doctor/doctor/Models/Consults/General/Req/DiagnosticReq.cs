﻿using System.Collections.Generic;

namespace doctor.Models.Consults.General.Req
{
    public class DiagnosticReq
    {
        public string GroupName { get; set; }
        public List<string> List { get; set; }
    }
}