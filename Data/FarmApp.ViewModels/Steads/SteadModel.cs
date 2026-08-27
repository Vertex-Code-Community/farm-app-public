namespace FarmApp.ViewModels.Steads;

public class SteadModel
{
    public string Id { get; set; }
    public string? CadNum { get; set; }
    public float Area { get; set; }
    public string AreaUnit { get; set; }
    public string? Address { get; set; }

    public string OwnershipId { get; set; }
    public string PurposeId { get; set; }
    public string CategoryId { get; set; }
}
