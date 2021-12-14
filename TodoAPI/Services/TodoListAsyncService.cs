using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TodoAPI.DTOs;
using TodoAPI.Models;
using TodoAPI.Parameters;

namespace TodoAPI.Services
{
    public class TodoListAsyncService
    {
        private readonly IWebHostEnvironment _env;
        private readonly TodoContext _todoContext;
        public TodoListAsyncService(TodoContext todoContext, IWebHostEnvironment env)
        {
            _env = env;
            _todoContext = todoContext;
        }
        public async Task<List<TodoListDto>> GetData(TodoSelectParameter value)
        {
            var result = _todoContext.TodoLists
                .Include(a => a.UpdateEmployee)
                .Include(a => a.InsertEmployee)
                .Include(a => a.UploadFiles)
                .Select(a => a);

            if (!string.IsNullOrWhiteSpace(value.name))
                result = result.Where(a => a.Name.Contains(value.name));

            if (value.enable != null)
                result = result.Where(a => a.Enable == value.enable);

            if (value.insertTime != null)
                result = result.Where(a => a.InsertTime.Date == value.insertTime);

            if (value.minOrder != null && value.maxOrder != null)
                result = result.Where(a => a.Orders >= value.minOrder && a.Orders <= value.maxOrder);

            var temp = await result.ToListAsync();
            return temp.Select(a => ItemToDto(a)).ToList();
        }

        public TodoListDto GetDataByTodoId(Guid TodoId)
        {
            var result = (from todo in _todoContext.TodoLists
                          where todo.TodoId == TodoId
                          select new TodoListDto
                          {
                              Enable = todo.Enable,
                              InsertEmployeeName = todo.InsertEmployee.Name,
                              InsertTime = todo.InsertTime,
                              Name = todo.Name,
                              Orders = todo.Orders,
                              TodoId = todo.TodoId,
                              UpdateEmployeeName = todo.UpdateEmployee.Name,
                              UpdateTime = todo.UpdateTime,
                              UploadFiles = (from file in _todoContext.UploadFiles
                                             where file.TodoId == todo.TodoId
                                             select new UploadFileDto
                                             {
                                                 Name = file.Name,
                                                 Src = file.Src,
                                                 TodoId = file.TodoId,
                                                 UploadFileId = file.UploadFileId
                                             }).ToList()
                          }).SingleOrDefault();
            return result;

        }

        public async Task<TodoList> InsertData(TodoListPostDto value)
        {
            TodoList insert = new TodoList
            {
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            };


            foreach (var file in value.UploadFiles)
            {
                insert.UploadFiles.Add(new UploadFile()
                {
                    Src = file.Src,
                    Name = file.Name
                });
            }

            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value);
            await _todoContext.SaveChangesAsync();

            return insert;
        }

        public TodoList InsertDataWithUpload(TodoListPostUpDto value)
        {
            TodoList insert = new TodoList
            {
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            };

            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value.TodoList);
            _todoContext.SaveChanges();

            string rootRoot = _env.ContentRootPath + @"\wwwroot\UploadFiles\" + insert.TodoId + "\\";

            if (!Directory.Exists(rootRoot))
                Directory.CreateDirectory(rootRoot);

            foreach (var file in value.files)
            {
                string fileName = file.FileName;
                using (var stream = System.IO.File.Create(rootRoot + fileName))
                {
                    file.CopyTo(stream);

                    var insert2 = new UploadFile
                    {
                        Name = fileName,
                        Src = "/UploadFiles/" + insert.TodoId + "/" + fileName,
                        TodoId = insert.TodoId
                    };

                    _todoContext.UploadFiles.Add(insert2);
                }
            }
            _todoContext.SaveChanges();
            return insert;
        }

        public int InsertDataBySQL(TodoListPostDto value)
        {
            var name = new SqlParameter("name", value.Name);
            var insertTime = new SqlParameter("insertTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var updateTime = new SqlParameter("updateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var enable = new SqlParameter("enable", value.Enable);
            var orders = new SqlParameter("orders", value.Orders);
            var insertEmployeeId = new SqlParameter("insertEmployeeId", "00000000-0000-0000-0000-000000000001");
            var updateEmployeeId = new SqlParameter("updateEmployeeId", "00000000-0000-0000-0000-000000000001");
            var startTime = new SqlParameter("startTime", value.StartTime);
            var endTime = new SqlParameter("endTime", value.EndTime);


            string sql = @"INSERT INTO [dbo].[TodoList]
           ([Name]
           ,[InsertTime]
           ,[UpdateTime]
           ,[Enable]
           ,[Orders]
           ,[InsertEmployeeId]
           ,[UpdateEmployeeId]
           ,[StartTime]
           ,[EndTime])
           VALUES
           (@name, @insertTime, @updateTime, @enable, @orders, @insertEmployeeId, @updateEmployeeId, @startTime, @endTime)";

            return _todoContext.Database.ExecuteSqlRaw(sql, name, insertTime, updateTime, enable, orders, insertEmployeeId, updateEmployeeId, startTime, endTime);
            
            
        }

        public int UpdateData(Guid id, TodoListPutDto value)
        {
            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {

                update.UpdateTime = DateTime.Now;
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                _todoContext.TodoLists.Update(update).CurrentValues.SetValues(value);
            }
            
            return _todoContext.SaveChanges();
        }

        public int UpdateData(Guid id, JsonPatchDocument value)
        {
            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.UpdateTime = DateTime.Now;
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                value.ApplyTo(update);
            }
            return _todoContext.SaveChanges();
        }
        
        public int DeleteData(Guid id)
        {
            var delete = (from todo in _todoContext.TodoLists
                          where todo.TodoId == id
                          select todo).Include(item => item.UploadFiles).SingleOrDefault();

            if (delete != null)
                _todoContext.TodoLists.Remove(delete);

            return _todoContext.SaveChanges();
        }

        public int DeleteBulkData(string ids)
        {
            var deleteList = JsonSerializer.Deserialize<List<Guid>>(ids);

            var delete = (from todo in _todoContext.TodoLists
                          where deleteList.Contains(todo.TodoId)
                          select todo).Include(item => item.UploadFiles);

            if (delete != null)
                _todoContext.TodoLists.RemoveRange(delete);

            return _todoContext.SaveChanges();
        }

        private static TodoListDto ItemToDto(TodoList item)
        {
            List<UploadFileDto> uploadDto = new List<UploadFileDto>();
            foreach (var tmp in item.UploadFiles)
            {
                UploadFileDto file = new UploadFileDto
                {
                    Name = tmp.Name,
                    Src = tmp.Src,
                    TodoId = tmp.TodoId,
                    UploadFileId = tmp.UploadFileId
                };
                uploadDto.Add(file);
            }
            return new TodoListDto
            {
                Enable = item.Enable,
                InsertEmployeeName = item.InsertEmployee.Name,
                InsertTime = item.InsertTime,
                Name = item.Name,
                Orders = item.Orders,
                TodoId = item.TodoId,
                UpdateEmployeeName = item.UpdateEmployee.Name,
                UpdateTime = item.UpdateTime,
                UploadFiles = uploadDto
            };
        }
    }
}
