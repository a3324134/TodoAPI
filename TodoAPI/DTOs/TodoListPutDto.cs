using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.Abstracts;

namespace TodoAPI.DTOs
{
    public class TodoListPutDto : TodoListEditDtoAbstract
    { 
        public Guid TodoId { get; set; }
        

    }
}
