namespace KolayCAR.Broker.API.Models;

public class SpecialRequestCategory
{
    public int Id { get; set; }
    public int SpecialRequestId { get; set; }
    public int CategoryId { get; set; }
    public SpecialRequest SpecialRequest { get; set; }
}
