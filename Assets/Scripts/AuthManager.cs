using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class AuthManager : Singleton<AuthManager>
{
    [Header("Firebase")]
    public FirebaseAuth auth; //���� ���� ��ü
    public FirebaseUser User; //�����
    public DatabaseReference DBref; //�����ͺ��̽� �ν��Ͻ�

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    [Header("Register")]
    public TMP_InputField emailRegisterField;
    public TMP_InputField userNameRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordCheckRegisterField;
    public TMP_Text warningRegisterText;

    [Header("ChangePassword")]
    public TMP_InputField passwordChangeField;
    public TMP_InputField passwordCheckChangeField;
    public TMP_Text warningPasswordText;



    public TMP_Text userNameText;
    private string strWeather;
    private string strLastLogin;
    public TMP_Text friendTxt;

    private void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                auth = FirebaseAuth.DefaultInstance;
                DBref = FirebaseDatabase.DefaultInstance.RootReference;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    private IEnumerator Register(string email, string password, string userName)
    {
        if (userName == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordCheckRegisterField.text)
        {
            warningRegisterText.text = "Password does not Match!";
        }
        else
        {
            var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to Register:{task.Exception}");
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorcode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorcode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;

            }
            else
            {
                User = task.Result.User;
                if (User != null)
                {
                    FirebaseUser user = auth.CurrentUser;
                    if (user != null)
                    {
                        UserProfile profile = new UserProfile { DisplayName = userName };
                        var profileTask = user.UpdateUserProfileAsync(profile);
                        yield return new WaitUntil(() => profileTask.IsCompleted);
                        if (profileTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register:{profileTask.Exception}");
                            FirebaseException profileEx = profileTask.Exception.GetBaseException() as FirebaseException;
                            AuthError profileErrorCode = (AuthError)profileEx.ErrorCode;
                            warningRegisterText.text = "Username Set Failed!";
                        }
                        else
                        {
                            UIManager.Instance.LoginPanel();
                            Debug.Log("User Profile Updated Successfully");
                            warningRegisterText.text = "";
                            StartCoroutine(SaveUserName());
                            StartCoroutine(SaveRewardData());
                        }
                    }
                }

            }

        }

    }

    public void OnRegister()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, userNameRegisterField.text));
    }

    private IEnumerator Login(string email, string password)
    {
        var task = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to Login:{task.Exception}");
            FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
            AuthError errorcode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorcode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not Exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            User = task.Result.User;
            Debug.Log($"User Signed in Successfully: {User.Email}, {User.DisplayName}");
            warningLoginText.text = "";

            //�� ����� �� ���� �̺�Ʈ ȣ��
            DBref.Child("users").Child(User.UserId).Child("LastLogin").ValueChanged += LoadLastLogin;

            UIManager.Instance.CloseLogin();
            StartCoroutine(LoadUserName());
            StartCoroutine(SaveLoginData());
            StartCoroutine(LoadWeather());
        }
    }

    private void LoadLastLogin(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }
        else
        {
            DBref.Child("users").Child(User.UserId).Child("RewardLogin")
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        if (snapshot != null && snapshot.Value != null)
                        {
                            strLastLogin = snapshot.Value.ToString();
                            Debug.Log($"Reward Login :{strLastLogin}");
                        }
                    }
                });
        }
    }


    private int consecutiveRewards = 0; // ���� �α��� Ƚ���� ������ ����
    public void OnRewardButton()
    {
        string date = GetNow();
        if (long.Parse(date.Substring(0, 12)) - long.Parse(strLastLogin.Substring(0, 12)) > 0)
        {
            if (long.Parse(date.Substring(0, 12)) - long.Parse(strLastLogin.Substring(0, 12)) == 1)
            {
                consecutiveRewards++;
                strLastLogin = date;
                DBref.Child("users").Child(User.UserId).Child("RewardLogin").SetValueAsync(date)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log($"Reward LoginDate Updated:{date}");
                        }
                    });
                Debug.Log($"{consecutiveRewards}���� ���� ����");
            }
            else if (long.Parse(date.Substring(0, 12)) - long.Parse(strLastLogin.Substring(0, 12)) > 1)
            {
                consecutiveRewards = 1;
                strLastLogin = date;
                DBref.Child("users").Child(User.UserId).Child("RewardLogin").SetValueAsync(date)
                    .ContinueWith(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log($"Reward LoginDate Updated:{date}");
                            Debug.Log($"{consecutiveRewards}���� ���� ����");
                        }
                    });
                Debug.Log($"���� ���� �ʱ�ȭ");
            }
        }
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator SaveUserName()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("UserName")
            .SetValueAsync(userNameRegisterField.text);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask.Exception}");
        }
        else Debug.Log("Save Completed");
    }

    private IEnumerator LoadUserName()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("UserName")
            .GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Load Task failed with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            Debug.Log("Load Completed");
            userNameText.text = $"Username: {snapshot.Value}";
        }
    }

    public IEnumerator LoadWeather()
    {
        var DBTask = DBref.Child("Weather").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Load Task failed with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            if (snapshot != null && snapshot.Value != null)
            {
                Debug.Log("Load Completed");
                strWeather = snapshot.Value.ToString();
            }
        }
        UIManager.Instance.StartGame();
    }

    public string GetWeather()
    {
        return strWeather;
    }

    //���� ȸ������ ����� ���� �ʱ�ȭ
    private IEnumerator SaveRewardData()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("RewardLogin")
            .SetValueAsync("00000000000000");
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to save task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Reward Data Initailized");
        }
    }

    public string GetNow()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }
    private IEnumerator SaveLoginData()
    {
        string currentDateTime = GetNow();
        var DBTask = DBref.Child("users").Child(User.UserId).Child("LastLogin").SetValueAsync(currentDateTime);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to Save task with {DBTask.Exception}");

        }
        else
        {
            Debug.Log("Login Date update: " + currentDateTime);
        }
    }
    public void Change()
    {
        StartCoroutine(ChangePassword(passwordChangeField.text));
    }

    private IEnumerator ChangePassword(string newPassword)
    {
        if (passwordChangeField.text != passwordCheckChangeField.text)
        {
            warningPasswordText.text = "Password does not Match!";
        }
        else
        {
            FirebaseUser user = auth.CurrentUser;
            var task = user.UpdatePasswordAsync(newPassword);

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogWarning($"Failed to change password: {task.Exception}");
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Password change failed!";
                switch (errorCode)
                {
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    default:
                        // Handle other error cases if needed
                        break;
                }
                warningPasswordText.text = message;

                // Show error message to the user
                // For example: warningChangePasswordText.text = message;
            }
            else
            {
                Debug.Log("Password changed successfully!");
                // Password changed successfully, you can handle this case accordingly
            }
        }
    }

    public void LoadUser()
    {
        StartCoroutine(LoadFriend());
    }

    IEnumerator LoadFriend()
    {
        var DBTask = DBref.Child("users").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save task failed with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            friendTxt.text = "";
            foreach (var snap in snapshot.Children)
            {
                IDictionary data = (IDictionary)snap.Value;

                string userId = snap.Key;
                //string lastLogin = data["LastLogin"].ToString();
                string userName = data["UserName"].ToString();

                Debug.Log($"{userName} User");
                friendTxt.text += $"{userName} user \n";
            }

            Debug.Log("����");
        }
    }

    public void AddFriend()
    {
        StartCoroutine(AddToFriend());
    }

    IEnumerator AddToFriend()
    {
        var DBTask = DBref.Child("users").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        var DBTasks = DBref.Child("users").Child(User.UserId).Child("UserName").GetValueAsync();
        yield return new WaitUntil(() => DBTasks.IsCompleted);

        List<string> friends = new List<string>();
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save task failed with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            DataSnapshot snapshot2 = DBTasks.Result;
            foreach (var snap in snapshot.Children)
            {
                IDictionary data = (IDictionary)snap.Value;

                string userId = snap.Key;
                //string lastLogin = data["LastLogin"].ToString();
                string userName = data["UserName"].ToString();
                if ((string)snapshot2.Value != userName)
                {
                    friends.Add(userName);
                }
            }
            var dbtask = DBref.Child("users").Child(User.UserId).Child("Friends").SetValueAsync(friends);
            yield return new WaitUntil(() => dbtask.IsCompleted);
            if (dbtask.Exception != null)
            {
                Debug.LogWarning($"Save task failed with {dbtask.Exception}");
            }
            else
            {
                print("ģ�� �߰�");
            }
            Debug.Log("����");
        }
    }
}
