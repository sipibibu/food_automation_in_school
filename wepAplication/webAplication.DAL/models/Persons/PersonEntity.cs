﻿using System.ComponentModel.DataAnnotations;
using webAplication.DAL.Interfaces;

namespace webAplication.DAL.models
{
    public abstract class PersonEntity : IEntity
    {
        [Key]
        public string Id { get; set; }
        public string? ImageId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public PersonEntity()
        {
        }

    }
}
