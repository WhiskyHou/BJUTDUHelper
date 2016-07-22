using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using System.Text.RegularExpressions;
using BJUTDUHelper.BJUTDUHelperlException;

namespace BJUTDUHelper.ViewModel
{
    public class BJUTEduCenterVM : ViewModelBase
    {
        public static string Name { get; set; }
        private readonly string messageToken = "1";
        public List<Model.EduNavigationModel> EduNavigationList { get; set; } = new List<Model.EduNavigationModel>
        {
            new Model.EduNavigationModel { IconUri="ms-appx:///images//timetable.png",Name="课程表", PageType=typeof(View.BJUTEduScheduleView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//test.png",Name="考试查询",PageType=typeof(View.BJUTEduExamView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//report.png",Name="成绩查询" ,  PageType=typeof(View.BJUTEduGradeView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//school.png",Name="教室查询" }
        };

        public Model.BJUTEduCenterUserinfo BJUTEduCenterUserinfo { get; set; }
        public Service.BJUTEduCenterService _coreService;
        public Service.HttpBaseService _httpService = new Service.HttpBaseService(true);

        public BJUTEduCenterVM()
        {
            //SaveCommand = new RelayCommand<object>(Save);
            _coreService = new Service.BJUTEduCenterService();
            //OpenCheckCodeDlg = true;
            //GetCheckCode();
        }
        public async void Loaded()
        {
            LoadBasicInfo();
            GetConnectedStatus();
            GetEduTaskInfo();
        }
        public async void LoadBasicInfo()
        {
            BJUTEduCenterUserinfo = await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
            if (BJUTEduCenterUserinfo == null)
            {
                Open = true;
                Saved = new Action<object>(SaveUserinfo);
            }
        }

        private Model.EduNavigationModel ClickedItem { get; set; }
        public async void ItemClick(object o, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Model.EduNavigationModel;
            ClickedItem = item;//暂存点击数据

            if (item.PageType == null)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("这只是个饼，而且还没画完O(∩_∩)O", messageToken);
                return;
            }
            //课程表页面特殊，可无网络连接查看
            if (ClickedItem != null && ClickedItem.PageType == typeof(View.BJUTEduScheduleView))
            {
                NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, _httpService);
                return;
            }
            if (IsConnected)//
            {
                var re = await GetAuthState();
                if (re != true)
                {
                    OpenCheckCodeDlg = true;
                    CheckCodeSource=await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行

                    
                }
                else//已经认证，直接打开
                {
                    if (ClickedItem != null && ClickedItem.PageType != null)
                    {
                        NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, _httpService);
                    }
                }
                
            }
            else
            {
                if (ClickedItem != null && ClickedItem.PageType != null)
                {
                    NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, null);
                }
            }

        }

        #region 验证码框
        private string _checkCode;
        public string CheckCode
        {
            get { return _checkCode; }
            set { Set(ref _checkCode, value); }
        }
        private ImageSource _checkCodeSource;
        public ImageSource CheckCodeSource
        {
            get { return _checkCodeSource; }
            set { Set(ref _checkCodeSource, value); }
        }
        private bool _openCheckCodeDlg;
        public bool OpenCheckCodeDlg
        {
            get { return _openCheckCodeDlg; }
            set { Set(ref _openCheckCodeDlg, value); }
        }

        /// <summary>
        /// 保存验证码后登录并导航
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public async void CheckCodeSaved(object o, EventArgs e)
        {
            await Login();
            
        }
        public async void CheckCodeRefresh(object o, EventArgs e)
        { 
            var source=await _coreService.GetCheckCode(_httpService);
            CheckCodeSource = source;
        }
        #endregion
        #region 用户名密码框逻辑代码
        //控制用户名密码框的状态
        private bool _open;
        public bool Open
        {
            get { return _open; }
            set { Set(ref _open, value); }
        }

        private Action<object> _saved;
        public Action<object> Saved
        {
            get { return _saved; }
            set { Set(ref _saved, value); }
        }
        public void SaveUserinfo(object o)
        {
            var user = o as Model.UserBase;
            if (BJUTEduCenterUserinfo == null)
            {
                BJUTEduCenterUserinfo = new Model.BJUTEduCenterUserinfo();
            }

            BJUTEduCenterUserinfo.Username = user.Username;
            BJUTEduCenterUserinfo.Password = user.Password;

            try
            {
                Manager.AccountManager.SetAccount(BJUTEduCenterUserinfo);
            }
            catch (NullReferenceException nullRef)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>(nullRef.Message, messageToken);
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存失败", messageToken);
            }

        }
        #endregion

        #region 检测是否已连接至校园网
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }
        /// <summary>
        /// 获取网络状态，检测是否可以连接到校园网
        /// </summary>
        public async void GetConnectedStatus()
        {
            try
            {
                var re = await _httpService.GetResponseCode(_coreService.educenterUri);
                if (re == System.Net.HttpStatusCode.OK)
                {
                    IsConnected = true;
                }
                else
                    IsConnected = false;
            }
            catch
            {
                IsConnected = false;
            }
        }
        #endregion

        #region 检测是否已经认证过教务系统
        public async Task<bool> GetAuthState()
        {
            if (BJUTEduCenterUserinfo == null)
            {
                return false;
            }
            var re=await _httpService.GetResponseCode(_coreService.checckAuthUri + BJUTEduCenterUserinfo.Username);
            if(re== System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        #endregion

       
       
        
        public async Task Login()
        {
            try
            {
                if (BJUTEduCenterUserinfo == null)
                {
                    throw new NullRefUserinfoException("用户名密码不能为空");
                }
                Name = await _coreService.LoginEduCenter(_httpService, BJUTEduCenterUserinfo.Username, BJUTEduCenterUserinfo.Password, CheckCode);

                if (ClickedItem != null && ClickedItem.PageType != null)
                {
                    NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, _httpService);
                }

            }
            catch (NullRefUserinfoException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("请输入用户名和密码", messageToken); 
                Open = true;//提示重新输入账号
                Saved = new Action<object>(SaveUserinfo);
                Saved +=  (o) =>
                {
                    Login();
                };
            }
            catch (HttpRequestException  )
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络错误", messageToken);
            }
            catch (InvalidCheckcodeException  )
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("验证码错误", messageToken);

                OpenCheckCodeDlg = true;
                CheckCodeSource = await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行

            }
            catch (InvalidUserInfoException  )
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("用户名或密码错误", messageToken);

                Open = true;//提示重新输入账号
                Saved += SaveUserinfo;
                Saved += async (o) =>
                {
                    OpenCheckCodeDlg = true;
                    CheckCodeSource = await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行 
                };
            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(ex.Message, messageToken);
            }
        }
        

        #region 获取教学教务基础信息
        public static string Year { get; set; }
        public static int Term { get; set; }
        public static int Week { get; set; }
        public async void GetEduTaskInfo()
        {
            try
            {
                var re = await _coreService.GetEduBasicInfo(_httpService);
                Year = re.Item1;
                Term = re.Item2;
                Week = re.Item3;
            }
            catch
            {
            }
        }
        #endregion

    }
}
