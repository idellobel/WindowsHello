using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Windows.Security.Credentials;
using Windows.Storage;
using WindowsHello.Domain.Models;

namespace WindowsHello.Domain.Services.AuthService
{
    public class MockStore
    {
        private const string USER_ACCOUNT_LIST_FILE_NAME = "userAccountsList.txt";
        // This cannot be a const because the LocalFolder is accessed at runtime
        private string _userAccountListPath = Path.Combine(
            ApplicationData.Current.LocalFolder.Path, USER_ACCOUNT_LIST_FILE_NAME);
        private List<User> _mockDatabaseUserAccountsList;

        public MockStore()
        {
            _mockDatabaseUserAccountsList = new List<User>();
            LoadAccountListAsync();
        }

        private void InitializeSampleUserAccounts()
        {
            // Create a sample Traditional User Account that only has a Username and Password
            // This will be used initially to demonstrate how to migrate to use Windows Hello

            User sampleUserAccount = new User()
            {
                UserId = Guid.NewGuid(),
                Username = "voorbeeldNaam",
                Password = "voorbeeldPaswoord",
            };

            // Add the sampleUserAccount to the _mockDatabase
            _mockDatabaseUserAccountsList.Add(sampleUserAccount);
            SaveAccountListAsync();
        }

        public Guid GetUserId(string username)
        {
            if (_mockDatabaseUserAccountsList.Any())
            {
                User account = _mockDatabaseUserAccountsList.FirstOrDefault(f => f.Username.Equals(username));
                if (account != null)
                {
                    return account.UserId;
                }
            }
            return Guid.Empty;
        }

        public User GetUserAccount(Guid userId)
        {
            return _mockDatabaseUserAccountsList.FirstOrDefault(f => f.UserId.Equals(userId));
        }

        public List<User> GetUserAccountsForDevice(Guid deviceId)
        {
            List<User> usersForDevice = new List<User>();

            foreach (User account in _mockDatabaseUserAccountsList)
            {
                if (account.PassportDevices.Any(f => f.DeviceId.Equals(deviceId)))
                {
                    usersForDevice.Add(account);
                }
            }

            return usersForDevice;
        }

        public byte[] GetPublicKey(Guid userId, Guid deviceId)
        {
            User account = _mockDatabaseUserAccountsList.FirstOrDefault(f => f.UserId.Equals(userId));
            if (account != null)
            {
                if (account.PassportDevices.Any())
                {
                    return account.PassportDevices.FirstOrDefault(p => p.DeviceId.Equals(deviceId)).PublicKey;
                }
            }
            return null;
        }

        public User AddAccount(string username)
        {
            User newAccount = null;
            try
            {
                newAccount = new User()
                {
                    UserId = Guid.NewGuid(),
                    Username = username,
                };

                _mockDatabaseUserAccountsList.Add(newAccount);
                SaveAccountListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return newAccount;
        }

        public bool RemoveAccount(Guid userId)
        {
            User userAccount = GetUserAccount(userId);
            if (userAccount != null)
            {
                _mockDatabaseUserAccountsList.Remove(userAccount);
                SaveAccountListAsync();
                return true;
            }
            return false;
        }

        public bool RemoveDevice(Guid userId, Guid deviceId)
        {
            User userAccount = GetUserAccount(userId);
            PassportDevice deviceToRemove = null;
            if (userAccount != null)
            {
                foreach (PassportDevice device in userAccount.PassportDevices)
                {
                    if (device.DeviceId.Equals(deviceId))
                    {
                        deviceToRemove = device;
                        break;
                    }
                }
            }

            if (deviceToRemove != null)
            {
                //Remove the PassportDevice
                userAccount.PassportDevices.Remove(deviceToRemove);
                SaveAccountListAsync();
            }

            return true;
        }

        public void PassportUpdateDetails(Guid userId, Guid deviceId, byte[] publicKey,
                                                KeyCredentialAttestationResult keyAttestationResult)
        {
            User existingUserAccount = GetUserAccount(userId);
            if (existingUserAccount != null)
            {
                if (!existingUserAccount.PassportDevices.Any(f => f.DeviceId.Equals(deviceId)))
                {
                    existingUserAccount.PassportDevices.Add(new PassportDevice()
                    {
                        DeviceId = deviceId,
                        PublicKey = publicKey,
                        // KeyAttestationResult = keyAttestationResult
                    });
                }
            }
            SaveAccountListAsync();
        }


        #region Save and Load Helpers
        /// <summary>
        /// Create and save a useraccount list file. (Replacing the old one)
        /// </summary>
        private async void SaveAccountListAsync()
        {
            string accountsXml = SerializeAccountListToXml();

            if (File.Exists(_userAccountListPath))
            {
                StorageFile accountsFile = await StorageFile.GetFileFromPathAsync(_userAccountListPath);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }
            else
            {
                StorageFile accountsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(USER_ACCOUNT_LIST_FILE_NAME);
                await FileIO.WriteTextAsync(accountsFile, accountsXml);
            }
        }

        /// <summary>
        /// Gets the useraccount list file and deserializes it from XML to a list of useraccount objects.
        /// </summary>
        /// <returns>List of useraccount objects</returns>
        private async void LoadAccountListAsync()
        {
            if (File.Exists(_userAccountListPath))
            {
                StorageFile accountsFile = await StorageFile.GetFileFromPathAsync(_userAccountListPath);

                string accountsXml = await FileIO.ReadTextAsync(accountsFile);
                DeserializeXmlToAccountList(accountsXml);
            }

            // If the UserAccountList does not contain the sampleUser Initialize the sample users
            // This is only needed as it in a Hand on Lab to demonstrate a user migrating
            // In the real world user accounts would just be in a database
            if (!_mockDatabaseUserAccountsList.Any(f => f.Username.Equals("voorbeeldNaam")))
            {
                //If the list is empty InitializeSampleAccounts and return the list
                InitializeSampleUserAccounts();
            }
        }

        /// <summary>
        /// Uses the local list of accounts and returns an XML formatted string representing the list
        /// </summary>
        /// <returns>XML formatted list of accounts</returns>
        private string SerializeAccountListToXml()
        {
            XmlSerializer xmlizer = new XmlSerializer(typeof(List<User>));
            StringWriter writer = new StringWriter();
            xmlizer.Serialize(writer, _mockDatabaseUserAccountsList);
            return writer.ToString();
        }

        /// <summary>
        /// Takes an XML formatted string representing a list of accounts and returns a list object of accounts
        /// </summary>
        /// <param name="listAsXml">XML formatted list of accounts</param>
        /// <returns>List object of accounts</returns>
        private List<User> DeserializeXmlToAccountList(string listAsXml)
        {
            XmlSerializer xmlizer = new XmlSerializer(typeof(List<User>));
            TextReader textreader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(listAsXml)));
            return _mockDatabaseUserAccountsList = (xmlizer.Deserialize(textreader)) as List<User>;
        }
        #endregion
    }
}
