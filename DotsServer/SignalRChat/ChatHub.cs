using System;
using System.Text;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using SignalRChatDatabase;
using System.Linq.Expressions;


using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq.Expressions;
using SignalRChatDatabase.Models;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Security.Cryptography;

namespace SignalRChat
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }


    public class ChatHub : Hub
    {
        private static CacheItemRemovedCallback OnCacheRemove = null;
        private static bool hasTask = false;

        private void AddTask(string name, int seconds)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
            HttpRuntime.Cache.Insert(name, seconds, null,
                DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }


        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            // do stuff here if it matches our taskname, like WebRequest
            // re-add our task so it recurs
            DateTime dt = DateTime.Now;
            Clients.All.keepMeAlive();

            var turnamentsStars = context.Turnaments.Where(x => x.StartTime <= dt && x.IsSeatAndGo == false && !x.isStarted).ToList();

            foreach (var turnament in turnamentsStars)
            {
                turnament.isStarted = true;
                context.SaveChanges();
                for (int i = 0; i < turnament.Playes.Count / 2; i++)
                {
                    var player1 = turnament.Playes[i];
                    var player2 = turnament.Playes[i + 1];
                    long gameId = DateTime.Now.Ticks;
                    int rows = turnament.SizeX;
                    int cols = turnament.SizeY;
                    string amount = "0";
                    AcceptUserInvite(player1.DeviceId, player2.DeviceId, rows.ToString(), cols.ToString(), amount, turnament.Id);
                    //Clients.Client(player1.UserWebClientId).acceptInvite(player1.DeviceId, player2.DeviceId, rows, cols, gameId);
                    //Clients.Client(player2.UserWebClientId).acceptInvite(player1.DeviceId, player2.DeviceId, rows, cols, gameId);
                }
            }

            int setNextTime = 60;
            AddTask(k, setNextTime);
        }



        private readonly static ConnectionMapping<string> _connections =
          new ConnectionMapping<string>();

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;

            _connections.Add(name, Context.ConnectionId);

            Clients.Client(Context.ConnectionId).allertAMessageOnly("You are connected \n hasTask:" + hasTask + " at: " + DateTime.Now);
            Clients.Client(Context.ConnectionId).userClientId(Context.ConnectionId);
            if (!hasTask)
            {
                AddTask("test", 60);
                hasTask = true;
            }
            else if (hasTask && UserHandler.ConnectedIds.Count == 0)
            {
                hasTask = false;
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            var user = context.Users.FirstOrDefault(x => x.UserWebClientId == Context.ConnectionId);
            user.IsOnline = false;
            context.SaveChanges();
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnected();
        }

        DotsContext context = new DotsContext();
        public void Send(string userId, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(userId, message);
        }

        public void GetTurnament(string typeIdStr, string userDeviceId)
        {
            int typeId = int.Parse(typeIdStr);
            SignalRChatDatabase.TurnamentsType type = (SignalRChatDatabase.TurnamentsType)typeId;
            var openTurnaments = context.Turnaments.Where(x => x.Type == type && x.RegistationOpen == true).ToList();
            Users user = context.Users.FirstOrDefault(x => x.DeviceId == userDeviceId);

            Clients.Client(user.UserWebClientId).clearCurrentTurnamentsTab(type.ToString());

            foreach (var turnament in openTurnaments)
            {
                var turnament1 = new Turnaments(turnament);
                turnament1.Playes = new List<SignalRChatDatabase.Users>();
                bool registered = false;
                if (turnament.Playes.FirstOrDefault(x => x.DeviceId == userDeviceId) != null)
                {
                    registered = true;
                }
                Clients.Client(user.UserWebClientId).showTurnament(turnament1, type.ToString(), registered);
            }

        }

        public void RegisterTurnament(string turnamentId, string userDeviceId)
        {
            int turnamentIdInt = int.Parse(turnamentId);
            SignalRChatDatabase.Models.Turnaments turnament = context.Turnaments.FirstOrDefault(x => x.Id == turnamentIdInt);
            SignalRChatDatabase.Users user = context.Users.FirstOrDefault(x => x.DeviceId == userDeviceId);
            turnament.TakenSeats++;

            turnament.Playes.Add(user);
            context.SaveChanges();
            Clients.Client(user.UserWebClientId).registrationStatus(turnament.Id, true);
            Clients.All.turnamentStatus(turnament.Id, turnament.TakenSeats);

            if (turnament.IsSeatAndGo && turnament.TakenSeats == turnament.Seats)
            {
                for (int i = 0; i < turnament.Playes.Count / 2; i++)
                {
                    var player1 = turnament.Playes[i];
                    var player2 = turnament.Playes[i + 1];
                    long gameId = DateTime.Now.Ticks;
                    int rows = turnament.SizeX;
                    int cols = turnament.SizeY;
                    string amount = "0";
                    AcceptUserInvite(player1.DeviceId, player2.DeviceId, rows.ToString(), cols.ToString(), amount, turnament.Id );
                    //Clients.Client(player1.UserWebClientId).acceptInvite(player1.DeviceId, player2.DeviceId, rows, cols, gameId);
                    //Clients.Client(player2.UserWebClientId).acceptInvite(player1.DeviceId, player2.DeviceId, rows, cols, gameId);
                }

            }

        }

        public void UnRegisterTurnament(string turnamentId, string userDeviceId)
        {
            int turnamentIdInt = int.Parse(turnamentId);
            SignalRChatDatabase.Models.Turnaments turnament = context.Turnaments.FirstOrDefault(x => x.Id == turnamentIdInt);
            SignalRChatDatabase.Users user = context.Users.FirstOrDefault(x => x.DeviceId == userDeviceId);
            turnament.TakenSeats--;

            turnament.Playes.RemoveAll(x => x.DeviceId == user.DeviceId);
            context.SaveChanges();
            Clients.Client(user.UserWebClientId).registrationStatus(turnament.Id, false);
            Clients.All.turnamentStatus(turnament.Id, turnament.TakenSeats);
        }

        public string HashingPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(password);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);

            return BitConverter.ToString(encodedBytes);
        }

        public void RegisterUser(string userId, string username, string password, string userWebClientId, string email)
        {
            Users registerUser = new Users();
            registerUser.DeviceId = userId;
            registerUser.Username = username;
            registerUser.IsOnline = true;
            registerUser.UserWebClientId = userWebClientId;
            registerUser.Password = HashingPassword(password);
            registerUser.Email = email;
            context.Users.Add(registerUser);
            context.SaveChanges();

            var users = context.Users.Where(x => x.IsOnline).ToList();

            foreach (var user in users)
            {
                Clients.All.broadcastUsers(user.DeviceId, user.Username);
            }
        }
        public void GetUserInfo(string deviceId)
        {
            var user = context.Users.FirstOrDefault(x => x.DeviceId == deviceId);
            string userInfo = user.Id + "-" + user.DeviceId;
            Clients.Client(user.UserWebClientId).userInfo(userInfo);
        }

        public void AddAmount(string userId, string payment, string ammount) {
            SignalRChatDatabase.Users user = context.Users.FirstOrDefault(x => x.DeviceId == userId);
            decimal newAmount = decimal.Parse(ammount);
            user.Amount = user.Amount + newAmount;
            context.SaveChanges();
            Clients.Client(user.UserWebClientId).consoleLog(payment);
        }

        public void LoginUserToLobby(string userId, string username, string password, string userWebClientId)
        {
            Users logUser = new Users();
            logUser.DeviceId = userId;
            logUser.Username = username;
            logUser.IsOnline = true;
            logUser.UserWebClientId = userWebClientId;
            logUser.Password = HashingPassword(password);

            var logoutAnotherUserOnDevice = context.Users.Where(x => x.DeviceId == userId).ToList();
            foreach (var user in logoutAnotherUserOnDevice)
            {
                user.DeviceId = "";
                user.IsOnline = false;
            }
            context.SaveChanges();

            var existDevice = context.Users.FirstOrDefault(x => x.Username == username && x.Password == logUser.Password);
            if (existDevice != null)
            {
                //existDevice.Username = username;
                existDevice.DeviceId = userId;
                existDevice.UserWebClientId = userWebClientId;
                existDevice.IsOnline = true;
                context.SaveChanges();
                Clients.Client(userWebClientId).userLogIn("successful", existDevice.Username);
            }
            else
            {
                Clients.Client(userWebClientId).userLogIn("failed", "");
                return;
            }

            var users = context.Users.Where(x => x.IsOnline && x.DeviceId != userId).ToList();

            foreach (var user in users)
            {
                Clients.Client(user.UserWebClientId).broadcastUsers(logUser.DeviceId, logUser.Username);
                Clients.Client(logUser.UserWebClientId).broadcastUsers(user.DeviceId, user.Username);
            }

        }

        public void InviteUser(string myId, string invitedUserId, string rows, string cols, string amountSum)
        {
            // var me = context.Users.FirstOrDefault(x => x.DeviceId == myId);
            // var oponent = context.Users.FirstOrDefault(x => x.DeviceId == invitedUserId);
            // Clients.Client(me.UserWebClientId).inviteUser(myId, invitedUserId, rows, cols);
            // Clients.Client(oponent.UserWebClientId).inviteUser(myId, invitedUserId, rows, cols);
            if (amountSum == "") {
                amountSum = "0";
            }
             Clients.All.inviteUser(myId, invitedUserId, rows, cols, amountSum);
        }

        public void AcceptUserInvite(string myId, string invitedUserId, string rows, string cols, string amount, int turnamentid = -1)
        {

            var myUser = context.Users.FirstOrDefault(x => x.DeviceId == myId);
            var oponentUser = context.Users.FirstOrDefault(x => x.DeviceId == invitedUserId);

            DateTime dateTime = DateTime.Now;
            string gameId = dateTime.Ticks.ToString();
            GameTables gameTable = new GameTables();
            gameTable.UserInTurn = myId;
            gameTable.GameId = gameId;
            gameTable.Rows = int.Parse(rows);
            gameTable.Cols = int.Parse(cols);
            gameTable.TurnamentId = turnamentid.ToString();
            gameTable.PlayerOneId = myId;
            gameTable.PlayerTwoId = invitedUserId;//oponentUser.DeviceId;

            Decimal amountD = decimal.Parse(amount); 
            if (amountD != 0.00M)
            {
                if (myUser.Amount >= amountD &&
                    oponentUser.Amount >= amountD)
                {
                    Decimal taxes = amountD * 0.1M;
                    Decimal userAmount = amountD - taxes;
                    var adm = context.Users.FirstOrDefault(x => x.Username == "admin");
                    adm.Amount = adm.Amount + taxes * 2;
                    gameTable.Amount = amountD * 2;
                    myUser.Amount = myUser.Amount - amountD;
                    oponentUser.Amount = oponentUser.Amount - amountD;
                }
                else
                {
                    //todo some of you doesn't have enough money please charge you acc
                    if (myUser.Amount < amountD) {
                        Clients.Client(myUser.UserWebClientId).notEnoughM("You haven't enough credits. Please rechage. ");//link to recharge
                        Clients.Client(oponentUser.UserWebClientId).notEnoughM("Your oponent does't have enough credits. ");
                    }
                    if (oponentUser.Amount < amountD)
                    {
                        Clients.Client(myUser.UserWebClientId).notEnoughM("Your oponent does't have enough credits.");
                        Clients.Client(oponentUser.UserWebClientId).notEnoughM("You haven't enough credits. Please rechage. ");//link to recharge
                    }
                    return;
                }
            }
            context.GameTables.Add(gameTable);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2 * gameTable.Rows + 1; i++)
            {
                for (int j = 0; j < 2 * gameTable.Cols + 1; j++)
                {
                    string cell = i + "-" + j + "-0,";
                    sb.Append(cell);

                }
            }
            MoveTable moveRecord = new MoveTable();
            moveRecord.TableId = gameId;
            moveRecord.UserId = myId;
            sb.Length--;
            moveRecord.Board = sb.ToString();
            moveRecord.TimeMove = DateTime.Now;
            context.MoveTables.Add(moveRecord);
            context.SaveChanges();
            //Clients.All.ConsoleLog("21 => " + gameTable.PlayerOneId + " " + gameTable.PlayerTwoId);

            Clients.All.acceptInvite(myId, invitedUserId, rows, cols, gameId);
        }

        public void MakeMove(string myId, string oponent, string tableId, string lineId)
        {
            DateTime dateTime = DateTime.Now;
            var table = context.GameTables.FirstOrDefault(x => x.GameId == tableId);

            var meUserPf = context.Users.FirstOrDefault(x => x.DeviceId == myId);
            var oponentUserPf = context.Users.FirstOrDefault(x => x.DeviceId == oponent);
            Clients.All.consoleLog(table.UserInTurn + " and " + myId);
            if (table.UserInTurn == myId)
            {
                DrawLine(tableId, lineId, oponent, myId);
                Clients.Client(meUserPf.UserWebClientId).drawLine(myId, oponent, tableId, lineId);
                Clients.Client(oponentUserPf.UserWebClientId).drawLine(myId, oponent, tableId, lineId);
            }
            else
            {
                Clients.Client(meUserPf.UserWebClientId).oponentOnTurn(myId);
            }
        }

        public void LogOut(string deviceId)
        {
            var user = context.Users.First(x => x.DeviceId == deviceId);
            user.IsOnline = false;
            context.SaveChanges();
        }

        public bool DrawLine(string tblId, string cellId, string oponentId, string myId)
        {
            var gt = context.GameTables.FirstOrDefault(x => x.GameId == tblId);

            var lastBoardInfoList = context.MoveTables.Where(x => x.TableId == gt.GameId).ToList();
            var lastBoardInfo = lastBoardInfoList[lastBoardInfoList.Count - 1];

            var tableCellStrings = lastBoardInfo.Board.Split(',');
            //Clients.All.allertAMessage(gt.UserInTurn, oponentId, tableCellStrings.Length);

            List<CellObj> tableCellObj = new List<CellObj>();

            foreach (var item in tableCellStrings)
            {
                CellObj obj = new CellObj(item);
                tableCellObj.Add(obj);
            }

            CellObj currentObj = new CellObj(cellId);
            currentObj.Taken = true;

            StringBuilder newBoardState = new StringBuilder();
            foreach (var item in tableCellObj)
            {
                if (item.Row == currentObj.Row && item.Col == currentObj.Col)
                {

                    newBoardState.Append(currentObj.ToString());
                }
                else
                {
                    newBoardState.Append(item.ToString());

                }
                newBoardState.Append(',');
            }
            newBoardState.Length--;

            NewState(cellId, gt.GameId, gt.UserInTurn, newBoardState);

            var hasCloseBox = CloseBox(currentObj, tableCellObj, newBoardState, gt.GameId, gt.UserInTurn);

            if (hasCloseBox.IsClose == false)
            {
                gt.UserInTurn = oponentId;
            }
            else
            {
                var meUserPf = context.Users.FirstOrDefault(x => x.DeviceId == myId);
                var oponentUserPf = context.Users.FirstOrDefault(x => x.DeviceId == oponentId);
                if (hasCloseBox.RowUp != 0 && hasCloseBox.ColUp != 0)
                {
                    string boxId = hasCloseBox.RowUp + "-" + hasCloseBox.ColUp;
                    Clients.Client(meUserPf.UserWebClientId).colorBox(myId, oponentId, tblId, boxId);
                    Clients.Client(oponentUserPf.UserWebClientId).colorBox(myId, oponentId, tblId, boxId);
                    if (gt.UserInTurn == gt.PlayerOneId)
                    {
                        gt.PlayerOneBoxes++;
                    }
                    else if (gt.UserInTurn == gt.PlayerTwoId)
                    {
                        gt.PlayerTwoBoxes++;
                    }
                }
                if (hasCloseBox.RowDown != 0 && hasCloseBox.ColDown != 0)
                {
                    string boxId = hasCloseBox.RowDown + "-" + hasCloseBox.ColDown;
                    Clients.Client(meUserPf.UserWebClientId).colorBox(myId, oponentId, tblId, boxId);
                    Clients.Client(oponentUserPf.UserWebClientId).colorBox(myId, oponentId, tblId, boxId);
                    if (gt.UserInTurn == gt.PlayerOneId)
                    {
                        gt.PlayerOneBoxes++;
                    }
                    else if (gt.UserInTurn == gt.PlayerTwoId)
                    {
                        gt.PlayerTwoBoxes++;
                    }
                }


                if(gt.PlayerOneBoxes > (gt.Rows * gt.Cols) / 2)
                {
                    Clients.Client(meUserPf.UserWebClientId).gameOver("You won a game");
                    Clients.Client(oponentUserPf.UserWebClientId).gameOver("You lost a game");
                }
                else if (gt.PlayerTwoBoxes > (gt.Rows * gt.Cols) / 2)
                {
                    Clients.Client(meUserPf.UserWebClientId).gameOver("You won a game");
                    Clients.Client(oponentUserPf.UserWebClientId).gameOver("You lost a game");
                }
            }
            context.SaveChanges();
            return true;

        }

        private void NewState(string cellId, string gameId, string userInTurn, StringBuilder newBoardState)
        {
            MoveTable nmm = new MoveTable();
            nmm.TableId = gameId;
            nmm.UserId = userInTurn;
            nmm.Board = newBoardState.ToString();
            nmm.CellTaken = cellId;
            nmm.TimeMove = DateTime.Now;
            context.MoveTables.Add(nmm);
            context.SaveChanges();
        }

        public void IsInGame(string gameId,string myId)
        {
            //Clients.All.consoleLog(gameId + " " + myId);
            //var gameSelected = context.MoveTables.LastOrDefault(x => x.TableId == gameId);
            //DateTime dt = DateTime.Now;
            Clients.All.consoleLog("lasr");
           
            //Clients.All.consoleLog(game);
            var move = context.MoveTables.Where(x => x.TableId == gameId).ToList();
            //Clients.All.consoleLog(move[move.Count-1].TimeMove);
            //Clients.All.consoleLog(gameSelected.TimeMove);

            var gg = move[move.Count - 1];
            DateTime dt = DateTime.Now;
            TimeSpan diff = (dt - gg.TimeMove);
            if (diff.Seconds >=30)
            {
                var myUser = context.Users.FirstOrDefault(x => x.DeviceId == myId);
                var opponent = context.Users.FirstOrDefault(x => x.DeviceId == gg.UserId);
                var gameTable = context.GameTables.FirstOrDefault(x => x.GameId == gameId);
                myUser.Amount = myUser.Amount + gameTable.Amount;
                gameTable.Paid = true;
                context.SaveChanges();
                Clients.Client(myUser.UserWebClientId).gameOver("You won a game");
                Clients.Client(opponent.UserWebClientId).gameOver("You lost a game");
           
            }
        }

        public void GetUserFullInfo(string deviceId, string username)
        {
            var user = context.Users.FirstOrDefault(x => x.DeviceId == deviceId && x.Username == username);
            user.Password = "";
            Clients.Client(user.UserWebClientId).getUserFullInfo(user);
        }

        public CloseBox CloseBox(CellObj cellObj, List<CellObj> tableComponentsObj, StringBuilder boardState, string gameId, string userId)
        {
            string lastCellId = tableComponentsObj[tableComponentsObj.Count - 1].Id;
            CellObj lastCell = new CellObj(lastCellId);
            lastCell.Row--;
            lastCell.Col--;
            CloseBox closed = new CloseBox(false);

            if (cellObj.IsHorizontal)
            {
                if (cellObj.Row == 0)
                {
                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 2 && x.Col == cellObj.Col && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col - 1 && x.Taken);// (cellObj.Row + 1) + "_" + (cellObj.Col - 1);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col + 1 && x.Taken);// (cellObj.Row + 1) + "_" + (col + 1);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row + 1;
                        closed.ColUp = cellObj.Col;
                        return closed;
                    }
                    return closed;
                }
                else if (cellObj.Row == lastCell.Row)
                {

                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 2 && x.Col == cellObj.Col && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row - 1;
                        closed.ColUp = cellObj.Col;
                        return closed;
                    }
                    return closed;
                }
                else
                {

                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 2 && x.Col == cellObj.Col && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row - 1;
                        closed.ColUp = cellObj.Col;
                    }

                    isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 2 && x.Col == cellObj.Col && x.Taken);
                    isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowDown = cellObj.Row + 1;
                        closed.ColDown = cellObj.Col;
                        return closed;
                    }
                    return closed;

                }
            }
            else
            {
                if (cellObj.Col == 0)
                {

                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row && x.Col == cellObj.Col + 2 && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row;
                        closed.ColUp = cellObj.Col + 1;
                        return closed;
                    }
                    return closed;
                }
                else if (cellObj.Col == lastCell.Col)
                {
                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row && x.Col == cellObj.Col - 2 && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row;
                        closed.ColUp = cellObj.Col - 1;
                        return closed;
                    }
                    return closed;
                }
                else
                {
                    var isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row && x.Col == cellObj.Col - 2 && x.Taken);
                    var isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    var isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col - 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {

                        closed.IsClose = true;
                        closed.RowUp = cellObj.Row;
                        closed.ColUp = cellObj.Col - 1;
                    }

                    isTakenUp = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row && x.Col == cellObj.Col + 2 && x.Taken);
                    isTakenLeft = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row - 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    isTakenRight = tableComponentsObj.FirstOrDefault(x => x.Row == cellObj.Row + 1 && x.Col == cellObj.Col + 1 && x.Taken);
                    if (isTakenUp != null && isTakenLeft != null && isTakenRight != null)
                    {
                        closed.IsClose = true;
                        closed.RowDown = cellObj.Row;
                        closed.ColDown = cellObj.Col + 1;
                    }
                    return closed;

                }
            }
        }


    }
}