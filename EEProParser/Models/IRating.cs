using System;
using System.Collections.Generic;
using System.Text;

namespace Impartial.Models
{
    public interface IRating
    {
        int Rating { get; set; }
        double Variance { get; set; }
    }
}
