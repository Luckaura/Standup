using Microsoft.Extensions.Hosting;

namespace Komikai.Data.Entities
{
    public class Comment
    {
        // Guid
        // Ulid
        public int Id { get; set; }
        public required string Content { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }

        public Set Set { get; set; }

        public CommentDto ToDto()
        {
            return new CommentDto(Id, Content, CreatedAt);
        }
    }
}
