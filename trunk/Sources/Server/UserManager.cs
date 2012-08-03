using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jukebox.Server.Models;
using System.Timers;

namespace Jukebox.Server
{
    class UserManager
    {
        public UserManager()
        {
            Instance = this;

            userTimer.Interval = Config.GetInstance().ActionPointsRestoreTime;
            userTimer.AutoReset = true;
            userTimer.Elapsed += new ElapsedEventHandler(userTimer_Elapsed);
            userTimer.Start();

            prevUpdateTime = new TimeSpan(DateTime.Now.Ticks);
        }

        void userTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            prevUpdateTime = new TimeSpan(DateTime.Now.Ticks);
            RestorePoints();
        }

        public void RestorePoints()
        {
            foreach (User user in userDictionary.Keys)
            {
                if (user.ActionPoints < Config.GetInstance().InitialActionPoints)
                {
                    userDictionary.Where(x => x.Key.UserIpAddress == user.UserIpAddress).First().Key.ActionPoints = Config.GetInstance().InitialActionPoints;
                }
                userDictionary.Where(x => x.Key.UserIpAddress == user.UserIpAddress).First().Key.VolumeLevelDiffs = 0;
            }
        }

        public TimeSpan GetTimeForNextRestore()
        {
            int actionPointsRestoreTime = Config.GetInstance().ActionPointsRestoreTime;
            int hour = actionPointsRestoreTime / (60 * 60 * 1000);
            actionPointsRestoreTime -= hour * (60 * 60 * 1000);
            int minute = actionPointsRestoreTime / (60 * 1000);
            actionPointsRestoreTime -= minute * (60 * 1000);
            int second = actionPointsRestoreTime / (1000);
            actionPointsRestoreTime -= second * 1000;
            int milliseconds = actionPointsRestoreTime % 1000;

            return (new TimeSpan(hour, minute, second) - (new TimeSpan(DateTime.Now.Ticks) - prevUpdateTime));
        }

        public void UserPerformAction(string userAddress, ActionType actionType)
        {
            User tempUser = new User() { UserIpAddress = userAddress};
            if (!userDictionary.ContainsKey(tempUser))
            {
                AddNewUser(tempUser);
            }
            userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.ActionPoints--;
        }

        public void UserChangeValueAction(string userAddress, ActionType actionType, object prevValue, object newValue)
        {
            User tempUser = new User() { UserIpAddress = userAddress };
            if (!userDictionary.ContainsKey(tempUser))
            {
                AddNewUser(tempUser);
            }

            if (actionType == ActionType.VolumeChangeAction)
            {
                double prevVolumeLevel = Convert.ToDouble(prevValue);
                double newVolumeLevel = Convert.ToDouble(newValue);

                double prevVolumeLevelDiffs = userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.VolumeLevelDiffs;
                int prevIntValue = (int)Math.Floor(prevVolumeLevelDiffs);
                double newVolumeLevelDiffs = prevVolumeLevelDiffs + Math.Abs(newVolumeLevel - prevVolumeLevel);
                int newIntValue = (int)Math.Floor(newVolumeLevelDiffs);

                userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.VolumeLevelDiffs = newVolumeLevelDiffs;
                if (newIntValue != 0 && prevIntValue != newIntValue && newIntValue % 4 == 0)
                {
                    userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.ActionPoints--;
                }
            }
        }

        public void AddActionPoints(string userAddress, int countOfPoints)
        {
            User tempUser = new User() { UserIpAddress = userAddress };
            if (!userDictionary.ContainsKey(tempUser))
            {
                AddNewUser(tempUser);
            }

            userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.ActionPoints += countOfPoints;
        }

        private void AddNewUser(User user)
        {
            if (!userDictionary.ContainsKey(user))
            {
                user.ActionPoints = Config.GetInstance().InitialActionPoints;
                user.VolumeLevelDiffs = 0;
                userDictionary.Add(user, true);
            }
        }

        public bool CanUserPerformAction(string userAddress)
        {
            User tempUser = new User() { UserIpAddress = userAddress };
            if (!userDictionary.ContainsKey(tempUser))
            {
                AddNewUser(tempUser);
                return true;
            }
            return userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key.ActionPoints > 0;
        }

        public User GetUserInfo(string userAddress)
        {
            User tempUser = new User() { UserIpAddress = userAddress };
            if (!userDictionary.ContainsKey(tempUser))
            {
                AddNewUser(tempUser);
            }
            return userDictionary.Where(x => x.Key.UserIpAddress == userAddress).First().Key;
        }

        TimeSpan prevUpdateTime;

        Timer userTimer = new Timer();

        Dictionary<User, bool> userDictionary = new Dictionary<User, bool>();

        public static UserManager Instance { get; private set; }
    }
}
