using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHakkinda")]
    [Authorize(Roles = "Admin")]
    public class AdminAbout : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminAbout(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            About cs = new();
            string base64Image1 = "", base64Image2 = "";
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new("AboutGet", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!Convert.IsDBNull(reader["Picture1"]))
                                    {
                                        byte[] imageBytes = (byte[])reader["Picture1"];
                                        base64Image1 = Convert.ToBase64String(imageBytes);
                                    }
                                    if (!Convert.IsDBNull(reader["Picture2"]))
                                    {
                                        byte[] imageBytes = (byte[])reader["Picture2"];
                                        base64Image2 = Convert.ToBase64String(imageBytes);
                                    }
                                    cs.ID = reader["ID"] != DBNull.Value ? Convert.ToByte(reader["ID"]) : default;
                                    cs.AboutTitle = reader["AboutTitle"]?.ToString();
                                    cs.AboutDetails1 = reader["AboutDetails1"]?.ToString();
                                    cs.AboutAdress = reader["AboutAdress"]?.ToString();
                                    cs.AboutMail = reader["AboutMail"]?.ToString();
                                    cs.AboutPhone = reader["AboutPhone"]?.ToString();
                                    cs.AboutWebSite = reader["AboutWebSite"]?.ToString();
                                    cs.AboutName = reader["AboutName"]?.ToString();
                                    cs.AboutDetails2 = reader["AboutDetails2"]?.ToString();
                                    cs.IFrameAdress = reader["IFrameAdress"]?.ToString();
                                }
                            }
                        }
                    }
                    ViewBag.Picture1 = $"data:image/jpeg;base64,{base64Image1}";
                    ViewBag.Picture2 = $"data:image/jpeg;base64,{base64Image2}";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hakkında Panelde Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hakkında Hatalı Listeleme İşlemi";
                    return RedirectToAction("Liste", "AdminHakkinda");
                }
            }
            return View(cs);
        }
        [Route("Guncelle/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            string base64Image1 = "", base64Image2 = "";
            About cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AboutGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!Convert.IsDBNull(reader["Picture1"]))
                                {
                                    byte[] imageBytes = (byte[])reader["Picture1"];
                                    base64Image1 = Convert.ToBase64String(imageBytes);
                                }
                                if (!Convert.IsDBNull(reader["Picture2"]))
                                {
                                    byte[] imageBytes = (byte[])reader["Picture2"];
                                    base64Image2 = Convert.ToBase64String(imageBytes);
                                }
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.AboutTitle = reader["AboutTitle"].ToString();
                                cs.AboutDetails1 = reader["AboutDetails1"].ToString();
                                cs.AboutAdress = reader["AboutAdress"].ToString();
                                cs.AboutMail = reader["AboutMail"].ToString();
                                cs.AboutPhone = reader["AboutPhone"].ToString();
                                cs.AboutWebSite = reader["AboutWebSite"].ToString();
                                cs.AboutName = reader["AboutName"].ToString();
                                cs.AboutDetails2 = reader["AboutDetails2"].ToString();
                                cs.IFrameAdress = reader["IFrameAdress"].ToString();
                            }
                            ViewBag.Picture1 = $"data:image/jpeg;base64,{base64Image1}";
                            ViewBag.Picture2 = $"data:image/jpeg;base64,{base64Image2}";
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hakkında Panelde Güncelleme Listesi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hakkında Hatalı Güncelleme Listeleme İşlemi";
                }
            }
            return View(cs);
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(About about)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("AboutTitle"))
                    errors["AboutTitle"] = string.Join(", ", ModelState["AboutTitle"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutDetails1"))
                    errors["AboutDetails1"] = string.Join(", ", ModelState["AboutDetails1"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutAdress"))
                    errors["AboutAdress"] = string.Join(", ", ModelState["AboutAdress"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutMail"))
                    errors["AboutMail"] = string.Join(", ", ModelState["AboutMail"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutPhone"))
                    errors["AboutPhone"] = string.Join(", ", ModelState["AboutPhone"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutWebSite"))
                    errors["AboutWebSite"] = string.Join(", ", ModelState["AboutWebSite"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutName"))
                    errors["AboutName"] = string.Join(", ", ModelState["AboutName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("AboutDetails2"))
                    errors["AboutDetails2"] = string.Join(", ", ModelState["AboutDetails2"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("IFrameAdress"))
                    errors["IFrameAdress"] = string.Join(", ", ModelState["IFrameAdress"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            byte[] imageBytes1 = null;
            byte[] imageBytes2 = null;
            if (about.Picture1 != null && about.Picture1.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await about.Picture1.CopyToAsync(ms);
                    imageBytes1 = ms.ToArray();
                }
            }
            if (about.Picture2 != null && about.Picture2.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await about.Picture2.CopyToAsync(ms);
                    imageBytes2 = ms.ToArray();
                }
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AboutUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Picture1", SqlDbType.VarBinary).Value = (object)imageBytes1 ?? DBNull.Value;
                    cmd.Parameters.Add("@Picture2", SqlDbType.VarBinary).Value = (object)imageBytes2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@AboutTitle", about.AboutTitle);
                    cmd.Parameters.AddWithValue("@AboutDetails1", about.AboutDetails1);
                    cmd.Parameters.AddWithValue("@AboutAdress", about.AboutAdress);
                    cmd.Parameters.AddWithValue("@AboutMail", about.AboutMail);
                    cmd.Parameters.AddWithValue("@AboutPhone", about.AboutPhone);
                    cmd.Parameters.AddWithValue("@AboutWebSite", about.AboutWebSite);
                    cmd.Parameters.AddWithValue("@AboutName", about.AboutName);
                    cmd.Parameters.AddWithValue("@AboutDetails2", about.AboutDetails2);
                    cmd.Parameters.AddWithValue("@IFrameAdress", about.IFrameAdress);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Hakkında Başarılı Güncelleme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hakkında Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hakkında Hatalı Güncelleme İşlemi";
                }
            }
            return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminHakkinda") });
        }
    }
}