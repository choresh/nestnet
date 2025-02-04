using GlobalE.Payments.Manager.Core.Data;

namespace GlobalE.Payments.Manager.Core.Resources.ChargeBack.Dtos
{
    /// <summary>
    /// This is a sample DTO class.
    /// </summary>
    public class SampleOutputDto
    {
        // Add here your required properties.

        public required Task<List<DisputeWithHistory>> DisputesWithHistory {  get; set; }
    }
}