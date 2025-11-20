public interface ILikeable
{
    public int Id { get; set; }
    public ICollection<Like> Likes { get; set; }
}
