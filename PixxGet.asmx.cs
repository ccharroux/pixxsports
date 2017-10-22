using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace Pixx
{
    public struct MemberAccomplishments
    {
        public string Label;
        public int Count;
    }
    public struct SMTPProviderRecord
    { 
        public int SMTPProviderID;
        public string ProviderName;
    }
    public struct GameRecord
	{
		public string GameID;
		public string Visitor;
		public string Home;
        public string VisitorImage;
        public string HomeImage;
        public Int32 VisitorScore;
        public Int32 HomeScore;
        public string Pixx;
        public string GameTime;
	}//end GameRecord
    public struct MemberInfo
    {
        public Int32 MemberID;
        public string FirstName;
        public string LastName;
        public string EmailAddress;
        public string Password;
        public string SMTPProviderID;
        public string TextPhoneNumber;
    }
    public struct Stats
    {
        public Int32 MemberID;
        public string MemberName;
        public Int32 Wins;
        public Int32 Losses;
        public Int32 Points;
        public string Label;
        public string LabelValue;
    }
    public struct MessageCount
    {
        public Int32 Count;
    }
    public struct GameCheckRecord
    {
        public bool Inserted;
        public Int32 TotalGames;
        public Int32 TotalWins;
        public Int32 TotalLosses;
        public Int32 TotalGames_after;
        public Int32 TotalWins_after;
        public Int32 TotalLosses_after;
    }
    public struct Notification
    { 
    		public string CreateDate;
		    public int TypeID;
		    public string NotificationTitle;
		    public string NotificationMessage;
		    public int FromMemberID;
		    public string FirstName;
		    public string LastName;
    }
    public struct ResultRec
    {
        public int result;
    }
    public struct MemberPreferences
    {
        public string TypeName;
        public int TypeID;
        public bool Email;
        public bool Text;
    }
    public struct SportRecord
    {
        public int SportID;
        public string SportName;
    }

    public struct JSONResponse
    {
        public string Result;
        public string Message;
    }
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PixxGet : System.Web.Services.WebService
    {
        // Connect Work Area
        #region WorkArea
        private static string GetConfig(string key)
        {
            string sValue = "";

            try
            {
                sValue = ConfigurationManager.AppSettings[key].ToString();
                if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].Contains("localhost") == true)
                {
                    sValue = ConfigurationManager.AppSettings[key + "STG"].ToString();
                }
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
            }

            return sValue;
        }

        private static void SendErrorEmail(string Subject, string Message, string ToAddress, string ToFirstName, string ToLastName)
        {

            MailMessage mailmessage = new MailMessage();

            mailmessage.From = new MailAddress("postmaster@pixxsports.com", "PIXX Sports");

            mailmessage.To.Add(new MailAddress(ToAddress, ToFirstName + " " + ToLastName));

            mailmessage.Subject = Subject;

            mailmessage.IsBodyHtml = true;
            mailmessage.Body = Message;
            SmtpClient smtp = new SmtpClient("mail.pixxsports.com");

            NetworkCredential Credentials = new NetworkCredential("postmaster@pixxsports.com", "Moorpark62");
            smtp.Credentials = Credentials;
            smtp.Send(mailmessage); 
        
        }
        #endregion

        // Start Class
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public GameRecord[] GetPixx(string MemberID, string GameDate)
        {

            List<GameRecord> oResults = new List<GameRecord>();
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));

                DateTime d = Convert.ToDateTime(GameDate);
                string sMon = "0" + d.Month.ToString();
                sMon = sMon.Substring(sMon.Length - 2, 2);
                string sDay = "0" + d.Day.ToString();
                sDay = sDay.Substring(sDay.Length - 2, 2);

                string sDate = d.Year.ToString() + "-" + sMon + "-" + sDay;
                cmd.Parameters.Add(new MySqlParameter("inGameDate", sDate));
                cmd.Parameters.Add(new MySqlParameter("inSportID", 1));

                cmd.CommandText = "usp_get_MemberPixx";

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GameRecord record = new GameRecord();
                    record.GameID = reader["GameSourceID"].ToString();
                    record.Pixx = reader["TeamPick"].ToString();
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<GameRecord>().ToArray();
            }
        
            finally
            {
                DBCleanup(conn, reader);
            }
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Stats[] GetAllStats(int SportID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<Stats> oResults = new List<Stats>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;


                string SQL = "call usp_get_AllStats (" + SportID.ToString() + ")";

                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Stats record = new Stats();
                    record.MemberID = Convert.ToInt32(reader["MemberID"].ToString());
                    record.MemberName = reader["LastName"].ToString() + ", " + reader["FirstName"].ToString();
                    record.Wins = Convert.ToInt32(reader["Wins"].ToString());
                    record.Losses = Convert.ToInt32(reader["Losses"].ToString());
                    //record.Points = Convert.ToInt32(reader["Points"].ToString());
                    record.Label = reader["Label"].ToString();
                    record.LabelValue = reader["LabelValue"].ToString();
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<Stats>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Stats[] GetStats(int MemberID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<Stats> oResults = new List<Stats>();
            
            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inSinceIveStarted", 1));
                parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inDaysBack", 0));

                string SQL = "call usp_get_Stats (" + MemberID.ToString() + ",1,1000)";
            
                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Stats record = new Stats();
                    record.MemberID = Convert.ToInt32(reader["MemberID"].ToString());
                    record.MemberName = reader["Member"].ToString();
                    record.Wins = Convert.ToInt32(reader["Wins"].ToString());
                    record.Losses = Convert.ToInt32(reader["Losses"].ToString());
                    record.Points = Convert.ToInt32(reader["Points"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<Stats>().ToArray();
            }
            
            finally
            {
                DBCleanup(conn, reader);
            }
            

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public MemberAccomplishments[] GetMemberAccomplishments(int MemberID, int SportID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<MemberAccomplishments> oResults = new List<MemberAccomplishments>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inSportID", SportID));

                string SQL = "usp_get_accomplishments";

                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MemberAccomplishments record = new MemberAccomplishments();
                    record.Label = reader["Label"].ToString();
                    record.Count = Convert.ToInt32(reader["Count"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<MemberAccomplishments>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Notification[] GetNotifications(int MemberID)
        {

            List<Notification> oResults = new List<Notification>();
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));

                string SQL = "usp_get_MemberNotificationListSummary";

                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Notification record = new Notification();
                    record.CreateDate = reader["CreateDate"].ToString();
                    record.TypeID = 0; //Convert.ToInt32(reader["TypeID"].ToString());
                    record.NotificationTitle = reader["NotificationTitle"].ToString();
                    record.NotificationMessage = reader["NotificationMessage"].ToString();
                    record.FromMemberID = Convert.ToInt32(reader["FromMemberID"].ToString());
                    record.FirstName = (record.FromMemberID == 0 ? "" : reader["FirstName"].ToString());
                    record.LastName = (record.FromMemberID == 0 ? "" : reader["LastName"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<Notification>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Notification[] GetNotificationDetails(int MemberID, int FromMemberID)
        {

            List<Notification> oResults = new List<Notification>();
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                cmd.Parameters.Add(new MySqlParameter("inFromMemberID", FromMemberID));

                string SQL = "usp_get_MemberNotifications";

                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Notification record = new Notification();
                    record.CreateDate = reader["CreateDate"].ToString();
                    record.TypeID = 0; //Convert.ToInt32(reader["TypeID"].ToString());
                    record.NotificationMessage = reader["NotificationMessage"].ToString();
                    record.FromMemberID = Convert.ToInt32(reader["FromMemberID"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<Notification>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public GameRecord[] GetGames(string MemberID, string GameDate, string SportID)
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;            
            List<GameRecord> oResults = new List<GameRecord>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();
                
                MySqlCommand cmd;     
                cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                DateTime d = Convert.ToDateTime(GameDate);
                string sMon = "0" + d.Month.ToString();
                sMon = sMon.Substring(sMon.Length - 2, 2);
                string sDay = "0" + d.Day.ToString();
                sDay = sDay.Substring(sDay.Length - 2, 2);

                string sDate = d.Year.ToString() + "-" + sMon + "-" + sDay;
                cmd.Parameters.Add(new MySqlParameter("inGameDate", sDate));
                cmd.Parameters.Add(new MySqlParameter("inSportID", SportID));
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));

                cmd.CommandText = "usp_get_Gamelist_MemberPixx";

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GameRecord record = new GameRecord();
                    record.GameID = reader["GameSourceID"].ToString();
                    record.Visitor = reader["AwayTeam"].ToString();
                    record.Home = reader["HomeTeam"].ToString();
                    record.VisitorImage = reader["AwayTeam"].ToString() + ".gif";
                    record.HomeImage = reader["HomeTeam"].ToString() + ".gif";
                    record.VisitorScore = Convert.ToInt32(reader["AwayScore"].ToString());
                    record.HomeScore = Convert.ToInt32(reader["HomeScore"].ToString());
                    record.Pixx = reader["TeamPick"].ToString();
                    record.GameTime = reader["GameDateDisplay"].ToString();   // UTC?
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<GameRecord>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }
            

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public MemberPreferences[] GetMemberPreferences(string MemberID)
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<MemberPreferences> oResults = new List<MemberPreferences>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd;                
                MySqlParameter parm = new MySqlParameter();
                
                cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Ticket Sent for this proc
                string SQL = "usp_get_MemberMessageSetting";

                cmd.CommandText = SQL;

                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MemberPreferences record = new MemberPreferences();
                    record.TypeName = reader["TypeName"].ToString();
                    record.Text = (reader["TextMessage"] == null ? false : (reader["TextMessage"].ToString() == "1" ? true : false));
                    record.Email = (reader["EmailMessage"] == null ? false : (reader["EmailMessage"].ToString() == "1" ? true : false));
                    record.TypeID = Convert.ToInt32(reader["MemberMessageTypeID"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<MemberPreferences>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public GameCheckRecord[] InsertGame(string GameSourceID, string GameFinal, string GameDate, string HomeTeam, string AwayTeam, string HomeScore, string AwayScore, string SportID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<GameCheckRecord> oResults = new List<GameCheckRecord>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();            
                cmd.Parameters.Add(new MySqlParameter("inSportID", Convert.ToInt32(SportID)));
                cmd.Parameters.Add(new MySqlParameter("inGameSourceID", GameSourceID));

                if (GameDate.Length > 0)
                {
                    if (GameFinal.ToLower().Contains("live") == true || GameFinal.Length == 0)
                    {
                        GameFinal = GameDate;
                        GameDate = DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString();
                    }
                }

                string sDate = GenerateFullDate(GameDate);

                cmd.Parameters.Add(new MySqlParameter("inGameDate", sDate));
                cmd.Parameters.Add(new MySqlParameter("inHomeTeam", HomeTeam));
                cmd.Parameters.Add(new MySqlParameter("inAwayTeam", AwayTeam));
                cmd.Parameters.Add(new MySqlParameter("inHomeScore", (HomeScore.Length == 0 ? 0 : Convert.ToInt32(HomeScore))));
                cmd.Parameters.Add(new MySqlParameter("inAwayScore", (AwayScore.Length == 0 ? 0 : Convert.ToInt32(AwayScore))));
                cmd.Parameters.Add(new MySqlParameter("inFinal", (GameFinal.ToLower().Contains("final") == true ? true : false)));
                cmd.Parameters.Add(new MySqlParameter("inGameDisplay", GameFinal));

                cmd.CommandText = "usp_insert_Game";

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    GameCheckRecord record = new GameCheckRecord();
                    record.Inserted = (reader["Inserted"].ToString()=="0" ? false : true);
                    record.TotalWins = Convert.ToInt32((reader["TotalWins"] == null ? "0" : reader["TotalWins"].ToString()));
                    record.TotalLosses = Convert.ToInt32((reader["TotalLosses"] == null ? "0" : reader["TotalLosses"].ToString()));
                    record.TotalGames = Convert.ToInt32((reader["TotalGames"] == null ? "0" : reader["TotalGames"].ToString()));
                    record.TotalWins_after = Convert.ToInt32((reader["TotalWins_after"] == null ? "0" : reader["TotalWins_after"].ToString()));
                    record.TotalLosses_after = Convert.ToInt32((reader["TotalLosses_after"] == null ? "0" : reader["TotalLosses_after"].ToString()));
                    record.TotalGames_after = Convert.ToInt32((reader["TotalGames_after"] == null ? "0" : reader["TotalGames_after"].ToString()));
                    oResults.Add(record);
                }

                return oResults.ToArray();

            }
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<GameCheckRecord>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }
            
        }
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void InsertMemberNotification(int MemberID, int FromMemberID, int NotificationTypeID, string Title, string Message, string FromFirstName, string FromLastName)
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                cmd.Parameters.Add(new MySqlParameter("inFromMemberID", FromMemberID));
                cmd.Parameters.Add(new MySqlParameter("inNotificationType", NotificationTypeID));
                cmd.Parameters.Add(new MySqlParameter("inNotificationTitle", ""));
                cmd.Parameters.Add(new MySqlParameter("inNotificationMessage", Message));

                cmd.CommandText = "usp_insert_MemberNotification";

                reader = cmd.ExecuteReader();


                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        int ReturnCode;
                        int.TryParse(reader["ReturnCode"].ToString(), out ReturnCode);

                        if (ReturnCode > 0)
                        {
                            string FirstName = reader["FirstName"].ToString();
                            string LastName = reader["LastName"].ToString();
                            string EmailAddress = reader["EmailAddress"].ToString();
                            string Text = (reader["TextMessage"] == null ? "0" : reader["TextMessage"].ToString());
                            string Email = (reader["EmailMessage"] == null ? "0" : reader["EmailMessage"].ToString());
                            string TextAddress = (reader["TextAddress"] == null ? "" : reader["TextAddress"].ToString());
                            if (Email == "1")
                            {
                                string mSubject = FromFirstName + " " + FromLastName + " has sent you a PIXX message!";
                                string mMessage = "<p>" + FirstName + ",<br></p><p><br><strong>Subject:&nbsp;</strong>" + mSubject + "<br><br>" + Message + "</p><p><hr>To reply and make your PIXX for today's games, login to your PIXX account.<br><br>The PIXX Team</p>";
                                SendEmail(mSubject, mMessage, EmailAddress, FirstName, LastName);
                            }

                            if (Text == "1")
                            {
                                EmailAddress = TextAddress;
                                if (Message.Length > 0 && EmailAddress.Length > 0)
                                {
                                    string mMessage = Message;
                                    SendEmail("", mMessage, EmailAddress, "", "");
                                }
                            }

                        }
                    }
                }


                return;
            }
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SendMassNotification(string MessageType, string Message, bool SystemMessage, string fromValue, string toValue, string SportID)
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {
                string mSubject = "";
                string mEMessage = "";
                string mTMessage = "";

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inNotificationType", MessageType));

                cmd.CommandText = "usp_get_optedInMemberNotification";

                reader = cmd.ExecuteReader();


                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        int MemberID = Convert.ToInt32(reader["MemberID"].ToString());
                        string FirstName = reader["FirstName"].ToString();
                        string LastName = reader["LastName"].ToString();
                        string EmailAddress = reader["EmailAddress"].ToString();
                        string Text = (reader["TextMessage"] == null ? "0" : reader["TextMessage"].ToString());
                        string Email = (reader["EmailMessage"] == null ? "0" : reader["EmailMessage"].ToString());
                        string TextAddress = (reader["TextAddress"] == null ? "" : reader["TextAddress"].ToString());
                        mEMessage = "";
                        mTMessage = "";
                        
                        //Text = "1";
                        //Email = "1";

                        string TempMessage = "";
                        if (MessageType == "Games Complete")
                        {
                            TempMessage = GenerateGamesCompleteMessage(MemberID, toValue, SportID);

                            mSubject = "PIXX Games Complete";
                        }
                        else
                        {
                            TempMessage = Message;

                            //mTMessage = Message.Replace("[NAME]", FirstName);
                            //mEMessage = mTMessage;

                            if (MessageType == "New Games")
                            {
                                mSubject = "PIXX Games are up!";
                            }
                            else
                            {
                                mSubject = MessageType.ToLower().Replace("message", "") + " message from PIXX!";
                            }
                        }

                        string[] GenMessage = TempMessage.Split('|');
                        if (GenMessage.Length > 0)
                        {
                            mTMessage = GenMessage[0];
                        }
                        if (GenMessage.Length > 1)
                        {
                            mEMessage = GenMessage[1];
                        }
                        mTMessage = mTMessage.Replace("[NAME]", FirstName);
                        mEMessage = mEMessage.Replace("[NAME]", FirstName);

                        if (Email == "1" && mEMessage.Length > 0 && EmailAddress.Length > 0)
                        {
                            SendEmail(mSubject, mEMessage, EmailAddress, FirstName, LastName);
                        }

                        if (Text == "1" && mTMessage.Length > 0 && TextAddress.Length > 0)
                        {
                            SendEmail("", mTMessage, TextAddress, "", "");
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateNotificationPreferences(int MemberID, string N1, string N1E, string N1T, string N2, string N2E, string N2T, string N3, string N3E, string N3T)
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();

                MySqlParameter parm = new MySqlParameter();

                string SQL = "";

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                SQL = "usp_Delete_MemberMessageSetting";
                cmd.CommandText = SQL;
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                reader = cmd.ExecuteReader();
                reader.Close();

                cmd.CommandType = System.Data.CommandType.Text;

                SQL = "call usp_insert_MemberMessageSetting ";
                SQL = SQL + " (" + MemberID + "," + N1 + ", " + OnOff(N1T) + ", " + OnOff(N1E) + ")";
                cmd.CommandText = SQL;
                reader = cmd.ExecuteReader();
                reader.Close();

                SQL = "call usp_insert_MemberMessageSetting ";
                SQL = SQL + " (" + MemberID + "," + N2 + ", " + OnOff(N2T) + ", " + OnOff(N2E) + ")";
                cmd.CommandText = SQL;
                reader = cmd.ExecuteReader();
                reader.Close();

                SQL = "call usp_insert_MemberMessageSetting ";
                SQL = SQL + " (" + MemberID + "," + N3 + ", " + OnOff(N3T) + ", " + OnOff(N3E) + ")";
                cmd.CommandText = SQL;
                reader = cmd.ExecuteReader();

                return;

            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GeneratePoints(string GameDate, int SportID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                string sSeasonEndDate = GetSportEndingDate(SportID);

                string sDate = GenerateFullDate(GameDate);
                DateTime d;
                d = Convert.ToDateTime(sDate);
                
                bool bEndOfWeek = false;
                if (d.DayOfWeek == DayOfWeek.Saturday)
                {
                    bEndOfWeek = true;
                }

                bool bEndOfMonth = false;
                DateTime dt;
                dt = new DateTime(d.Year, d.Month, 1);
                dt = dt.AddMonths(1);
                dt = dt.AddDays(-1);
                if (dt.Day == d.Day)
                {
                    bEndOfMonth = true;
                }

                bool bEndOfSeason = false;
                // Season ends so do all periods
                DateTime dSeasonEnd;
                dSeasonEnd = Convert.ToDateTime(sSeasonEndDate);

                if (dSeasonEnd.Date == d.Date)
                {
                    bEndOfSeason = true;
                    bEndOfWeek = true;
                    bEndOfMonth = true;
                }

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "usp_update_MemberPoints";

                MySqlParameter parm = new MySqlParameter();
                //TypeOfStats varchar(10), ValueOfTime Date, inSportID int
                if (bEndOfSeason == true)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("TypeOfStats", "Season"));
                    cmd.Parameters.Add(new MySqlParameter("ValueOfTime", sDate));
                    cmd.Parameters.Add(new MySqlParameter("inSportID", SportID));
                    reader = cmd.ExecuteReader();
                    reader.Close();
                }

                if (bEndOfMonth == true)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("TypeOfStats", "Month"));
                    cmd.Parameters.Add(new MySqlParameter("ValueOfTime", sDate));
                    cmd.Parameters.Add(new MySqlParameter("inSportID", SportID));
                    reader = cmd.ExecuteReader();
                    reader.Close();
                }

                if (bEndOfWeek == true)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("TypeOfStats", "Week"));
                    cmd.Parameters.Add(new MySqlParameter("ValueOfTime", sDate));
                    cmd.Parameters.Add(new MySqlParameter("inSportID", SportID));
                    reader = cmd.ExecuteReader();
                    reader.Close();
                }

                return;
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }

        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdatePixx(string MemberID, string GameDate, string GameID, string Pixx)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", Convert.ToInt32(MemberID)));

                cmd.Parameters.Add(new MySqlParameter("inSportID", 2));

                cmd.Parameters.Add(new MySqlParameter("inGameSourceID", GameID));

                DateTime d = Convert.ToDateTime(GameDate);

                string sDate = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString();

                cmd.Parameters.Add(new MySqlParameter("inGameDate", sDate));
                cmd.Parameters.Add(new MySqlParameter("inTeamPick", Pixx));

                cmd.CommandText = "usp_insert_MemberPixx";

                reader = cmd.ExecuteReader();

                return;
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SendPassword(string EmailAddress)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inEmailAddress", EmailAddress));
                cmd.CommandText = "usp_get_MemberPassword";
                string Password = "";

                reader = cmd.ExecuteReader();

                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        Password = reader["Password"].ToString();
                    }
                }

                if (Password.Length > 0)
                {
                    SendEmail("PIXX Password", "<p>Here is your PIXX Password: " + Password + "</p>", EmailAddress, "", "");
                }
                return;
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return;
            }

            finally
            {
                DBCleanup(conn, reader);
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ResultRec[] UpdateMember(int MemberID, string FirstName, string LastName, string EmailAddress, string Password, string ProviderID, string MobileNumber)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<ResultRec> oResults = new List<ResultRec>();
            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();

                MySqlParameter parm = new MySqlParameter();

                string SQL = "";

                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));
                cmd.Parameters.Add(new MySqlParameter("inFirstName", FirstName));
                cmd.Parameters.Add(new MySqlParameter("inLastName", LastName));
                cmd.Parameters.Add(new MySqlParameter("inEmailAddress", EmailAddress));
                cmd.Parameters.Add(new MySqlParameter("inPassword", Password));
                cmd.Parameters.Add(new MySqlParameter("inProviderID", ProviderID));
                cmd.Parameters.Add(new MySqlParameter("inTextPhoneNumber", MobileNumber));

                SQL = "usp_insert_memeber";

                cmd.CommandText = SQL;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ResultRec record = new ResultRec();
                    record.result = Convert.ToInt32(reader["Result"].ToString());
                    oResults.Add(record);
                }

                return oResults.ToArray();


            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return oResults.ToArray(); 
            }

            finally
            {
                DBCleanup(conn, reader);                
                
                //HttpContext.Current.Response.Clear();
                //HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                //HttpContext.Current.Response.Write("UpdateData({})");
                //HttpContext.Current.Response.End();
            }
            
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public MemberInfo[] GetMemberID(string EmailAddress, string PassKey)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null; 
            List<MemberInfo> oResults = new List<MemberInfo>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inEmailAddress", EmailAddress));
                cmd.Parameters.Add(new MySqlParameter("inPassKey", PassKey));

                //cmd.CommandType = System.Data.CommandType;

                string SQL = "usp_get_MemberID";
                //SQL = SQL + "select MemberID, FirstName, LastName, Admin from member where EmailAddress='" + EmailAddress + "' and binary Password='" + PassKey + "' and DeleteDate is null";

                //cmd.CommandText = "usp_get_MemberID";
                cmd.CommandText = SQL;

                MemberInfo record = new MemberInfo();

                reader = cmd.ExecuteReader();

                record.EmailAddress = EmailAddress;
                record.Password = PassKey;
            
                if (reader.HasRows == false)
                {
                    record.MemberID = 0;
                    record.FirstName = "";
                    record.LastName = "";
                    record.SMTPProviderID ="";
                    record.TextPhoneNumber = "";
                    oResults.Add(record);
                }
                else
                {
                    while (reader.Read())
                    {
                        record.MemberID = Convert.ToInt32(reader["MemberID"].ToString());
                        record.FirstName = reader["FirstName"].ToString();
                        record.LastName = reader["LastName"].ToString();
                        record.SMTPProviderID = reader["SMTPProviderID"].ToString();
                        record.TextPhoneNumber = reader["TextPhoneNumber"].ToString();
                        oResults.Add(record);
                    }
                }
                
                return oResults.ToArray();
            }
            
            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<MemberInfo>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }
            
        }
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public MessageCount[] OpenMemberNotificationCount(int MemberID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<MessageCount> oResults = new List<MessageCount>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                MySqlParameter parm = new MySqlParameter();
                cmd.Parameters.Add(new MySqlParameter("inMemberID", MemberID));

                //cmd.CommandType = System.Data.CommandType;

                string SQL = "usp_get_OpenMemberNotificationCount";
                //SQL = SQL + "select MemberID, FirstName, LastName, Admin from member where EmailAddress='" + EmailAddress + "' and binary Password='" + PassKey + "' and DeleteDate is null";

                //cmd.CommandText = "usp_get_MemberID";
                cmd.CommandText = SQL;

                MessageCount record = new MessageCount();

                reader = cmd.ExecuteReader();

                if (reader.HasRows == false)
                {
                    record.Count = 0;
                    oResults.Add(record);
                }
                else
                {
                    while (reader.Read())
                    {
                        record.Count = Convert.ToInt32(reader["MessageCount"].ToString());

                        oResults.Add(record);
                    }
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<MessageCount>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }

        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public SportRecord[] GetSports()
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<SportRecord> oResults = new List<SportRecord>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd;
                cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.CommandText = "Select SportID, SportName FROM Sport WHERE DeleteDate IS NULL ORDER BY 1";

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    SportRecord record = new SportRecord();
                    record.SportID = Convert.ToInt32(reader["SportID"].ToString());
                    record.SportName = reader["SportName"].ToString();
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<SportRecord>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public SMTPProviderRecord[] GetSMTPProviders()
        {

            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<SMTPProviderRecord> oResults = new List<SMTPProviderRecord>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd;
                cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.CommandText = "usp_get_AllSMTPProviders";

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    SMTPProviderRecord record = new SMTPProviderRecord();
                    record.SMTPProviderID = Convert.ToInt32(reader["SMTPProviderID"].ToString());
                    record.ProviderName = reader["ProviderName"].ToString();
                    oResults.Add(record);
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<SMTPProviderRecord>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }


        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public MemberInfo[] GetMemberList(int MemberID)
        {
            MySqlConnection conn = new MySqlConnection();
            MySqlDataReader reader = null;
            List<MemberInfo> oResults = new List<MemberInfo>();

            try
            {

                string sConn = GetDSN();

                conn.ConnectionString = sConn;
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                //cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //MySqlParameter parm = new MySqlParameter();
                //cmd.Parameters.Add(new MySqlParameter("inEmailAddress", EmailAddress));
                //cmd.Parameters.Add(new MySqlParameter("inPassKey", PassKey));

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                string SQL = "";
                SQL = SQL + "usp_get_AllMembers";

                //cmd.CommandText = "usp_get_MemberID";
                cmd.CommandText = SQL;

                MemberInfo record = new MemberInfo();

                reader = cmd.ExecuteReader();

                if (reader.HasRows == false)
                {
                    record.MemberID = 0;
                    record.FirstName = "";
                    record.LastName = "";
                    oResults.Add(record);
                }
                else
                {
                    while (reader.Read())
                    {
                        record.MemberID = Convert.ToInt32(reader["MemberID"].ToString());
                        record.FirstName = reader["FirstName"].ToString();
                        record.LastName = reader["LastName"].ToString();
                        record.EmailAddress = reader["EmailAddress"].ToString();
                        oResults.Add(record);
                    }
                }

                return oResults.ToArray();
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return new List<MemberInfo>().ToArray();
            }

            finally
            {
                DBCleanup(conn, reader);
            }

        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        // Internal Functions
        public void SendEmail(string Subject, string Message, string ToAddress, string ToFirstName, string ToLastName)
        {

            try
            {
                MailMessage mailmessage = new MailMessage();

                mailmessage.From = new MailAddress("postmaster@pixxsports.com", "PIXX Sports");

                mailmessage.To.Add(new MailAddress(ToAddress, ToFirstName + " " + ToLastName));

                mailmessage.Subject = Subject;

                mailmessage.IsBodyHtml = true;
                mailmessage.Body = Message;
                SmtpClient smtp = new SmtpClient("mail.pixxsports.com");

                NetworkCredential Credentials = new NetworkCredential("postmaster@pixxsports.com", "Moorpark62");
                smtp.Credentials = Credentials;
                smtp.Send(mailmessage); 
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
            }
        }
        private static void DBCleanup(MySqlConnection conn, MySqlDataReader reader)
        {

            try
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        
        }
        private static string OnOff(string inStr)
        {
            return (inStr.ToLower() == "on" ? "1" : "0");
        }
        private static string GetSportEndingDate(int SportID)
        {
            string sRetDate = "2014-04-13";

            return sRetDate;
        
        }

        private static string GetDSN()
        {
            string sConn = "";

            try
            {
                sConn = ConfigurationManager.AppSettings["DatabaseConnection"].ToString();
                //if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].Contains("localhost") == true)
                //{
                //    sConn = ConfigurationManager.AppSettings["DatabaseConnection"].ToString();
                //}
                
            }

            


            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
            }

            return sConn;
        }
        private static void ErrorRtn(string ErrorMessage)
        {
            //SendErrorEmail("PIXX Error", ErrorMessage, "CarlCharroux@yahoo.com", "Carl", "Charroux");
            return;
        }
        private static string GenerateFullDate(string GameDate)
        {   
            GameDate = GameDate.Replace(" ", "");

            if (GameDate.Length == 0)
            {
                GameDate = DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString();
            }

            DateTime d;

            d = Convert.ToDateTime(GameDate + "/" + DateTime.Now.Year.ToString());

            string sMon = "0" + d.Month.ToString();
            sMon = sMon.Substring(sMon.Length - 2, 2);
            string sDay = "0" + d.Day.ToString();
            sDay = sDay.Substring(sDay.Length - 2, 2);

            string sDate = d.Year.ToString() + "-" + sMon + "-" + sDay;

            return sDate;
        }
        private string GenerateGamesCompleteMessage(int MemberID, string GameDate, string SportID)
        {
            int Wins = 0;
            int Losses = 0;
            try
            { 
                GameRecord[] oResults = GetGames(MemberID.ToString(), GameDate, SportID);
                string sTable = "";
                for (int i = 0; i < oResults.Length; i++)
                {
                    sTable = sTable + "<tr>";
                    sTable = sTable + "<td>" + oResults[i].Visitor + "</td>";
                    sTable = sTable + "<td>" + oResults[i].VisitorScore + "</td>";
                    sTable = sTable + "<td>" + oResults[i].Home + "</td>";
                    sTable = sTable + "<td>" + oResults[i].HomeScore + "</td>";

                    sTable = sTable + "<td>";

                    if (oResults[i].VisitorScore > oResults[i].HomeScore)
                    {
                        if (oResults[i].Pixx == oResults[i].Home)
                        {
                            sTable = sTable + "L";
                            Losses++;
                        }
                        else if (oResults[i].Pixx == oResults[i].Visitor)
                        {

                            sTable = sTable + "W";
                            Wins++;
                        }

                    }
                    else
                    {
                        if (oResults[i].Pixx == oResults[i].Home)
                        {
                            sTable = sTable + "W";
                            Wins++;
                        }
                        else if (oResults[i].Pixx == oResults[i].Visitor)
                        {
                            sTable = sTable + "L";
                            Losses++;
                        }
                    }

                    sTable = sTable + "</td>";
                    sTable = sTable + "</tr>";
                }

                if (sTable.Length > 0)
                {
                    //sTable = "<table border='0' cellpadding='3' cellspacing='3'>" + sTable + "</table>";
                    sTable = "<tr><td colspan='5'>Hey [NAME]- here are your PIXX results:<br/> " + Wins.ToString() + (Wins == 1 ? " Win" : " Wins") + " and " + Losses.ToString() + (Losses == 1 ? " Loss" : " Losses") + "</td></tr>" + sTable;
                    sTable = "<table border='0' cellpadding='3' cellspacing='3'>" + sTable + "</table>";
                    sTable = " Hey [NAME]- here are your PIXX results: " + Wins.ToString() + (Wins == 1 ? " Win" : " Wins") + " and " + Losses.ToString() + (Losses == 1 ? " Loss" : " Losses") + "|" + sTable;
                }

                if ((Wins + Losses) == 0)
                {
                    sTable = "";
                }

                return sTable;
            }

            catch (Exception ex)
            {
                ErrorRtn(ex.ToString());
                return "";
            }
        
        }
        // End Class
    }

}
