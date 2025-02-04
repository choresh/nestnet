namespace SampleApp.Core.Modules.MyModules.Dtos
{
    /// <summary>
    /// * This is an auto-generated DTO class.
    /// * Do not modify this file directly as it will be regenerated.
    /// * To modify the properties, please update properties/attributes at the corresponding entity class ('MyModuleEntity').
    /// </summary>
    public class MyModuleCreateDto
    {
		public required String Name { get; set; }
		public required Int64 Age { get; set; }
		public String? Email { get; set; }
		public required String MyVirtualField { get; set; }
    }
}