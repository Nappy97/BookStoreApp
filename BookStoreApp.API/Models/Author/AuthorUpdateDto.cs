using System.ComponentModel.DataAnnotations;

namespace BookStoreApp.API.Models.Author;

public class AuthorUpdateDto: BaseDto
{
    [Required(ErrorMessage = "첫번째 이름은 필수 값입니다.")]
    [StringLength(50, ErrorMessage = "첫번째 이름은 50자를 넘을 수 없습니다.")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "두번째 이름은 필수 값입니다.")]
    [StringLength(50, ErrorMessage = "두번째 이름은 50자를 넘을 수 없습니다.")]
    public string LastName { get; set; }
    
    [StringLength(250, ErrorMessage = "소개는 250자를 넘을 수 없습니다.")]
    public string Bio { get; set; }
}