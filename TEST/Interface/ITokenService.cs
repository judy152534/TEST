using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEST.Entities;

namespace TEST.Interface
{
  
        public interface ITokenService
        {
            string CreateToken(AppUser user);

        }
}

