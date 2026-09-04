using System.ComponentModel.DataAnnotations;



namespace AutoStockIQ.Models.ViewModels;



public class OrderDecisionViewModel

{

    [Required]

    public int OrderId { get; set; }



    [StringLength(500)]

    public string? Note { get; set; }

}

