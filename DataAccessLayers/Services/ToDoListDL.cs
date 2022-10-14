using CommonLayer.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class ToDoListDL : IToDoListDL
    {
        public readonly IConfiguration _configuration;
        public readonly ILogger<ToDoListDL> _logger;
        public readonly MySqlConnection _mySqlConnection;
        public ToDoListDL(IConfiguration configuration, ILogger<ToDoListDL> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _mySqlConnection = new MySqlConnection(_configuration["ConnectionStrings:MySqlDBString"]);
        }

        public async Task<AddNoteResponse> AddNote(AddNoteRequest request)
        {
            AddNoteResponse response = new AddNoteResponse();
            response.IsSuccess = true;
            response.Message = "Inserção de Anotação feita com Sucesso.";

            try
            {

                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string SqlQuery = @"INSERT INTO NoteDetails(CreatedDate, Note, DataAgendada, HoraAgendada, Segunda, Terca, Quarta, Quinta, Sexta, Sabado, Domingo) 
                                    VALUES (@CreatedDate, @Note, @DataAgendada, @HoraAgendada, @Segunda, @Terca, @Quarta, @Quinta, @Sexta, @Sabado, @Domingo)";

                using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@CreatedDate",DateTime.Now.ToString("MMMM, dd-MM-yyyy HH:mm tt"));
                    sqlCommand.Parameters.AddWithValue("@Note", request.Note);
                    sqlCommand.Parameters.AddWithValue("@DataAgendada", String.IsNullOrEmpty(request.DataAgendada)?null: request.DataAgendada);
                    sqlCommand.Parameters.AddWithValue("@HoraAgendada", String.IsNullOrEmpty(request.HoraAgendada) ? null : request.HoraAgendada);
                    sqlCommand.Parameters.AddWithValue("@Segunda", request.Segunda);
                    sqlCommand.Parameters.AddWithValue("@Terca", request.Terca);
                    sqlCommand.Parameters.AddWithValue("@Quarta", request.Quarta);
                    sqlCommand.Parameters.AddWithValue("@Quinta", request.Quinta);
                    sqlCommand.Parameters.AddWithValue("@Sexta", request.Sexta);
                    sqlCommand.Parameters.AddWithValue("@Sabado", request.Sabado);
                    sqlCommand.Parameters.AddWithValue("@Domingo", request.Domingo);
                    int Status = await sqlCommand.ExecuteNonQueryAsync();
                    if (Status <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Query Não Executada";
                        _logger.LogError("Error Occur : Query Não Executada");
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<GetNoteResponse> GetNote(GetNoteRequest request)
        {
            GetNoteResponse response = new GetNoteResponse();
            response.IsSuccess = true;
            response.Message = "Fetch Data Successfully.";

            try
            {

                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                int Offset = (request.PageNumber - 1) * request.NumberOfRecordPerPage;

                string SqlQuery = string.Empty;

                
                SqlQuery = @" SELECT Id, CreatedDate, Note, DataAgendada, HoraAgendada, Segunda, Terca, Quarta, Quinta, Sexta, Sabado, Domingo,
                                  (SELECT COUNT(*) FROM NoteDetails) AS TotalRecord
                                  From NoteDetails 
                                  Order By Id " + request.SortBy.ToUpperInvariant()+@"
                                  LIMIT @Offset, @NumberOfRecordPerPage";
                
                

                using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Offset", Offset);
                    sqlCommand.Parameters.AddWithValue("@NumberOfRecordPerPage", request.NumberOfRecordPerPage);
                    using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (dataReader.HasRows)
                        {
                            int Count = 0;
                            response.data = new List<GetNote>();
                            while(await dataReader.ReadAsync())
                            {
                                response.data.Add(
                                    new GetNote()
                                    {
                                        NoteId = dataReader["Id"] != DBNull.Value ? (Int32)dataReader["Id"] : -1,
                                        Note = dataReader["Note"] != DBNull.Value ? (string)dataReader["Note"] : null,
                                        DataAgendada = dataReader["ScheduleDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["DataAgendada"]).ToString("dd/MM/yyyy") : null,
                                        HoraAgendada = dataReader["ScheduleTime"] != DBNull.Value ? Convert.ToDateTime(dataReader["HoraAgendada"]).ToString("hh:mm tt") : null,
                                        Segunda = dataReader["Segunda"] != DBNull.Value ? Convert.ToBoolean(dataReader["Segunda"]) : false,
                                        Terca = dataReader["Terca"] != DBNull.Value ? Convert.ToBoolean(dataReader["Terca"]) : false,
                                        Quarta= dataReader["Quarta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Quarta"]) : false,
                                        Quinta= dataReader["Quinta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Quinta"]) : false,
                                        Sexta= dataReader["Sexta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Sexta"]) : false,
                                        Sabado= dataReader["Sabado"] != DBNull.Value ? Convert.ToBoolean(dataReader["Sabado"]) : false,
                                        Domingo= dataReader["Domingo"] != DBNull.Value ? Convert.ToBoolean(dataReader["Domingo"]) : false,
                                    }) ;

                                if (Count == 0)
                                {
                                    Count++;
                                    response.TotalRecords = dataReader["TotalRecord"] != DBNull.Value ? Convert.ToInt32(dataReader["TotalRecord"]) : -1;
                                    response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(response.TotalRecords / request.NumberOfRecordPerPage)));
                                    response.CurrentPage = request.PageNumber;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<GetNoteByIdResponse> GetNoteById(string Id)
        {
            GetNoteByIdResponse response = new GetNoteByIdResponse();
            response.IsSuccess = true;
            response.Message = "Busca de Anotação por Id Com Sucesso";

            try
            {

                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string SqlQuery = @"SELECT * FROM NoteDetails WHERE Id=@Id";//Id, CreatedDate, Note, HoraAgendada

                using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Id", Id);
                    using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
                    {
                        if (dataReader.HasRows)
                        {
                            await dataReader.ReadAsync();
                            response.data = new AddNoteRequest();
                            response.data.Id = dataReader["Id"] != DBNull.Value ? (Int32)dataReader["Id"] : -1;
                            response.data.Note = dataReader["Note"] != DBNull.Value ? (string)dataReader["Note"] : null;
                            response.data.DataAgendada = dataReader["DataAgendada"] != DBNull.Value ? (string)dataReader["DataAgendada"] : null;
                            response.data.HoraAgendada = dataReader["HoraAgendada"] != DBNull.Value ? (string)dataReader["HoraAgendada"] : null;
                            response.data.Segunda = dataReader["Segunda"] != DBNull.Value ? Convert.ToBoolean(dataReader["Segunda"]) : false;
                            response.data.Terca = dataReader["Terca"] != DBNull.Value ? Convert.ToBoolean(dataReader["Terca"]) : false;
                            response.data.Quarta = dataReader["Quarta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Quarta"]) : false;
                            response.data.Quinta = dataReader["Quinta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Quinta"]) : false;
                            response.data.Sexta = dataReader["Sexta"] != DBNull.Value ? Convert.ToBoolean(dataReader["Sexta"]) : false;
                            response.data.Sabado = dataReader["Sabado"] != DBNull.Value ? Convert.ToBoolean(dataReader["Sabado"]) : false;
                            response.data.Domingo = dataReader["Domingo"] != DBNull.Value ? Convert.ToBoolean(dataReader["Domingo"]) : false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<UpdateNoteResponse> UpdateNote(AddNoteRequest request)
        {
            UpdateNoteResponse response = new UpdateNoteResponse();
            response.IsSuccess = true;
            response.Message = "Update de Anotação feita com Sucesso.";
            try
            {

                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string SqlQuery = @"
                                    UPDATE todolist.notedetails
                                    SET UpdatedDate=@UpdatedDate, 
                                        Note=@Note, 
                                        DataAgendada=@DataAgendada, 
                                        HoraAgendada=@HoraAgendada,
                                        Segunda=@Segunda, 
                                        Terca=@Terca, 
                                        Quarta=@Quarta, 
                                        Quinta=@Quinta, 
                                        Sexta=@Sexta, 
                                        Sabado=@Sabado, 
                                        Domingo=@Domingo
                                    WHERE Id=@Id
                                    ";//Id, CreatedDate, Note, HoraAgendada

                using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Id", request.Id);
                    sqlCommand.Parameters.AddWithValue("@UpdatedDate", DateTime.Now.ToString("MMMM, dd-MM-yyyy HH:mm tt"));
                    sqlCommand.Parameters.AddWithValue("@Note", request.Note);
                    sqlCommand.Parameters.AddWithValue("@DataAgendada", request.DataAgendada);
                    sqlCommand.Parameters.AddWithValue("@HoraAgendada", request.HoraAgendada);
                    sqlCommand.Parameters.AddWithValue("@Segunda", request.Segunda);
                    sqlCommand.Parameters.AddWithValue("@Terca", request.Terca);
                    sqlCommand.Parameters.AddWithValue("@Quarta", request.Quarta);
                    sqlCommand.Parameters.AddWithValue("@Terca", request.Terca);
                    sqlCommand.Parameters.AddWithValue("@Sexta", request.Sexta);
                    sqlCommand.Parameters.AddWithValue("@Sabado", request.Sabado);
                    sqlCommand.Parameters.AddWithValue("@Domingo", request.Domingo);
                    int Status = await sqlCommand.ExecuteNonQueryAsync();
                    if (Status <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Query Não Executada";
                        _logger.LogError("Error Occur : Query Não Executada");
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<DeleteNoteResponse> DeleteNote(string Id)
        {
            DeleteNoteResponse response = new DeleteNoteResponse();
            response.IsSuccess = true;
            response.Message = "Anotação Deletada com Sucesso";

            try
            {

                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string SqlQuery = @"DELETE FROM todolist.notedetails WHERE Id=@Id";//Id, CreatedDate, Note, DataAgendada

                using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.CommandTimeout = 180;
                    sqlCommand.Parameters.AddWithValue("@Id", Id);
                    int Status = await sqlCommand.ExecuteNonQueryAsync();
                    if (Status <= 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Query Não Executada";
                        _logger.LogError("Error Occur : Query Não Executada");
                        return response;
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

    }
}
