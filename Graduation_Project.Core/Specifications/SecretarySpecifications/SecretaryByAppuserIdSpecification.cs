using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Core.Specifications.SecretarySpecifications
{
    public class SecretaryByAppuserIdSpecification : BaseSpecifications<Secretary>
    {
        public SecretaryByAppuserIdSpecification(string id): base(s => s.ApplicationUserId == id)
        {
            ThenIncludes.Add(s => s.Include(s => s.clinic).ThenInclude(s => s.Doctor));
        }
    }
}
