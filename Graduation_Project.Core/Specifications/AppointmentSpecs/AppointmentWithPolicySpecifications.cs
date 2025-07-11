using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    public class AppointmentWithPolicySpecifications: BaseSpecifications<Appointment>
    {

        /*
         Purpose: Retrieves a specific appointment by Id and includes its Policy.

         Relation: Helps in cases where policy info tied to appointment needs to be retrieved 
         (e.g., if patient wnna to cancel ).

         */
        public AppointmentWithPolicySpecifications(int id)
            : base(a =>
                a.Id == id
            )
        {
            Includes.Add(a => a.Policy);
        }
    }
}
