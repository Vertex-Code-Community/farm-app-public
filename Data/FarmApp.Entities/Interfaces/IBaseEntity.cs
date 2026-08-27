using System.ComponentModel.DataAnnotations;

namespace FarmApp.Entities.Interfaces;

public interface IBaseEntity<TId>
{
    [Key] public TId Id { get; set; }
}