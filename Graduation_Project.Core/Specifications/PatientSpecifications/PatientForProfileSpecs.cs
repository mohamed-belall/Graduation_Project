﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.PatientSpecifications
{
    public class PatientForProfileSpecs : BaseSpecifications<Patient>
    {
        public PatientForProfileSpecs(int id):base(p => p.Id == id)
        {
            
        }
    }
}
