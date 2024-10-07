namespace Komikai.Data.Entities
{
    public class Comedian
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Only can be set/seen by admin
        public bool IsBlocked { get; set; }

        public ComedianDto ToDto()
        {
            return new ComedianDto(Id, Name, Description);
        }
    }
}
