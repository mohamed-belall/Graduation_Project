﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.SpecialitySpecifications
{
    public class SpecialitiySpecs : BaseSpecifications<Specialty>
    {
        public SpecialitiySpecs(int id) : base (s => s.Id == id)
        {
            
        }
    }
}
