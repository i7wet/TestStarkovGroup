using FileHelpers;

namespace TestStarkovGroup.DTO;

[DelimitedRecord("|")]
public class JobTitleDTO
{
     public string? Name { get; set; }
     
     public List<string> Validate()
     {
          var errors = new List<string>();
          
          if (String.IsNullOrEmpty(Name) || String.IsNullOrWhiteSpace(Name))
               errors.Add("Наимеование не указано.");

          return errors;
     }
}