using System.ComponentModel.DataAnnotations;

namespace Timeline.Models.Validation
{
    public class PositiveIntegerAttribute : RangeAttribute
    {
        public PositiveIntegerAttribute() : base(1, int.MaxValue)
        {
        }
    }
}

